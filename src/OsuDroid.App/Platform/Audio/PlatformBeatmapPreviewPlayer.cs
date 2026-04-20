#if ANDROID || IOS
using OsuDroid.Game.Runtime;
#if IOS
using AVFoundation;
using Foundation;
#endif
#if ANDROID
using Android.Media;
#endif

namespace OsuDroid.App.Platform.Audio;

public sealed class PlatformBeatmapPreviewPlayer : IBeatmapPreviewPlayer, IDisposable
{
#if IOS
    private AVAudioPlayer? localPlayer;
    private AVPlayer? remotePlayer;
#endif
#if ANDROID
    private MediaPlayer? player;
#endif

    public void Play(string audioPath, int previewTimeMilliseconds)
    {
        StopPreview();
        if (!File.Exists(audioPath))
            return;

#if IOS
        localPlayer = AVAudioPlayer.FromUrl(NSUrl.FromFilename(audioPath));
        if (localPlayer is null)
            return;

        localPlayer.CurrentTime = Math.Max(0, previewTimeMilliseconds) / 1000d;
        localPlayer.PrepareToPlay();
        localPlayer.Play();
#elif ANDROID
        player = new MediaPlayer();
        player.SetDataSource(audioPath);
        player.Prepare();
        if (previewTimeMilliseconds > 0)
            player.SeekTo(previewTimeMilliseconds);
        player.Start();
#endif
    }

    public void Play(Uri previewUri)
    {
        StopPreview();
#if IOS
        var url = NSUrl.FromString(previewUri.ToString());
        if (url is null)
            return;

        remotePlayer = AVPlayer.FromUrl(url);
        remotePlayer?.Play();
#elif ANDROID
        player = new MediaPlayer();
        player.SetDataSource(previewUri.ToString());
        player.PrepareAsync();
        player.Prepared += (_, _) => player?.Start();
#endif
    }

    public void StopPreview()
    {
#if IOS
        localPlayer?.Stop();
        localPlayer?.Dispose();
        localPlayer = null;
        remotePlayer?.Pause();
        remotePlayer?.Dispose();
        remotePlayer = null;
#elif ANDROID
        if (player is not null)
        {
            if (player.IsPlaying)
                player.Stop();
            player.Release();
            player.Dispose();
            player = null;
        }
#endif
    }

    public void Dispose() => StopPreview();
}
#endif
