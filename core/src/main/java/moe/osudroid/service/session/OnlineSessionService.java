package moe.osudroid.service.session;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.concurrent.TimeUnit;

import okhttp3.FormBody;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;

public final class OnlineSessionService implements SessionService {
    private static final String HOSTNAME = "osudroid.moe";
    private static final String ENDPOINT = "https://" + HOSTNAME + "/api/";
    private static final String LOGIN_ENDPOINT = ENDPOINT + "login.php";
    private static final String DEFAULT_AVATAR_URL = "https://" + HOSTNAME + "/user/avatar/0.png";
    private static final String ONLINE_VERSION = "60";

    private final OkHttpClient client;
    private final CredentialsSource credentialsSource;

    private SessionSnapshot currentSession = SessionSnapshot.signedOut();

    public OnlineSessionService() {
        this(defaultClient(), new EnvironmentCredentialsSource(System.getenv()));
    }

    OnlineSessionService(OkHttpClient client, CredentialsSource credentialsSource) {
        this.client = client;
        this.credentialsSource = credentialsSource;
    }

    @Override
    public synchronized SessionSnapshot currentSession() {
        return currentSession;
    }

    @Override
    public synchronized SessionSnapshot restoreSession() {
        Credentials credentials = credentialsSource.load();
        if (credentials == null || isBlank(credentials.username) || isBlank(credentials.password)) {
            currentSession = SessionSnapshot.signedOut("No environment-backed session was restored.");
            return currentSession;
        }
        return signIn(credentials.username, credentials.password);
    }

    @Override
    public synchronized SessionSnapshot signIn(String username, String password) {
        if (isBlank(username) || isBlank(password)) {
            currentSession = SessionSnapshot.signedOut("Username and password are required.");
            return currentSession;
        }

        Credentials credentials = new Credentials(username.trim(), password);
        OnlineLoginSnapshot remote = requestOnlineSession(credentials);
        if (remote == null) {
            currentSession = SessionSnapshot.signedOut("Login failed or the service is unreachable.");
            return currentSession;
        }

        currentSession = new SessionSnapshot(
                true,
                true,
                remote.sessionId,
                remote.username,
                remote.username,
                remote.userId,
                remote.rank,
                remote.score,
                remote.pp,
                remote.accuracy,
                remote.avatarUrl,
                "Connected to osudroid.moe.");
        return currentSession;
    }

    @Override
    public synchronized SessionSnapshot signOut() {
        currentSession = SessionSnapshot.signedOut("Signed out.");
        return currentSession;
    }

    private OnlineLoginSnapshot requestOnlineSession(Credentials credentials) {
        FormBody form = new FormBody.Builder()
                .add("username", credentials.username)
                .add("password", hashPassword(credentials.password))
                .add("version", ONLINE_VERSION)
                .build();
        Request request = new Request.Builder().url(LOGIN_ENDPOINT).post(form).build();

        try (Response response = client.newCall(request).execute()) {
            if (!response.isSuccessful() || response.body() == null) {
                return null;
            }
            return parseLoginResponse(response.body().string());
        } catch (IOException e) {
            return null;
        }
    }

    private OnlineLoginSnapshot parseLoginResponse(String responseBody) {
        List<String> lines = compactLines(responseBody);
        if (lines.size() < 2 || !"SUCCESS".equals(lines.get(0))) {
            return null;
        }

        String[] fields = lines.get(1).split("\\s+");
        if (fields.length < 7) {
            return null;
        }

        OnlineLoginSnapshot snapshot = new OnlineLoginSnapshot();
        snapshot.userId = parseLong(fields[0], -1L);
        snapshot.sessionId = fields[1];
        snapshot.rank = parseLong(fields[2], 0L);
        snapshot.score = parseLong(fields[3], 0L);
        snapshot.pp = parseFloat(fields[4], 0f);
        snapshot.accuracy = parseFloat(fields[5], 0f);
        snapshot.username = fields[6];
        String avatarUrl = fields.length >= 8 ? fields[7].trim() : "";
        snapshot.avatarUrl = avatarUrl.isEmpty() ? DEFAULT_AVATAR_URL : avatarUrl;
        return snapshot;
    }

    private static List<String> compactLines(String body) {
        String[] split = body.replace("\r", "").split("\n");
        List<String> lines = new ArrayList<String>(split.length);
        for (String raw : split) {
            String line = raw == null ? "" : raw.trim();
            if (!line.isEmpty()) {
                lines.add(line);
            }
        }
        return lines;
    }

    private static boolean isBlank(String value) {
        return value == null || value.trim().isEmpty();
    }

    private static long parseLong(String value, long fallback) {
        try {
            return Long.parseLong(value);
        } catch (NumberFormatException e) {
            return fallback;
        }
    }

    private static float parseFloat(String value, float fallback) {
        try {
            return Float.parseFloat(value);
        } catch (NumberFormatException e) {
            return fallback;
        }
    }

    private static String hashPassword(String password) {
        String prepared = escapeHtml(addSlashes(password == null ? "" : password.trim())) + "taikotaiko";
        return md5Hex(prepared);
    }

    private static String escapeHtml(String value) {
        return value
                .replace("&", "&amp;")
                .replace("\"", "&quot;")
                .replace("'", "&apos;")
                .replace("<", "&lt;")
                .replace(">", "&gt;");
    }

    private static String addSlashes(String value) {
        return value
                .replace("\\", "\\\\")
                .replace("'", "\\'")
                .replace("\"", "\\\"");
    }

    private static String md5Hex(String text) {
        try {
            MessageDigest digest = MessageDigest.getInstance("MD5");
            byte[] bytes = digest.digest(text.getBytes(StandardCharsets.UTF_8));
            StringBuilder builder = new StringBuilder(bytes.length * 2);
            for (byte value : bytes) {
                int normalized = value & 0xFF;
                if (normalized < 16) {
                    builder.append('0');
                }
                builder.append(Integer.toHexString(normalized));
            }
            return builder.toString();
        } catch (NoSuchAlgorithmException e) {
            throw new IllegalStateException("MD5 algorithm is required", e);
        }
    }

    private static OkHttpClient defaultClient() {
        return new OkHttpClient.Builder()
                .connectTimeout(2, TimeUnit.SECONDS)
                .readTimeout(3, TimeUnit.SECONDS)
                .callTimeout(4, TimeUnit.SECONDS)
                .build();
    }

    interface CredentialsSource {
        Credentials load();
    }

    static final class Credentials {
        private final String username;
        private final String password;

        Credentials(String username, String password) {
            this.username = username;
            this.password = password;
        }
    }

    private static final class EnvironmentCredentialsSource implements CredentialsSource {
        private static final String USER_KEY = "OSUDROID_ONLINE_USERNAME";
        private static final String PASSWORD_KEY = "OSUDROID_ONLINE_PASSWORD";

        private final Map<String, String> environment;

        private EnvironmentCredentialsSource(Map<String, String> environment) {
            this.environment = environment;
        }

        @Override
        public Credentials load() {
            String username = environment.get(USER_KEY);
            String password = environment.get(PASSWORD_KEY);
            if (isBlank(username) || isBlank(password)) {
                return null;
            }
            return new Credentials(username.trim(), password);
        }
    }

    private static final class OnlineLoginSnapshot {
        private long userId;
        private String sessionId;
        private long rank;
        private long score;
        private float pp;
        private float accuracy;
        private String username;
        private String avatarUrl;
    }
}
