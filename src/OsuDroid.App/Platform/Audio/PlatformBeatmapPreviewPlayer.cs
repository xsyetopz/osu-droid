#if ANDROID || IOS
using ManagedBass;
using ManagedBass.Fx;
using OsuDroid.Game.Runtime;
#if IOS
using AVFoundation;
using Foundation;
#endif

namespace OsuDroid.App.Platform.Audio;

public sealed class PlatformBeatmapPreviewPlayer : IBeatmapPreviewPlayer, IDisposable
{
    private const int SpectrumSize = 512;

    private readonly object playbackGate = new();
    private readonly float[] spectrumFrame = new float[SpectrumSize];
    private int channel;
#if IOS
    private AVAudioPlayer? fallbackPlayer;
#endif
    private bool disposed;

    public bool IsPlaying
    {
        get
        {
            lock (playbackGate)
                return channel != 0 && Bass.ChannelIsActive(channel) == PlaybackState.Playing
#if IOS
                    || fallbackPlayer?.Playing == true
#endif
                    ;
        }
    }

    public int PositionMilliseconds
    {
        get
        {
            lock (playbackGate)
            {
                if (channel == 0)
                {
#if IOS
                    return fallbackPlayer is null ? 0 : (int)Math.Min(int.MaxValue, fallbackPlayer.CurrentTime * 1000d);
#else
                    return 0;
#endif
                }

                var bytes = Bass.ChannelGetPosition(channel, PositionFlags.Bytes);
                if (bytes < 0)
                    return 0;

                var seconds = Bass.ChannelBytes2Seconds(channel, bytes);
                if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0d)
                    return 0;

                return (int)Math.Min(int.MaxValue, seconds * 1000d);
            }
        }
    }

    public void Play(string audioPath, int previewTimeMilliseconds)
    {
        if (string.IsNullOrWhiteSpace(audioPath) || !File.Exists(audioPath))
            return;

        lock (playbackGate)
        {
            if (!BassAudioEngine.EnsureReady())
            {
                PlayFallbackLocked(audioPath, previewTimeMilliseconds);
                return;
            }

            FreeCurrentChannel();
            var handle = CreateLocalPlaybackStream(audioPath);
            if (handle == 0)
            {
                PlayFallbackLocked(audioPath, previewTimeMilliseconds);
                return;
            }

            channel = handle;
            SeekLocked(previewTimeMilliseconds);
            if (!Bass.ChannelPlay(channel, false))
            {
                BassAudioEngine.LogBassError($"BASS_ChannelPlay({Path.GetFileName(audioPath)})");
                FreeCurrentChannel();
                PlayFallbackLocked(audioPath, previewTimeMilliseconds);
            }
        }
    }

    public void Play(Uri previewUri)
    {
        if (previewUri is null)
            return;

        lock (playbackGate)
        {
            if (!BassAudioEngine.EnsureReady())
                return;

            FreeCurrentChannel();
            var handle = Bass.CreateStream(previewUri.ToString(), 0, BassFlags.AutoFree, null, IntPtr.Zero);
            if (handle == 0)
            {
                BassAudioEngine.LogBassError($"BASS_StreamCreateURL({previewUri})");
                return;
            }

            channel = handle;
            if (!Bass.ChannelPlay(channel, true))
            {
                BassAudioEngine.LogBassError($"BASS_ChannelPlay({previewUri})");
                FreeCurrentChannel();
            }
        }
    }

    public void PausePreview()
    {
        lock (playbackGate)
        {
            if (channel != 0 && Bass.ChannelIsActive(channel) == PlaybackState.Playing && !Bass.ChannelPause(channel))
                BassAudioEngine.LogBassError("BASS_ChannelPause");
#if IOS
            fallbackPlayer?.Pause();
#endif
        }
    }

    public void ResumePreview()
    {
        lock (playbackGate)
        {
            if (channel != 0 && Bass.ChannelIsActive(channel) == PlaybackState.Paused && !Bass.ChannelPlay(channel, false))
                BassAudioEngine.LogBassError("BASS_ChannelPlay(resume)");
#if IOS
            if (fallbackPlayer is not null && !fallbackPlayer.Playing)
                fallbackPlayer.Play();
#endif
        }
    }

    public void StopPreview()
    {
        lock (playbackGate)
            FreeCurrentChannel();
    }

    public bool TryReadSpectrum1024(float[] destination)
    {
        if (destination.Length < SpectrumSize)
            return false;

        lock (playbackGate)
        {
            if (channel == 0 || Bass.ChannelIsActive(channel) != PlaybackState.Playing)
                return false;

            var read = Bass.ChannelGetData(channel, spectrumFrame, (int)DataFlags.FFT1024);
            if (read < 0)
            {
                BassAudioEngine.LogBassError("BASS_ChannelGetData(FFT1024)");
                return false;
            }

            Array.Copy(spectrumFrame, destination, SpectrumSize);
            return true;
        }
    }

    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        StopPreview();
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
                return;
            }

            fallbackPlayer.PrepareToPlay();
            if (previewTimeMilliseconds > 0)
                fallbackPlayer.CurrentTime = previewTimeMilliseconds / 1000d;
            fallbackPlayer.Play();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[osu-droid] AVAudioPlayer fallback exception: {ex}");
            FreeFallbackPlayer();
        }
#endif
    }

    private void SeekLocked(int milliseconds)
    {
        if (channel == 0 || milliseconds <= 0)
            return;

        var bytes = Bass.ChannelSeconds2Bytes(channel, milliseconds / 1000d);
        if (bytes >= 0 && !Bass.ChannelSetPosition(channel, bytes, PositionFlags.Bytes))
            BassAudioEngine.LogBassError("BASS_ChannelSetPosition");
    }

    private void FreeCurrentChannel()
    {
        if (channel == 0)
        {
#if IOS
            FreeFallbackPlayer();
#endif
            return;
        }

        Bass.ChannelStop(channel);
        Bass.StreamFree(channel);
        channel = 0;
        Array.Clear(spectrumFrame, 0, spectrumFrame.Length);
#if IOS
        FreeFallbackPlayer();
#endif
    }

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
