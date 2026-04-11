package moe.osudroid.service.beatmap;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import okhttp3.HttpUrl;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.concurrent.TimeUnit;

public final class OsuDirectBeatmapLibraryService implements BeatmapLibraryService {
    private static final String SEARCH_BASE_URL = "https://osu.direct/api/v2/search";
    private static final long CACHE_WINDOW_MS = 60_000L;

    private final OkHttpClient client;
    private final boolean onlineFetchEnabled;
    private final String songsRoot;

    private List<BeatmapCard> cachedOnlineBeatmaps;
    private long nextRefreshEpochMs;
    private String lastOnlineQuery;
    private String onlineStatus;

    public OsuDirectBeatmapLibraryService() {
        this(defaultClient(), true, defaultSongsRoot());
    }

    public OsuDirectBeatmapLibraryService(boolean onlineFetchEnabled) {
        this(defaultClient(), onlineFetchEnabled, defaultSongsRoot());
    }

    public OsuDirectBeatmapLibraryService(String songsRoot, boolean onlineFetchEnabled) {
        this(defaultClient(), onlineFetchEnabled, songsRoot);
    }

    OsuDirectBeatmapLibraryService(OkHttpClient client, boolean onlineFetchEnabled, String songsRoot) {
        this.client = client;
        this.onlineFetchEnabled = onlineFetchEnabled;
        this.songsRoot = songsRoot;
        this.cachedOnlineBeatmaps = offlineFallback();
        this.nextRefreshEpochMs = 0L;
        this.lastOnlineQuery = "";
        this.onlineStatus = onlineFetchEnabled
                ? "Online discovery idle."
                : "Online discovery disabled for deterministic tests.";
    }

    @Override
    public synchronized List<BeatmapCard> installedBeatmaps() {
        File root = new File(songsRoot);
        if (!root.exists() || !root.isDirectory()) {
            return Collections.emptyList();
        }

        File[] entries = root.listFiles();
        if (entries == null) {
            return Collections.emptyList();
        }

        List<BeatmapCard> beatmaps = new ArrayList<BeatmapCard>();
        for (File entry : entries) {
            if (!entry.isDirectory()) {
                continue;
            }
            scanBeatmapSet(entry, beatmaps);
        }

        Collections.sort(beatmaps, new Comparator<BeatmapCard>() {
            @Override
            public int compare(BeatmapCard left, BeatmapCard right) {
                int titleCompare = left.getTitle().compareToIgnoreCase(right.getTitle());
                if (titleCompare != 0) {
                    return titleCompare;
                }
                return left.getDifficultyName().compareToIgnoreCase(right.getDifficultyName());
            }
        });
        return Collections.unmodifiableList(beatmaps);
    }

    @Override
    public synchronized List<BeatmapCard> searchOnline(String query) {
        String safeQuery = query == null ? "" : query.trim();
        long now = System.currentTimeMillis();
        if (safeQuery.equals(lastOnlineQuery) && now < nextRefreshEpochMs) {
            return cachedOnlineBeatmaps;
        }

        if (onlineFetchEnabled) {
            List<BeatmapCard> fetched = fetchFromOsuDirect(safeQuery);
            if (!fetched.isEmpty()) {
                cachedOnlineBeatmaps = fetched;
                onlineStatus = "Loaded " + fetched.size() + " beatmap sets from osu.direct.";
            } else {
                onlineStatus = "No online beatmaps matched the current query.";
            }
        } else {
            onlineStatus = "Online discovery disabled for deterministic tests.";
        }

        lastOnlineQuery = safeQuery;
        nextRefreshEpochMs = now + CACHE_WINDOW_MS;
        return cachedOnlineBeatmaps;
    }

    @Override
    public String getSongsRoot() {
        return songsRoot;
    }

    @Override
    public String getOnlineStatus() {
        return onlineStatus;
    }

    private List<BeatmapCard> fetchFromOsuDirect(String query) {
        HttpUrl url = HttpUrl.parse(SEARCH_BASE_URL).newBuilder()
                .addQueryParameter("sort", "ranked_date:desc")
                .addQueryParameter("mode", "0")
                .addQueryParameter("query", query)
                .addQueryParameter("offset", "0")
                .addQueryParameter("amount", "12")
                .build();
        Request request = new Request.Builder()
                .url(url)
                .addHeader("User-Agent", "osu!droid rewrite core")
                .build();

        try (Response response = client.newCall(request).execute()) {
            if (!response.isSuccessful() || response.body() == null) {
                onlineStatus = "Online discovery failed with HTTP " + response.code() + ".";
                return Collections.emptyList();
            }

            String body = response.body().string();
            JsonElement parsed = JsonParser.parseString(body);
            if (!parsed.isJsonArray()) {
                return Collections.emptyList();
            }

            List<BeatmapCard> cards = parseSearchResponse(parsed.getAsJsonArray());
            if (cards.isEmpty()) {
                return Collections.emptyList();
            }
            return Collections.unmodifiableList(cards);
        } catch (IOException | RuntimeException e) {
            onlineStatus = "Online discovery unavailable: " + e.getMessage();
            return Collections.emptyList();
        }
    }

    private static List<BeatmapCard> parseSearchResponse(JsonArray beatmapSets) {
        List<BeatmapCard> cards = new ArrayList<BeatmapCard>(beatmapSets.size());
        for (JsonElement setElement : beatmapSets) {
            if (!setElement.isJsonObject()) {
                continue;
            }
            JsonObject set = setElement.getAsJsonObject();
            JsonArray beatmaps = getArray(set, "beatmaps");
            JsonObject representative = selectRepresentativeBeatmap(beatmaps);
            if (representative == null) {
                continue;
            }

            long setId = getLong(set, "id");
            long beatmapId = getLong(representative, "id");
            String title = preferredText(set, "title_unicode", "title");
            String artist = preferredText(set, "artist_unicode", "artist");
            String mapper = getString(set, "creator");
            float stars = (float) getDouble(representative, "difficulty_rating");
            int bpm = (int) Math.round(getDouble(representative, "bpm"));
            String status = rankedStatusLabel((int) getLong(set, "ranked"));

            cards.add(new BeatmapCard(
                    setId,
                    beatmapId,
                    title,
                    artist,
                    mapper,
                    getString(representative, "version"),
                    stars,
                    bpm,
                    status,
                    "osu.direct",
                    beatmapId > 0L ? "https://osu.direct/api/media/preview/" + beatmapId : "",
                    setId > 0L ? "https://osu.direct/api/d/" + setId : "",
                    ""));
        }
        return cards;
    }

    private static void scanBeatmapSet(File beatmapSetDirectory, List<BeatmapCard> sink) {
        File[] entries = beatmapSetDirectory.listFiles();
        if (entries == null) {
            return;
        }
        for (File entry : entries) {
            if (!entry.isFile() || !entry.getName().toLowerCase(Locale.ROOT).endsWith(".osu")) {
                continue;
            }
            BeatmapCard parsed = parseBeatmapFile(entry);
            if (parsed != null) {
                sink.add(parsed);
            }
        }
    }

    private static BeatmapCard parseBeatmapFile(File beatmapFile) {
        Map<String, String> fields = new LinkedHashMap<String, String>();
        BufferedReader reader = null;
        try {
            reader = new BufferedReader(new FileReader(beatmapFile));
            String line;
            while ((line = reader.readLine()) != null) {
                int separatorIndex = line.indexOf(':');
                if (separatorIndex <= 0) {
                    continue;
                }
                fields.put(line.substring(0, separatorIndex).trim(), line.substring(separatorIndex + 1).trim());
            }
        } catch (IOException e) {
            return null;
        } finally {
            if (reader != null) {
                try {
                    reader.close();
                } catch (IOException ignored) {
                }
            }
        }

        String title = firstNonBlank(fields.get("TitleUnicode"), fields.get("Title"), beatmapFile.getParentFile().getName());
        String artist = firstNonBlank(fields.get("ArtistUnicode"), fields.get("Artist"), "Unknown Artist");
        String mapper = firstNonBlank(fields.get("Creator"), "Unknown Mapper");
        String difficultyName = firstNonBlank(fields.get("Version"), "Normal");
        return new BeatmapCard(
                parseLong(fields.get("BeatmapSetID")),
                parseLong(fields.get("BeatmapID")),
                title,
                artist,
                mapper,
                difficultyName,
                parseFloat(fields.get("OverallDifficulty")),
                Math.round(parseFloat(fields.get("BPM"))),
                "local",
                "local",
                "",
                "",
                beatmapFile.getAbsolutePath());
    }

    private static JsonObject selectRepresentativeBeatmap(JsonArray beatmaps) {
        if (beatmaps == null || beatmaps.size() == 0) {
            return null;
        }
        JsonObject best = null;
        double bestStars = Double.NEGATIVE_INFINITY;
        for (JsonElement beatmap : beatmaps) {
            if (!beatmap.isJsonObject()) {
                continue;
            }
            JsonObject beatmapObj = beatmap.getAsJsonObject();
            double stars = getDouble(beatmapObj, "difficulty_rating");
            if (best == null || stars > bestStars) {
                best = beatmapObj;
                bestStars = stars;
            }
        }
        return best;
    }

    private static JsonArray getArray(JsonObject object, String key) {
        JsonElement element = object.get(key);
        if (element == null || !element.isJsonArray()) {
            return null;
        }
        return element.getAsJsonArray();
    }

    private static long getLong(JsonObject object, String key) {
        JsonElement element = object.get(key);
        if (element == null || element.isJsonNull()) {
            return 0L;
        }
        try {
            return element.getAsLong();
        } catch (RuntimeException e) {
            return 0L;
        }
    }

    private static double getDouble(JsonObject object, String key) {
        JsonElement element = object.get(key);
        if (element == null || element.isJsonNull()) {
            return 0d;
        }
        try {
            return element.getAsDouble();
        } catch (RuntimeException e) {
            return 0d;
        }
    }

    private static String getString(JsonObject object, String key) {
        JsonElement element = object.get(key);
        if (element == null || element.isJsonNull()) {
            return "";
        }
        try {
            return element.getAsString();
        } catch (RuntimeException e) {
            return "";
        }
    }

    private static String preferredText(JsonObject object, String preferredKey, String fallbackKey) {
        String preferred = getString(object, preferredKey).trim();
        if (!preferred.isEmpty()) {
            return preferred;
        }
        return getString(object, fallbackKey).trim();
    }

    private static String rankedStatusLabel(int ranked) {
        switch (ranked) {
            case -2:
                return "graveyard";
            case -1:
                return "wip";
            case 0:
                return "pending";
            case 1:
                return "ranked";
            case 2:
                return "approved";
            case 3:
                return "qualified";
            case 4:
                return "loved";
            default:
                return "unknown";
        }
    }

    private static List<BeatmapCard> offlineFallback() {
        List<BeatmapCard> cards = new ArrayList<BeatmapCard>();
        cards.add(new BeatmapCard(39804L, 129891L, "Freedom Dive", "xi", "Nakagawa-Kanon", "FOUR DIMENSIONS", 6.52f, 222, "ranked", "offline", "https://osu.direct/api/media/preview/129891", "https://osu.direct/api/d/39804", ""));
        cards.add(new BeatmapCard(292301L, 656431L, "Everything will freeze", "UNDEAD CORPORATION", "Ekoro", "Time Freeze", 6.88f, 240, "ranked", "offline", "https://osu.direct/api/media/preview/656431", "https://osu.direct/api/d/292301", ""));
        cards.add(new BeatmapCard(41823L, 131891L, "Blue Zenith", "xi", "Asphyxia", "Blue Another", 5.96f, 222, "ranked", "offline", "https://osu.direct/api/media/preview/131891", "https://osu.direct/api/d/41823", ""));
        cards.add(new BeatmapCard(55722L, 168245L, "The Big Black", "The Quick Brown Fox", "Blue Dragon", "Blackest", 6.34f, 180, "approved", "offline", "https://osu.direct/api/media/preview/168245", "https://osu.direct/api/d/55722", ""));
        return Collections.unmodifiableList(cards);
    }

    private static String defaultSongsRoot() {
        String envPath = System.getenv("OSUDROID_SONGS_PATH");
        if (envPath != null && !envPath.trim().isEmpty()) {
            return envPath.trim();
        }

        String propertyPath = System.getProperty("osudroid.songsPath");
        if (propertyPath != null && !propertyPath.trim().isEmpty()) {
            return propertyPath.trim();
        }

        String home = System.getProperty("user.home", ".");
        return home + File.separator + "Songs";
    }

    private static String firstNonBlank(String preferred, String fallback, String defaultValue) {
        if (preferred != null && !preferred.trim().isEmpty()) {
            return preferred.trim();
        }
        if (fallback != null && !fallback.trim().isEmpty()) {
            return fallback.trim();
        }
        return defaultValue;
    }

    private static String firstNonBlank(String preferred, String defaultValue) {
        return firstNonBlank(preferred, null, defaultValue);
    }

    private static long parseLong(String value) {
        if (value == null || value.isEmpty()) {
            return 0L;
        }
        try {
            return Long.parseLong(value);
        } catch (NumberFormatException e) {
            return 0L;
        }
    }

    private static float parseFloat(String value) {
        if (value == null || value.isEmpty()) {
            return 0f;
        }
        try {
            return Float.parseFloat(value);
        } catch (NumberFormatException e) {
            return 0f;
        }
    }

    private static OkHttpClient defaultClient() {
        return new OkHttpClient.Builder()
                .connectTimeout(2, TimeUnit.SECONDS)
                .readTimeout(3, TimeUnit.SECONDS)
                .callTimeout(4, TimeUnit.SECONDS)
                .build();
    }
}
