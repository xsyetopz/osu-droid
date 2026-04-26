namespace OsuDroid.Game.Scenes.Options;

internal sealed record SettingsCategory(string TitleKey, IReadOnlyList<SettingsRow> Rows);
