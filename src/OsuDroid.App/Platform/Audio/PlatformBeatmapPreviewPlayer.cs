#if ANDROID || IOS
using ManagedBass;
using ManagedBass.Fx;
using OsuDroid.Game.Runtime;
#if IOS
using AVFoundation;
using Foundation;
#endif

namespace OsuDroid.App.Platform.Audio;

public sealed partial class PlatformBeatmapPreviewPlayer : IBeatmapPreviewPlayer, IDisposable
{
    private const int SpectrumSize = 512;

    private readonly object playbackGate = new();
    private readonly SemaphoreSlim playSignal = new(0);
    private readonly float[] spectrumFrame = new float[SpectrumSize];
    private PreviewRequest? pendingPlayRequest;
    private Task? playWorker;
    private long playGeneration;
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
            pendingPlayRequest = new PreviewRequest(audioPath, previewTimeMilliseconds, ++playGeneration);
            playWorker ??= Task.Run(ProcessPlayRequests);
        }

        playSignal.Release();
    }

    private async Task ProcessPlayRequests()
    {
        while (true)
        {
            await playSignal.WaitAsync().ConfigureAwait(false);

            PreviewRequest? request;
            lock (playbackGate)
            {
                if (disposed)
                    return;

                request = pendingPlayRequest;
                pendingPlayRequest = null;
            }

            if (request is null)
                continue;

            while (playSignal.Wait(0))
            {
                lock (playbackGate)
                {
                    request = pendingPlayRequest ?? request;
                    pendingPlayRequest = null;
                }
            }

            PlayLatestRequest(request);
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
        {
            pendingPlayRequest = null;
            playGeneration++;
            FreeCurrentChannel();
        }
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
        playSignal.Release();
    }
}
#endif
