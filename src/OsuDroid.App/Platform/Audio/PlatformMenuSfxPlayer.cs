#if ANDROID || IOS
using ManagedBass;
using OsuDroid.Game.Runtime;
using System.Collections.Concurrent;

namespace OsuDroid.App.Platform.Audio;

public sealed class PlatformMenuSfxPlayer(string assetsRoot) : IMenuSfxPlayer, IDisposable
{
    private readonly ConcurrentDictionary<string, string> paths = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, int> channels = new(StringComparer.Ordinal);
    private float volume = 1f;
    private bool disposed;

    public void Play(string key)
    {
        var start = PerfDiagnostics.Start();
        if (disposed || !BassAudioEngine.EnsureReady())
            return;

        var path = paths.GetOrAdd(key, ResolvePath);
        if (!File.Exists(path))
            return;

        var channel = channels.GetOrAdd(key, _ => CreateChannel(path, key));
        if (channel == 0)
            return;

        ApplyVolume(channel);
        if (!Bass.ChannelPlay(channel, true))
        {
            BassAudioEngine.LogBassError($"BASS_ChannelPlay(sfx:{key})");
            if (channels.TryRemove(key, out var failed))
                Bass.StreamFree(failed);
        }

        PerfDiagnostics.Log("audio.sfxPlay", start, $"key={key}");
    }

    public void Preload(params string[] keys)
    {
        var start = PerfDiagnostics.Start();
        if (disposed || !BassAudioEngine.EnsureReady())
            return;

        foreach (var key in keys)
        {
            var path = paths.GetOrAdd(key, ResolvePath);
            if (File.Exists(path))
                ApplyVolume(channels.GetOrAdd(key, _ => CreateChannel(path, key)));
        }

        PerfDiagnostics.Log("audio.sfxPreload", start, $"count={keys.Length}");
    }

    public void SetVolume(float normalizedVolume)
    {
        volume = Math.Clamp(normalizedVolume, 0f, 1f);
        foreach (var channel in channels.Values)
            ApplyVolume(channel);
    }

    public void Dispose()
    {
        disposed = true;
        foreach (var channel in channels.Values)
        {
            Bass.ChannelStop(channel);
            Bass.StreamFree(channel);
        }

        channels.Clear();
    }

    private static int CreateChannel(string path, string key)
    {
        var channel = Bass.CreateStream(path, 0L, 0L, BassFlags.Default);
        if (channel == 0)
            channel = Bass.CreateStream(path, 0L, 0L, BassFlags.Unicode);
        if (channel == 0)
            BassAudioEngine.LogBassError($"BASS_StreamCreateFile(sfx:{key})");
        return channel;
    }

    private void ApplyVolume(int channel)
    {
        if (channel != 0)
            _ = Bass.ChannelSetAttribute(channel, ChannelAttribute.Volume, volume);
    }

    private string ResolvePath(string key) => Path.Combine(assetsRoot, $"{key}.ogg");
}
#endif
