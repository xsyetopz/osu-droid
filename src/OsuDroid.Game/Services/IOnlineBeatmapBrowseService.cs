using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OsuDroid.Game;

public interface IOnlineBeatmapBrowseService
{
    Task<IReadOnlyList<BeatmapCard>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
