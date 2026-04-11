using System;
using System.Collections.Generic;
using Android.Content;
using OsuDroid.Game;

namespace OsuDroid.Android;

internal sealed class AndroidAudioService : IAudioService
{
    public void PlayMenuSample(MenuSample sample)
    {
    }
}

internal sealed class AndroidExternalUriLauncher(Context context) : IExternalUriLauncher
{
    public void Open(System.Uri uri)
    {
        var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(uri.ToString()));
        intent.AddFlags(ActivityFlags.NewTask);
        context.StartActivity(intent);
    }
}

internal sealed class AndroidPlatformStorage : IPlatformStorage
{
    public IReadOnlyList<string> GetSongRoots() => Array.Empty<string>();
}
