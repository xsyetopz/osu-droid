using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NUnit.Framework;
using OsuDroid.Game.Localization;

namespace OsuDroid.Game.Tests;

public sealed class OsuDroidLocalizationParityTests
{
    private static readonly string[] s_localizedSceneFiles =
    [
        "src/OsuDroid.Game/Scenes/MainMenu/MainMenuScene.About.cs",
        "src/OsuDroid.Game/Scenes/MainMenu/MainMenuScene.Controls.cs",
        "src/OsuDroid.Game/Scenes/SongSelect/SongSelectScene.PopupLayout.cs",
        "src/OsuDroid.Game/Scenes/BeatmapDownloader/BeatmapDownloaderScene.Cards.cs",
        "src/OsuDroid.Game/Scenes/BeatmapDownloader/BeatmapDownloaderScene.Details.cs",
        "src/OsuDroid.Game/Scenes/BeatmapDownloader/BeatmapDownloaderScene.Filters.cs",
        "src/OsuDroid.Game/Scenes/BeatmapDownloader/BeatmapDownloaderScene.Layout.cs",
        "src/OsuDroid.Game/Scenes/BeatmapDownloader/BeatmapDownloaderScene.Operations.cs",
    ];

    [Test]
    public void OsuDroidAndroidResourceStringsAreAvailableInEnglishCatalog()
    {
        var localizer = new GameLocalizer();
        (string Name, string Value)[] sourceStrings = LoadOsuDroidAndroidStrings();

        foreach ((string? name, string? value) in sourceStrings)
        {
            Assert.That(localizer[$"OsuDroid_{name}"], Is.EqualTo(value), name);
        }
    }

    [Test]
    public void OsuDroidLanguagePackEnglishStringsAreAvailableInEnglishCatalog()
    {
        var localizer = new GameLocalizer();
        (string Name, string Value)[] languagePackStrings = LoadLanguagePackEnglishStrings();

        Assert.That(languagePackStrings.Length, Is.GreaterThan(300));

        foreach ((string? name, string? value) in languagePackStrings)
        {
            Assert.That(localizer[$"OsuDroidLanguagePack_{name}"], Is.EqualTo(value), name);
        }
    }

    [Test]
    public void OsuDroidStringReferencesResolveToLocalOrLanguagePackResources()
    {
        var knownNames = LoadOsuDroidAndroidStrings().Select(item => item.Name)
            .Concat(LoadLanguagePackEnglishStrings().Select(item => item.Name))
            .Concat(["ok", "cancel"])
            .ToHashSet(StringComparer.Ordinal);
        (string Name, string Path, int Line)[] references = LoadOsuDroidStringReferences();

        Assert.That(references.Length, Is.GreaterThan(350));

        foreach ((string Name, string Path, int Line) reference in references)
        {
            Assert.That(knownNames, Does.Contain(reference.Name), $"{reference.Name} at {reference.Path}:{reference.Line}");
        }
    }

    [Test]
    public void EnglishCatalogIsGeneratedFromOsuDroidSources()
    {
        DirectoryInfo root = RepoRoot();
        string script = Path.Combine(root.FullName, "scripts", "dev", "generate-osudroid-localization.py");
        Process process = Process.Start(new ProcessStartInfo
        {
            FileName = "python3",
            ArgumentList = { script, "--check" },
            WorkingDirectory = root.FullName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        }) ?? throw new InvalidOperationException("Could not start localization generator.");

        process.WaitForExit();

        Assert.That(process.ExitCode, Is.EqualTo(0), process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd());
    }

    [Test]
    public void CriticalOsuDroidUiStringsMatchLatestUpstreamEnglish()
    {
        var localizer = new GameLocalizer();

        Assert.Multiple(() =>
        {
            Assert.That(localizer["Options_RemoveSliderLockTitle"], Is.EqualTo("Remove slider and spinner lock"));
            Assert.That(localizer["Options_RemoveSliderLockSummary"], Is.EqualTo("[UNRANKED] Allow circles and sliders to be hittable when another slider or spinner is currently active."));
            Assert.That(localizer["Options_PreferNoVideoDownloadsTitle"], Is.EqualTo("Prefer downloads without video"));
            Assert.That(localizer["Options_PreferNoVideoDownloadsSummary"], Is.EqualTo("Prefer downloading beatmaps without video in beatmap downloader if the selected beatmap mirror supports it"));
            Assert.That(localizer["Options_HighPrecisionInputSummary"], Is.EqualTo("Use more touch samples for more accurate hit timing and cursor movement. May increase battery usage and reduce performance on low-end devices"));
            Assert.That(localizer["Options_RegisterTitle"], Is.EqualTo("Register"));
            Assert.That(localizer["Options_LoginSummary"], Is.EqualTo("Your online name"));
            Assert.That(localizer["Options_OffsetCalibrationTitle"], Is.EqualTo("Offset Calibration"));
            Assert.That(localizer["SongSelect_ManageFavorites"], Is.EqualTo("Manage Beatmap folder"));
            Assert.That(localizer["BeatmapDownloader_WorkInProgress"], Is.EqualTo("Work in Progress"));
            Assert.That(localizer["BeatmapDownloader_Connecting"], Is.EqualTo("Connecting to server..."));
            Assert.That(localizer["BeatmapDownloader_Downloading"], Is.EqualTo("Downloading beatmap {0}..."));
            Assert.That(localizer["MainMenu_AboutDiscord"], Is.EqualTo("Join the official Discord server ↗"));
            Assert.That(localizer["MainMenu_ExitInstruction"], Is.EqualTo("Done playing? Swipe this app away to close it."));
            Assert.That(localizer["MainMenu_ExitDialogTitle"], Is.EqualTo("Exit"));
            Assert.That(localizer["MainMenu_ExitDialogMessage"], Is.EqualTo("Are you sure you want to exit the game?"));
            Assert.That(localizer["MainMenu_ExitDialogConfirm"], Is.EqualTo("Yes"));
            Assert.That(localizer["MainMenu_ExitDialogCancel"], Is.EqualTo("No"));
            Assert.That(localizer["BeatmapDownloader_ConnectionFailed"], Is.EqualTo("Failed to connect to server, please check your internet connection."));
            Assert.That(localizer["OsuDroidLanguagePack_menu_mod_back"], Is.EqualTo("Back"));
            Assert.That(localizer["OsuDroidLanguagePack_menu_mod_reset"], Is.EqualTo("Reset all mods"));
            Assert.That(localizer["OsuDroidLanguagePack_mod_section_difficulty_reduction"], Is.EqualTo("Difficulty Reduction"));
            Assert.That(localizer["OsuDroidLanguagePack_mod_section_difficulty_increase"], Is.EqualTo("Difficulty Increase"));
            Assert.That(localizer["OsuDroidLanguagePack_mod_section_difficulty_automation"], Is.EqualTo("Automation"));
            Assert.That(localizer["OsuDroidLanguagePack_mod_section_difficulty_conversion"], Is.EqualTo("Conversion"));
            Assert.That(localizer["OsuDroidLanguagePack_mod_section_fun"], Is.EqualTo("Fun"));
        });
    }

    [Test]
    public void LocalizedSceneFilesDoNotKeepKnownUserFacingLiterals()
    {
        var forbidden = new Regex("\"(?:Song Properties|Add to Favorites|Manage Favorites|Delete beatmap|Search for\\.\\.\\.|No collections|Filters|Sort by|Ranked status|Download \\(no video\\)|Loading more\\.\\.\\.|Mapped by|No beatmaps found|Failed to connect to server, please check your internet connection\\.|DEVELOPMENT BUILD|Made by osu!droid team)\"", RegexOptions.Compiled);

        foreach (string file in s_localizedSceneFiles)
        {
            string path = Path.Combine(RepoRoot().FullName, file);
            Assert.That(File.Exists(path), Is.True, file);
            Assert.That(forbidden.IsMatch(File.ReadAllText(path)), Is.False, file);
        }
    }

    private static (string Name, string Value)[] LoadOsuDroidAndroidStrings()
    {
        string root = Path.Combine(RepoRoot().FullName, "third_party", "osu-droid-legacy");
        return LoadAndroidValues(root).ToArray();
    }

    private static (string Name, string Value)[] LoadLanguagePackEnglishStrings()
    {
        string root = Path.Combine(RepoRoot().FullName, "third_party", "osu-droid-language-pack", "language-pack", "src", "main", "res", "values");
        return LoadAndroidValues(root).ToArray();
    }

    private static string AndroidUnescape(string value) => value
        .Replace("\\'", "'", StringComparison.Ordinal)
        .Replace("\\\"", "\"", StringComparison.Ordinal)
        .Replace("\\n", "\n", StringComparison.Ordinal);

    private static string AndroidToResxFormat(string value)
    {
        int index = 0;
        return Regex.Replace(value, "%(?:\\d+\\$)?[sd]", _ => $"{{{index++}}}");
    }

    private static IEnumerable<(string Name, string Value)> LoadAndroidValues(string root)
    {
        foreach (string? path in Directory.EnumerateFiles(root, "*.xml", SearchOption.AllDirectories)
                     .Where(path => Path.GetFileName(Path.GetDirectoryName(path)!) == "values")
                     .OrderBy(path => path, StringComparer.Ordinal))
        {
            var document = XDocument.Load(path);

            foreach (XElement? element in document.Root!.Elements().Where(element => element.Name.LocalName is "string" or "string-array" or "plurals"))
            {
                string? name = element.Attribute("name")?.Value;
                if (name is null)
                {
                    continue;
                }

                string value = element.Name.LocalName switch
                {
                    "string" => element.Value,
                    "string-array" => string.Join("|", element.Elements("item").Select(item => item.Value)),
                    "plurals" => string.Join("|", element.Elements("item").Select(item => $"{item.Attribute("quantity")!.Value}={item.Value}")),
                    _ => element.Value,
                };

                yield return (name, AndroidToResxFormat(AndroidUnescape(value.Trim())));
            }
        }
    }

    private static (string Name, string Path, int Line)[] LoadOsuDroidStringReferences()
    {
        DirectoryInfo root = RepoRoot();
        string sourceRoot = Path.Combine(root.FullName, "third_party", "osu-droid-legacy");
        var referenceRegex = new Regex(@"(?:com\.osudroid\.resources\.)?R\.string\.([A-Za-z0-9_]+)|\bstring\.([A-Za-z0-9_]+)|@string/([A-Za-z0-9_]+)", RegexOptions.Compiled);
        var references = new List<(string Name, string Path, int Line)>();

        foreach (string? path in Directory.EnumerateFiles(sourceRoot, "*.*", SearchOption.AllDirectories)
                     .Where(path => path.EndsWith(".java", StringComparison.Ordinal) || path.EndsWith(".kt", StringComparison.Ordinal) || path.EndsWith(".xml", StringComparison.Ordinal)))
        {
            if (path.Contains($"{Path.DirectorySeparatorChar}res{Path.DirectorySeparatorChar}values", StringComparison.Ordinal))
            {
                continue;
            }

            int lineNumber = 0;
            foreach (string line in File.ReadLines(path))
            {
                lineNumber++;
                foreach (Match match in referenceRegex.Matches(line))
                {
                    string name = match.Groups.Cast<Group>().Skip(1).First(group => group.Success).Value;
                    references.Add((name, Path.GetRelativePath(root.FullName, path), lineNumber));
                }
            }
        }

        return references.Distinct().ToArray();
    }

    private static DirectoryInfo RepoRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "upstream-sources.lock.json")))
        {
            directory = directory.Parent;
        }

        return directory ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
