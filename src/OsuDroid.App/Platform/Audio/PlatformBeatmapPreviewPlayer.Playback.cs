#if ANDROID || IOS
using ManagedBass;
using ManagedBass.Fx;
#if IOS
using AVFoundation;
using Foundation;
#endif

namespace OsuDroid.App.Platform.Audio;

public sealed partial class PlatformBeatmapPreviewPlayer
{
    private void PlayLatestRequest(PreviewRequest request)
    {
        var start = PerfDiagnostics.Start();
        if (!BassAudioEngine.EnsureReady())
        {
            lock (playbackGate)
            {
                if (!disposed && request.Generation == playGeneration)
                    PlayFallbackLocked(request.AudioPath, request.PreviewTimeMilliseconds);
            }
            PerfDiagnostics.Log(
                "audio.previewPlay",
                start,
                $"backend=fallback path=\"{Path.GetFileName(request.AudioPath)}\""
            );
            return;
        }

        var handle = CreateLocalPlaybackStream(request.AudioPath);
        if (handle == 0)
        {
            lock (playbackGate)
            {
                if (!disposed && request.Generation == playGeneration)
                    PlayFallbackLocked(request.AudioPath, request.PreviewTimeMilliseconds);
            }
            PerfDiagnostics.Log(
                "audio.previewPlay",
                start,
                $"backend=fallback streamFailed=true path=\"{Path.GetFileName(request.AudioPath)}\""
            );
            return;
        }

        Seek(handle, request.PreviewTimeMilliseconds);
        lock (playbackGate)
            ApplyBassVolumeLocked(handle);
        if (!Bass.ChannelPlay(handle, false))
        {
            BassAudioEngine.LogBassError(
                $"BASS_ChannelPlay({Path.GetFileName(request.AudioPath)})"
            );
            Bass.StreamFree(handle);
            lock (playbackGate)
            {
                if (!disposed && request.Generation == playGeneration)
                    PlayFallbackLocked(request.AudioPath, request.PreviewTimeMilliseconds);
            }
            PerfDiagnostics.Log(
                "audio.previewPlay",
                start,
                $"backend=fallback playFailed=true path=\"{Path.GetFileName(request.AudioPath)}\""
            );
            return;
        }

        lock (playbackGate)
        {
            if (disposed || request.Generation != playGeneration)
            {
                Bass.ChannelStop(handle);
                Bass.StreamFree(handle);
                return;
            }

            FreeCurrentChannel();
            channel = handle;
            playbackSnapshot = new BeatmapPreviewPlaybackSnapshot(
                request.AudioPath,
                true,
                PositionMillisecondsLocked(),
                DurationMillisecondsLocked()
            );
        }

        PerfDiagnostics.Log(
            "audio.previewPlay",
            start,
            $"backend=bass path=\"{Path.GetFileName(request.AudioPath)}\""
        );
    }

    private static int CreateLocalPlaybackStream(string audioPath)
    {
        var flags = BassFlags.Prescan | BassFlags.AutoFree;
        var stream = Bass.CreateStream(audioPath, 0L, 0L, flags);
        if (stream == 0)
            stream = Bass.CreateStream(audioPath, 0L, 0L, flags | BassFlags.Unicode);
        if (stream == 0)
        {
            BassAudioEngine.LogBassError($"BASS_StreamCreateFile({audioPath})");
            return 0;
        }

        _ = Bass.ChannelSetAttribute(stream, ChannelAttribute.Buffer, 0.03f);
        return stream;
    }

    private void PlayFallbackLocked(string audioPath, int previewTimeMilliseconds)
    {
#if IOS
        FreeFallbackPlayer();
        try
        {
            using var url = NSUrl.FromFilename(audioPath);
            fallbackPlayer = AVAudioPlayer.FromUrl(url);
            if (fallbackPlayer is null)
            {
                Console.Error.WriteLine($"[osu-droid] AVAudioPlayer.FromUrl failed: {audioPath}");
                playbackSnapshot = new();
                return;
            }

            fallbackPlayer.PrepareToPlay();
            fallbackPlayer.Volume = volume;
            if (previewTimeMilliseconds > 0)
                fallbackPlayer.CurrentTime = previewTimeMilliseconds / 1000d;
            if (fallbackPlayer.Play())
                playbackSnapshot = new BeatmapPreviewPlaybackSnapshot(
                    audioPath,
                    true,
                    PositionMillisecondsLocked(),
                    DurationMillisecondsLocked()
                );
            else
                playbackSnapshot = new();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[osu-droid] AVAudioPlayer fallback exception: {ex}");
            FreeFallbackPlayer();
            playbackSnapshot = new();
        }
#endif
    }

    private void SeekLocked(int milliseconds)
    {
        if (channel == 0 || milliseconds <= 0)
            return;

        Seek(channel, milliseconds);
    }

    private static void Seek(int handle, int milliseconds)
    {
        if (handle == 0 || milliseconds <= 0)
            return;

        var bytes = Bass.ChannelSeconds2Bytes(handle, milliseconds / 1000d);
        if (bytes >= 0 && !Bass.ChannelSetPosition(handle, bytes, PositionFlags.Bytes))
            BassAudioEngine.LogBassError("BASS_ChannelSetPosition");
    }

    private void FreeCurrentChannel()
    {
        if (channel == 0)
        {
#if IOS
            FreeFallbackPlayer();
#endif
            playbackSnapshot = new();
            return;
        }

        Bass.ChannelStop(channel);
        Bass.StreamFree(channel);
        channel = 0;
        Array.Clear(spectrumFrame, 0, spectrumFrame.Length);
#if IOS
        FreeFallbackPlayer();
#endif
        playbackSnapshot = new();
    }

    private sealed record PreviewRequest(
        string AudioPath,
        int PreviewTimeMilliseconds,
        long Generation
    );

#if IOS
    private void FreeFallbackPlayer()
    {
        fallbackPlayer?.Stop();
        fallbackPlayer?.Dispose();
        fallbackPlayer = null;
    }
#endif
}
#endif
