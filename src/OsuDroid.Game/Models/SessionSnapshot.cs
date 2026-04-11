namespace OsuDroid.Game;

public sealed record SessionSnapshot(
    bool IsGuest,
    bool IsSignedIn,
    string Username,
    string DisplayName)
{
    public static SessionSnapshot Guest() => new(true, false, "Guest", "Guest");
}
