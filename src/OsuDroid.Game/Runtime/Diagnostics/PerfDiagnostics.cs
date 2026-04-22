using System.Diagnostics;

namespace OsuDroid.Game.Runtime;

public static class PerfDiagnostics
{
    private const string EnvironmentVariable = "OSUDROID_PERF_DIAGNOSTICS";
    private static readonly bool enabled = IsEnabled();

    public static bool Enabled => enabled;

    public static long Start() => enabled ? Stopwatch.GetTimestamp() : 0L;

    public static void Log(string phase, long startTimestamp, string? details = null)
    {
        if (!enabled || startTimestamp == 0L)
            return;

        var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
        Console.WriteLine(details is null
            ? $"osu!droid perf phase={phase} elapsedMs={elapsed.TotalMilliseconds:0.###}"
            : $"osu!droid perf phase={phase} elapsedMs={elapsed.TotalMilliseconds:0.###} {details}");
    }

    private static bool IsEnabled()
    {
#if DEBUG
        var value = Environment.GetEnvironmentVariable(EnvironmentVariable);
        return string.Equals(value, "1", StringComparison.Ordinal) ||
               string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
#else
        return false;
#endif
    }
}
