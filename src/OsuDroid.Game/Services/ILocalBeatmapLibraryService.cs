using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OsuDroid.Game;

public interface ILocalBeatmapLibraryService
{
    IReadOnlyList<BeatmapCard> Beatmaps { get; }

    Task<IReadOnlyList<BeatmapCard>> RefreshAsync(CancellationToken cancellationToken = default);
}
