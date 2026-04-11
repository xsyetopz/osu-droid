using System;
using System.Collections.Generic;
using Foundation;
using OsuDroid.Game;
using UIKit;

namespace OsuDroid.iOS;

internal sealed class IOSAudioService : IAudioService
{
    public void PlayMenuSample(MenuSample sample)
    {
    }
}

internal sealed class IOSExternalUriLauncher : IExternalUriLauncher
{
    public void Open(Uri uri)
    {
        UIApplication.SharedApplication.OpenUrl(new NSUrl(uri.ToString()), new UIApplicationOpenUrlOptions(), null);
    }
}

internal sealed class IOSPlatformStorage : IPlatformStorage
{
    public IReadOnlyList<string> GetSongRoots() => Array.Empty<string>();
}
