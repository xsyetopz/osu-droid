#if ANDROID || IOS
using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.App.Platform;

public interface IPlatformPaths
{
    DroidPathRoots Roots { get; }

    string CorePath => Roots.CoreRoot;
}
#endif
