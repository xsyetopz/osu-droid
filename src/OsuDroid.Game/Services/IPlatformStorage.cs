using System.Collections.Generic;

namespace OsuDroid.Game;

public interface IPlatformStorage
{
    IReadOnlyList<string> GetSongRoots();
}
