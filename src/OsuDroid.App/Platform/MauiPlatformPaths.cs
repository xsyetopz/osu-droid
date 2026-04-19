#if ANDROID || IOS
using Microsoft.Maui.Storage;

namespace OsuDroid.App.Platform;

public sealed class MauiPlatformPaths : IPlatformPaths
{
    public string CorePath => FileSystem.AppDataDirectory;
}
#endif
