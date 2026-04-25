using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.MainMenu;

public sealed partial class MainMenuScene
{
    private void AddMusicControls(List<UiElementSnapshot> elements)
    {
        UiRect nowPlayingBounds = GetAndroidMusicNowPlayingBounds();
        var titleBounds = new UiRect(0f, nowPlayingBounds.Y, MusicNowPlayingTextRight, 35f);
        var titleStyle = new UiTextStyle(28f, false, UiTextAlignment.Right);
        string title = TruncateNowPlayingTitle(_nowPlaying.ArtistTitle ?? string.Empty);

        elements.Add(new UiElementSnapshot(
            "music-now-playing",
            UiElementKind.Sprite,
            nowPlayingBounds,
            s_white,
            1f,
            DroidAssets.MusicNowPlaying,
            MeasuredTextAnchor: string.IsNullOrWhiteSpace(title)
                ? null
                : new UiMeasuredTextAnchor(title, titleStyle, MusicNowPlayingTextRight, MusicNowPlayingSpriteLeftPadding)));

        if (!string.IsNullOrWhiteSpace(title))
        {
            elements.Add(new UiElementSnapshot(
                "music-title",
                UiElementKind.Text,
                titleBounds,
                s_white,
                1f,
                Text: title,
                TextStyle: titleStyle));
        }

        AddMusicControl(elements, DroidAssets.MusicPrevious, UiAction.MainMenuMusicPrevious, 6f);
        AddMusicControl(elements, DroidAssets.MusicPlay, UiAction.MainMenuMusicPlay, 5f);
        AddMusicControl(elements, DroidAssets.MusicPause, UiAction.MainMenuMusicPause, 4f);
        AddMusicControl(elements, DroidAssets.MusicStop, UiAction.MainMenuMusicStop, 3f);
        AddMusicControl(elements, DroidAssets.MusicNext, UiAction.MainMenuMusicNext, 2f);

        AddSongProgress(elements);
    }

    public static string TruncateNowPlayingTitle(string title)
    {
        const string ellipsis = "...";
        return title.Length > MusicNowPlayingCharactersMaximum
            ? string.Concat(title.AsSpan(0, MusicNowPlayingCharactersMaximum - ellipsis.Length), ellipsis)
            : title;
    }

    private void AddMusicControl(List<UiElementSnapshot> elements, string assetName, UiAction action, float androidIndex)
    {
        elements.Add(new UiElementSnapshot(
            assetName,
            UiElementKind.Sprite,
            GetAndroidMusicControlBounds(androidIndex),
            GetPressedColor(action),
            1f,
            assetName,
            action));
    }

    private void AddSongProgress(List<UiElementSnapshot> elements)
    {
        var progressBounds = new UiRect(MusicProgressX, MusicProgressY, MusicProgressWidth, MusicProgressHeight);
        elements.Add(new UiElementSnapshot(
            "music-progress-bg",
            UiElementKind.Fill,
            progressBounds,
            UiColor.Opaque(0, 0, 0),
            0.3f,
            CornerRadius: 3f));

        float ratio = GetMusicProgressRatio();
        if (ratio <= 0f)
        {
            return;
        }

        elements.Add(new UiElementSnapshot(
            "music-progress-fg",
            UiElementKind.Fill,
            new UiRect(progressBounds.X, progressBounds.Y, progressBounds.Width * ratio, progressBounds.Height),
            UiColor.Opaque(230, 230, 230),
            0.8f,
            CornerRadius: 3f));
    }

    private float GetMusicProgressRatio()
    {
        return _nowPlaying.LengthMilliseconds <= 0
            ? 0f
            : Math.Clamp(_nowPlaying.PositionMilliseconds / (float)_nowPlaying.LengthMilliseconds, 0f, 1f);
    }
}
