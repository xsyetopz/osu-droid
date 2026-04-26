using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.App.Platform;

internal static class CrashLogInstaller
{
    private static readonly object s_gate = new();
    private static bool s_isInstalled;
    private static string? s_logPath;

    public static void Install(DroidPathRoots pathRoots)
    {
        var pathLayout = new DroidGamePathLayout(pathRoots);
        Install(pathLayout.Log);
    }

    public static void Install(string logDirectory)
    {
        lock (s_gate)
        {
            Directory.CreateDirectory(logDirectory);
            s_logPath = Path.Combine(logDirectory, "crash.log");
            if (s_isInstalled)
            {
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            s_isInstalled = true;
        }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args) =>
        WriteCrash(
            "unhandled",
            args.ExceptionObject as Exception
                ?? new InvalidOperationException(
                    args.ExceptionObject?.ToString() ?? "Unknown exception."
                )
        );

    private static void OnUnobservedTaskException(
        object? sender,
        UnobservedTaskExceptionEventArgs args
    ) => WriteCrash("unobserved-task", args.Exception);

    private static void WriteCrash(string kind, Exception exception)
    {
        string? logPath;
        lock (s_gate)
        {
            logPath = s_logPath;
        }

        if (string.IsNullOrWhiteSpace(logPath))
        {
            return;
        }

        try
        {
            string? logDirectory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrWhiteSpace(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            File.AppendAllText(
                logPath,
                $"{DateTimeOffset.UtcNow:O} {kind}{Environment.NewLine}{exception}{Environment.NewLine}"
            );
        }
        catch (Exception) { }
    }
}
