#if ANDROID || IOS
using Microsoft.Maui.Storage;
using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.App.Platform;

public sealed class MauiPlatformPaths : IPlatformPaths
{
    public DroidPathRoots Roots =>
        DroidPathRoots.FromAppDataDirectory(FileSystem.AppDataDirectory, FileSystem.CacheDirectory);
}
#endif
