using System.Globalization;
using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.UI.Elements;

public sealed record OnlineProfileSnapshot(
    string Username,
    string? AvatarAssetName = null,
    string? AvatarPath = null,
    int? Rank = null,
    int? PerformancePoints = null,
    float? Accuracy = null
);

public sealed record OnlineProfilePanelState(
    OnlineProfileSnapshot? Profile = null,
    string Message = "Logging in...",
    string Submessage = "Connecting to server..."
)
{
    public static OnlineProfilePanelState Connecting { get; } = new();

    public static OnlineProfilePanelState? FromOptionalProfile(OnlineProfileSnapshot? profile) =>
        profile is not null && !string.IsNullOrWhiteSpace(profile.Username)
            ? new OnlineProfilePanelState(profile)
            : Connecting;
}

public static class OnlineProfilePanelSnapshots
{
    private static readonly UiColor s_panel = DroidUiColors.OnlineProfilePanel;
    private static readonly UiColor s_footer = DroidUiColors.OnlineProfileFooter;
    private static readonly UiColor s_white = DroidUiColors.TextPrimary;
    private static readonly UiColor s_secondary = DroidUiColors.OnlineProfileSecondaryText;
    private static readonly UiColor s_rank = DroidUiColors.OnlineProfileRankText;

    public static void Add(
        List<UiElementSnapshot> elements,
        string idPrefix,
        UiRect bounds,
        float avatarSize,
        OnlineProfilePanelState state,
        float panelAlpha = 1f,
        float footerAlpha = 1f
    )
    {
        elements.Add(
            new UiElementSnapshot(
                idPrefix + "-panel",
                UiElementKind.Fill,
                bounds,
                s_panel,
                panelAlpha
            )
        );

        var avatarBounds = new UiRect(bounds.X, bounds.Y, avatarSize, avatarSize);
        elements.Add(
            new UiElementSnapshot(
                idPrefix + "-avatar-footer",
                UiElementKind.Fill,
                avatarBounds,
                s_footer,
                footerAlpha
            )
        );

        OnlineProfileSnapshot? profile = state.Profile;
        if (profile is null)
        {
            elements.Add(
                new UiElementSnapshot(
                    idPrefix + "-message",
                    UiElementKind.Text,
                    new UiRect(bounds.X + 110f, bounds.Y + 5f, bounds.Width - 120f, 28f),
                    s_white,
                    1f,
                    Text: state.Message,
                    TextStyle: new UiTextStyle(16f)
                )
            );

            elements.Add(
                new UiElementSnapshot(
                    idPrefix + "-submessage",
                    UiElementKind.Text,
                    new UiRect(bounds.X + 110f, bounds.Y + 60f, bounds.Width - 120f, 40f),
                    s_white,
                    1f,
                    Text: state.Submessage,
                    TextStyle: new UiTextStyle(14f)
                )
            );
            return;
        }

        elements.Add(
            new UiElementSnapshot(
                idPrefix + "-avatar",
                UiElementKind.Sprite,
                avatarBounds,
                s_white,
                1f,
                profile.AvatarAssetName ?? DroidAssets.EmptyAvatar,
                ExternalAssetPath: profile.AvatarPath
            )
        );

        elements.Add(
            new UiElementSnapshot(
                idPrefix + "-player",
                UiElementKind.Text,
                new UiRect(
                    bounds.X + avatarSize + 10f,
                    bounds.Y + 5f,
                    bounds.Width - avatarSize - 20f,
                    28f
                ),
                s_white,
                1f,
                Text: profile.Username,
                TextStyle: new UiTextStyle(16f)
            )
        );

        if (profile.Rank is int rank)
        {
            elements.Add(
                new UiElementSnapshot(
                    idPrefix + "-rank",
                    UiElementKind.Text,
                    new UiRect(bounds.Right - 86f, bounds.Y + 55f, 76f, 24f),
                    s_rank,
                    1f,
                    Text: "#" + rank.ToString("N0", CultureInfo.InvariantCulture),
                    TextStyle: new UiTextStyle(20f, Alignment: UiTextAlignment.Right)
                )
            );
        }

        if (profile.PerformancePoints is int pp)
        {
            elements.Add(
                new UiElementSnapshot(
                    idPrefix + "-pp",
                    UiElementKind.Text,
                    new UiRect(
                        bounds.X + avatarSize + 10f,
                        bounds.Y + 50f,
                        bounds.Width - avatarSize - 20f,
                        24f
                    ),
                    s_secondary,
                    1f,
                    Text: "Performance: " + pp.ToString("N0", CultureInfo.InvariantCulture) + "pp",
                    TextStyle: new UiTextStyle(18f)
                )
            );
        }

        if (profile.Accuracy is float accuracy)
        {
            elements.Add(
                new UiElementSnapshot(
                    idPrefix + "-acc",
                    UiElementKind.Text,
                    new UiRect(
                        bounds.X + avatarSize + 10f,
                        bounds.Y + 75f,
                        bounds.Width - avatarSize - 20f,
                        24f
                    ),
                    s_secondary,
                    1f,
                    Text: "Accuracy: "
                        + accuracy.ToString("0.00", CultureInfo.InvariantCulture)
                        + "%",
                    TextStyle: new UiTextStyle(18f)
                )
            );
        }
    }
}
