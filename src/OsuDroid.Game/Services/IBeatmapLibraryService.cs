using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OsuDroid.Game;

public interface IBeatmapLibraryService
{
    IReadOnlyList<BeatmapCard> LocalBeatmaps { get; }

    Task<IReadOnlyList<BeatmapCard>> RefreshLocalAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BeatmapCard>> SearchOnlineAsync(string query, CancellationToken cancellationToken = default);
}
