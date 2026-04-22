#if ANDROID || IOS
using System.Runtime.InteropServices;
using ManagedBass;
using ManagedBass.Fx;
#if IOS
using AVFoundation;
using Foundation;
#endif

namespace OsuDroid.App.Platform.Audio;

internal static class BassAudioEngine
{
    private static readonly object InitGate = new();
    private static bool resolverRegistered;
    private static bool initAttempted;
    private static bool available;

    public static bool IsAvailable
    {
        get
        {
            EnsureReady();
            return available;
        }
    }

    public static bool EnsureReady()
    {
        if (initAttempted)
            return available;

        lock (InitGate)
        {
            if (initAttempted)
                return available;

            initAttempted = true;
            try
            {
                RegisterNativeResolvers();
                ConfigurePlatformAudioSession();
                Bass.UpdatePeriod = 5;
                Bass.DeviceBufferLength = 10;
                Bass.PlaybackBufferLength = 50;
                Bass.DeviceNonStop = true;

                available = Bass.Init(-1, 44100, DeviceInitFlags.Latency, IntPtr.Zero);
                if (!available)
                    LogBassError("BASS_Init");
                else
                    _ = BassFx.Version;
            }
            catch (Exception ex)
            {
                available = false;
                Console.Error.WriteLine($"[osu-droid] BASS init exception: {ex}");
            }

            return available;
        }
    }

    public static void LogBassError(string operation) =>
        Console.Error.WriteLine($"[osu-droid] {operation} failed: {Bass.LastError}");

    private static void RegisterNativeResolvers()
    {
        if (resolverRegistered)
            return;

        resolverRegistered = true;
        TrySetResolver(typeof(Bass).Assembly);
        TrySetResolver(typeof(BassFx).Assembly);
    }

    private static void TrySetResolver(System.Reflection.Assembly assembly)
    {
        try
        {
            NativeLibrary.SetDllImportResolver(assembly, ResolveNativeLibrary);
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static IntPtr ResolveNativeLibrary(string libraryName, System.Reflection.Assembly assembly, DllImportSearchPath? searchPath)
    {
#if IOS
        if (libraryName.Equals("bass", StringComparison.OrdinalIgnoreCase) && NativeLibrary.TryLoad("@rpath/bass.framework/bass", out var bass))
            return bass;
        if ((libraryName.Equals("bass_fx", StringComparison.OrdinalIgnoreCase) || libraryName.Equals("bassfx", StringComparison.OrdinalIgnoreCase)) && NativeLibrary.TryLoad("@rpath/bass_fx.framework/bass_fx", out var bassFx))
            return bassFx;

        if (TryLoadFrameworkBinary(libraryName, "bass", out bass))
            return bass;
        if (TryLoadFrameworkBinary(libraryName, "bass_fx", out bassFx))
            return bassFx;
#endif
        return IntPtr.Zero;
    }

#if IOS
    private static bool TryLoadFrameworkBinary(string libraryName, string frameworkName, out IntPtr handle)
    {
        handle = IntPtr.Zero;
        var normalizedLibraryName = libraryName.Replace("-", "_", StringComparison.OrdinalIgnoreCase);
        if (!normalizedLibraryName.Equals(frameworkName, StringComparison.OrdinalIgnoreCase) &&
            !normalizedLibraryName.Equals(frameworkName.Replace("_", string.Empty, StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase))
            return false;

        foreach (var path in GetFrameworkProbePaths(frameworkName))
        {
            if (NativeLibrary.TryLoad(path, out handle))
                return true;
        }

        return false;
    }

    private static IEnumerable<string> GetFrameworkProbePaths(string frameworkName)
    {
        var binaryName = frameworkName;
        if (!string.IsNullOrWhiteSpace(NSBundle.MainBundle.PrivateFrameworksPath))
            yield return Path.Combine(NSBundle.MainBundle.PrivateFrameworksPath, $"{frameworkName}.framework", binaryName);
        yield return Path.Combine(NSBundle.MainBundle.BundlePath, "Frameworks", $"{frameworkName}.framework", binaryName);
        yield return Path.Combine(NSBundle.MainBundle.BundlePath, $"{frameworkName}.framework", binaryName);
    }
#endif

    private static void ConfigurePlatformAudioSession()
    {
#if IOS
        try
        {
            var session = AVAudioSession.SharedInstance();
            NSError? error;
            session.SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.DefaultToSpeaker, out error);
            if (error is not null)
                Console.Error.WriteLine($"[osu-droid] AVAudioSession SetCategory failed: {error.LocalizedDescription}");
            session.SetActive(true, out error);
            if (error is not null)
                Console.Error.WriteLine($"[osu-droid] AVAudioSession SetActive failed: {error.LocalizedDescription}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[osu-droid] AVAudioSession exception: {ex}");
        }
#endif
    }
}
#endif
