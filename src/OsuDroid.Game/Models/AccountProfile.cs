namespace OsuDroid.Game;

public sealed record AccountProfile(
    string DisplayName,
    string? AvatarUrl,
    string StatusText)
{
    public static AccountProfile Guest() => new("Guest", null, string.Empty);
}
