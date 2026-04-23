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
    private BeatmapPreviewPlaybackSnapshot playbackSnapshot = new();
    private PreviewRequest? pendingPlayRequest;
    private Task? playWorker;
    private long playGeneration;
    private int channel;
    private float volume = 1f;
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

    public BeatmapPreviewPlaybackSnapshot PlaybackSnapshot
    {
        get
        {
            lock (playbackGate)
                return CreatePlaybackSnapshotLocked();
        }
    }

    public void Play(string audioPath, int previewTimeMilliseconds)
    {
        if (string.IsNullOrWhiteSpace(audioPath) || !File.Exists(audioPath))
            return;

        lock (playbackGate)
        {
            FreeCurrentChannel();
            playbackSnapshot = new();
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
            ApplyBassVolumeLocked(channel);
            if (!Bass.ChannelPlay(channel, true))
            {
                BassAudioEngine.LogBassError($"BASS_ChannelPlay({previewUri})");
                FreeCurrentChannel();
                playbackSnapshot = new();
                return;
            }

            playbackSnapshot = new BeatmapPreviewPlaybackSnapshot(previewUri.ToString(), true, PositionMillisecondsLocked(), DurationMillisecondsLocked());
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
            playbackSnapshot = CreatePlaybackSnapshotLocked();
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
            playbackSnapshot = CreatePlaybackSnapshotLocked();
        }
    }

    public void StopPreview()
    {
        lock (playbackGate)
        {
            pendingPlayRequest = null;
            playGeneration++;
            FreeCurrentChannel();
            playbackSnapshot = new();
        }
    }

    public void SetVolume(float normalizedVolume)
    {
        lock (playbackGate)
        {
            volume = Math.Clamp(normalizedVolume, 0f, 1f);
            ApplyBassVolumeLocked(channel);
#if IOS
            if (fallbackPlayer is not null)
                fallbackPlayer.Volume = volume;
#endif
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

    private BeatmapPreviewPlaybackSnapshot CreatePlaybackSnapshotLocked()
    {
        if (playbackSnapshot.Source is null)
            return new();

        return playbackSnapshot with
        {
            IsPlaying = IsPlayingLocked(),
            PositionMilliseconds = PositionMillisecondsLocked(),
            DurationMilliseconds = DurationMillisecondsLocked(),
        };
    }

    private bool IsPlayingLocked() =>
        channel != 0 && Bass.ChannelIsActive(channel) == PlaybackState.Playing
#if IOS
        || fallbackPlayer?.Playing == true
#endif
        ;

    private int PositionMillisecondsLocked()
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

    private int DurationMillisecondsLocked()
    {
        if (channel == 0)
        {
#if IOS
            return fallbackPlayer is null ? playbackSnapshot.DurationMilliseconds : (int)Math.Min(int.MaxValue, fallbackPlayer.Duration * 1000d);
#else
            return playbackSnapshot.DurationMilliseconds;
#endif
        }

        var bytes = Bass.ChannelGetLength(channel, PositionFlags.Bytes);
        if (bytes < 0)
            return playbackSnapshot.DurationMilliseconds;

        var seconds = Bass.ChannelBytes2Seconds(channel, bytes);
        if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds <= 0d)
            return playbackSnapshot.DurationMilliseconds;

        return (int)Math.Min(int.MaxValue, seconds * 1000d);
    }

    private void ApplyBassVolumeLocked(int handle)
    {
        if (handle != 0)
            _ = Bass.ChannelSetAttribute(handle, ChannelAttribute.Volume, volume);
    }
}
#endif
