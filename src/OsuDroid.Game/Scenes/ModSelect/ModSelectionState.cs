namespace OsuDroid.Game.Scenes.ModSelect;

public sealed record ModSelectionState(
    IReadOnlyList<string> Acronyms,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Settings
)
{
    public static ModSelectionState Empty { get; } =
        new(
            [],
            new Dictionary<string, IReadOnlyDictionary<string, string>>(
                StringComparer.OrdinalIgnoreCase
            )
        );
}
