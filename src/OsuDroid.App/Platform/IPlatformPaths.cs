#if ANDROID || IOS
namespace OsuDroid.App.Platform;

public interface IPlatformPaths
{
    string CorePath { get; }
}
#endif
