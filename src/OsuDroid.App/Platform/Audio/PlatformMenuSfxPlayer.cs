#if ANDROID || IOS
using ManagedBass;
using OsuDroid.Game.Runtime;
using System.Collections.Concurrent;

namespace OsuDroid.App.Platform.Audio;

public sealed class PlatformMenuSfxPlayer(string assetsRoot) : IMenuSfxPlayer, IDisposable
{
    private readonly ConcurrentDictionary<string, string> paths = new(StringComparer.Ordinal);
    private readonly ConcurrentBag<int> activeChannels = [];
    private bool disposed;

    public void Play(string key)
    {
        if (disposed || !BassAudioEngine.EnsureReady())
            return;

        var path = paths.GetOrAdd(key, ResolvePath);
        if (!File.Exists(path))
            return;

        var channel = Bass.CreateStream(path, 0L, 0L, BassFlags.AutoFree);
        if (channel == 0)
            channel = Bass.CreateStream(path, 0L, 0L, BassFlags.AutoFree | BassFlags.Unicode);
        if (channel == 0)
        {
            BassAudioEngine.LogBassError($"BASS_StreamCreateFile(sfx:{key})");
            return;
        }

        activeChannels.Add(channel);
        if (!Bass.ChannelPlay(channel, true))
        {
            BassAudioEngine.LogBassError($"BASS_ChannelPlay(sfx:{key})");
            Bass.StreamFree(channel);
        }

        TrimFinishedChannels();
    }

    public void Dispose()
    {
        disposed = true;
        while (activeChannels.TryTake(out var channel))
        {
            Bass.ChannelStop(channel);
            Bass.StreamFree(channel);
        }
    }

    private void TrimFinishedChannels()
    {
        var keep = new List<int>();
        while (activeChannels.TryTake(out var channel))
        {
            if (Bass.ChannelIsActive(channel) == PlaybackState.Stopped)
                Bass.StreamFree(channel);
            else
                keep.Add(channel);
        }

        foreach (var channel in keep)
            activeChannels.Add(channel);
    }

    private string ResolvePath(string key) => Path.Combine(assetsRoot, $"{key}.ogg");
}
#endif
