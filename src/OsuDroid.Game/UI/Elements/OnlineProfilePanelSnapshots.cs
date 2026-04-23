using System.Globalization;

namespace OsuDroid.Game.UI.Elements;

public sealed record OnlineProfileSnapshot(
    string Username,
    string? AvatarAssetName = null,
    string? AvatarPath = null,
    int? PerformancePoints = null,
    float? Accuracy = null)
{
    public static OnlineProfileSnapshot Guest { get; } = new("Guest", DroidAssets.EmptyAvatar);

    public bool IsGuest => PerformancePoints is null && Accuracy is null;
}

public static class OnlineProfilePanelSnapshots
{
    private static readonly UiColor s_panel = new(51, 51, 51, 128);
    private static readonly UiColor s_footer = new(51, 51, 51, 204);
    private static readonly UiColor s_white = UiColor.Opaque(255, 255, 255);

    public static void Add(
        List<UiElementSnapshot> elements,
        string idPrefix,
        UiRect bounds,
        float avatarSize,
        OnlineProfileSnapshot profile,
        float panelAlpha = 1f,
        float footerAlpha = 1f)
    {
        elements.Add(new UiElementSnapshot(
            idPrefix + "-panel",
            UiElementKind.Fill,
            bounds,
            s_panel,
            panelAlpha));

        var avatarBounds = new UiRect(bounds.X, bounds.Y, avatarSize, avatarSize);
        elements.Add(new UiElementSnapshot(
            idPrefix + "-avatar-footer",
            UiElementKind.Fill,
            avatarBounds,
            s_footer,
            footerAlpha));

        elements.Add(new UiElementSnapshot(
            idPrefix + "-avatar",
            UiElementKind.Sprite,
            avatarBounds,
            s_white,
            1f,
            profile.AvatarAssetName ?? DroidAssets.EmptyAvatar,
            ExternalAssetPath: profile.AvatarPath));

        elements.Add(new UiElementSnapshot(
            idPrefix + "-player",
            UiElementKind.Text,
            new UiRect(bounds.X + avatarSize + 10f, bounds.Y + 5f, bounds.Width - avatarSize - 20f, 28f),
            s_white,
            1f,
            Text: string.IsNullOrWhiteSpace(profile.Username) ? "Guest" : profile.Username,
            TextStyle: new UiTextStyle(20f)));

        if (profile.IsGuest)
        {
            return;
        }

        if (profile.PerformancePoints is int pp)
        {
            elements.Add(new UiElementSnapshot(
                idPrefix + "-pp",
                UiElementKind.Text,
                new UiRect(bounds.X + avatarSize + 10f, bounds.Y + 38f, bounds.Width - avatarSize - 20f, 24f),
                s_white,
                1f,
                Text: pp.ToString("N0", CultureInfo.InvariantCulture) + "pp",
                TextStyle: new UiTextStyle(18f)));
        }

        if (profile.Accuracy is float accuracy)
        {
            elements.Add(new UiElementSnapshot(
                idPrefix + "-acc",
                UiElementKind.Text,
                new UiRect(bounds.X + avatarSize + 10f, bounds.Y + 64f, bounds.Width - avatarSize - 20f, 24f),
                s_white,
                1f,
                Text: accuracy.ToString("0.00", CultureInfo.InvariantCulture) + "%",
                TextStyle: new UiTextStyle(18f)));
        }
    }
}
