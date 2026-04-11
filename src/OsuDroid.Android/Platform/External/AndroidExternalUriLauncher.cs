using Android.Content;
using OsuDroid.Game;

namespace OsuDroid.Android.Platform.External;

internal sealed class AndroidExternalUriLauncher(Context context) : IExternalUriLauncher
{
    public void Open(System.Uri uri)
    {
        var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(uri.ToString()));
        intent.AddFlags(ActivityFlags.NewTask);
        context.StartActivity(intent);
    }
}
