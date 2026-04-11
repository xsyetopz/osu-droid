package moe.osudroid.service.beatmap;

import java.util.List;

public interface BeatmapLibraryService {
    List<BeatmapCard> installedBeatmaps();

    List<BeatmapCard> searchOnline(String query);

    String getSongsRoot();

    String getOnlineStatus();
}
