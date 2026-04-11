using Foundation;
using OsuDroid.Game;
using UIKit;

namespace OsuDroid.iOS.Platform.External;

internal sealed class IOSExternalUriLauncher : IExternalUriLauncher
{
    public void Open(Uri uri)
    {
        UIApplication.SharedApplication.OpenUrl(new NSUrl(uri.ToString()), new UIApplicationOpenUrlOptions(), null);
    }
}
