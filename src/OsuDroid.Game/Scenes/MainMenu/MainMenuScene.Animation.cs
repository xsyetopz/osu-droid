using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Geometry;
namespace OsuDroid.Game.Scenes.MainMenu;

public sealed partial class MainMenuScene
{
#pragma warning disable IDE0072 // Fallback arms are intentional for scene state defaults.
    private UiRect GetLogoBounds(VirtualViewport viewport)
    {
        UiRect collapsed = GetCenteredLogoBounds(viewport);
        UiRect expanded = GetExpandedLogoBounds();
        float progress = GetMenuTransitionProgress();
        return new UiRect(
            Lerp(collapsed.X, expanded.X, progress),
            Lerp(collapsed.Y, expanded.Y, progress),
            Lerp(collapsed.Width, expanded.Width, progress),
            Lerp(collapsed.Height, expanded.Height, progress));
    }

    private static UiRect GetCenteredLogoBounds(VirtualViewport viewport)
    {
        float logoSize = ExpandedLogoReferenceSize / MainMenuReferenceToVirtualScale;
        return new UiRect((viewport.VirtualWidth - logoSize) / 2f, (viewport.VirtualHeight - logoSize) / 2f, logoSize, logoSize);
    }

    private static UiRect GetExpandedLogoBounds() => ReferenceRect(ExpandedLogoReferenceX, ExpandedLogoReferenceY, ExpandedLogoReferenceSize, ExpandedLogoReferenceSize);

    private float GetMenuTransitionProgress() => _menuVisibility switch
    {
        MenuVisibility.Collapsed => 0f,
        MenuVisibility.Expanding => EaseOutExpo((float)Math.Clamp(_transitionMilliseconds / MenuExpandDurationMilliseconds, 0d, 1d)),
        MenuVisibility.Expanded => 1f,
        MenuVisibility.Collapsing => 1f - EaseOutBounce((float)Math.Clamp(_transitionMilliseconds / MenuCollapseDurationMilliseconds, 0d, 1d)),
        MenuVisibility.Exiting => 1f,
        _ => 0f,
    };

    private float GetMenuButtonAlpha() => _menuVisibility switch
    {
        MenuVisibility.Collapsed => 0f,
        MenuVisibility.Expanding => 0.9f * EaseOutCubic((float)Math.Clamp(_transitionMilliseconds / MenuExpandDurationMilliseconds, 0d, 1d)),
        MenuVisibility.Expanded => 0.9f,
        MenuVisibility.Collapsing => 0.9f * (1f - EaseOutExpo((float)Math.Clamp(_transitionMilliseconds / MenuCollapseDurationMilliseconds, 0d, 1d))),
        MenuVisibility.Exiting => 0f,
        _ => 0f,
    };

    private float GetAnimatedMenuButtonX(float finalX)
    {
        return _menuVisibility switch
        {
            MenuVisibility.Collapsed => finalX,
            MenuVisibility.Expanding => Lerp(finalX - ButtonExpandOffset / MainMenuReferenceToVirtualScale, finalX, EaseOutElastic((float)Math.Clamp(_transitionMilliseconds / 500d, 0d, 1d))),
            MenuVisibility.Expanded => finalX,
            MenuVisibility.Collapsing => Lerp(finalX, finalX - ButtonCollapseOffset / MainMenuReferenceToVirtualScale, EaseOutExpo((float)Math.Clamp(_transitionMilliseconds / MenuCollapseDurationMilliseconds, 0d, 1d))),
            MenuVisibility.Exiting => finalX,
            _ => finalX,
        };
    }

    private float GetLogoHeartbeatScale()
    {
        if (_heartbeatMilliseconds < 0d)
        {
            return 1f;
        }

        double growDuration = _currentBeatMilliseconds * 0.9d;
        double shrinkDuration = _currentBeatMilliseconds * 0.07d;
        if (_heartbeatMilliseconds <= growDuration)
        {
            return Lerp(1f, LogoBeatScale, (float)(_heartbeatMilliseconds / growDuration));
        }

        float shrinkProgress = (float)Math.Clamp((_heartbeatMilliseconds - growDuration) / shrinkDuration, 0d, 1d);
        return Lerp(LogoBeatScale, 1f, shrinkProgress);
    }

    private float GetExitProgress() => _menuVisibility == MenuVisibility.Exiting
        ? (float)Math.Clamp(_exitMilliseconds / ExitAnimationMilliseconds, 0d, 1d)
        : 0f;

    private UiColor GetPressedColor(UiAction action)
    {
        if (action == UiAction.None || _pressedAction != action)
        {
            return s_white;
        }

        byte channel = (byte)MathF.Round(byte.MaxValue * PressTint);
        return UiColor.Opaque(channel, channel, channel);
    }

    private string GetVersionText() => $"osu!droid {_displayVersion}";

    private static UiRect CreateVersionPillBounds(VirtualViewport viewport, string text)
    {
        float width = EstimateVersionTextWidth(text) + VersionPillTextXInset * 2f;
        float height = VersionPillTextHeight + VersionPillTextYInset * 2f;
        return new UiRect(VersionPillMargin, viewport.VirtualHeight - height - VersionPillMargin, width, height);
    }

    private static float EstimateVersionTextWidth(string text) => Math.Max(120f, text.Length * VersionPillTextSize * 0.58f + 4f);

    private static UiRect ReferenceRect(float x, float y, float width, float height) => new(
        x / MainMenuReferenceToVirtualScale,
        y / MainMenuReferenceToVirtualScale,
        width / MainMenuReferenceToVirtualScale,
        height / MainMenuReferenceToVirtualScale);

    private static float Lerp(float start, float end, float progress) => start + (end - start) * progress;

    private static float EaseOutCubic(float progress) => 1f - MathF.Pow(1f - progress, 3f);

    private static float EaseOutExpo(float progress) => progress >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * progress);

    private static float EaseOutElastic(float progress)
    {
        if (progress <= 0f)
        {
            return 0f;
        }

        if (progress >= 1f)
        {
            return 1f;
        }

        const float period = 2f * MathF.PI / 3f;
        return MathF.Pow(2f, -10f * progress) * MathF.Sin((progress * 10f - 0.75f) * period) + 1f;
    }

    private static float EaseOutBounce(float progress)
    {
        const float bounce = 7.5625f;
        const float divisor = 2.75f;

        return progress < 1f / divisor
            ? bounce * progress * progress
            : progress < 2f / divisor
            ? bounce * (progress -= 1.5f / divisor) * progress + 0.75f
            : progress < 2.5f / divisor
            ? bounce * (progress -= 2.25f / divisor) * progress + 0.9375f
            : bounce * (progress -= 2.625f / divisor) * progress + 0.984375f;
    }

    private static UiRect ScaleFromCenter(UiRect bounds, float scale)
    {
        float width = bounds.Width * scale;
        float height = bounds.Height * scale;
        return new UiRect(
            bounds.X - (width - bounds.Width) / 2f,
            bounds.Y - (height - bounds.Height) / 2f,
            width,
            height);
    }
#pragma warning restore IDE0072
}
