using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class MainMenuScene
{
    private void AddMenuButtons(List<UiElementSnapshot> elements)
    {
        if (!IsMenuShown)
            return;

        AddMenuButton(elements, 0, GetAndroidMainMenuButtonBounds(0), UiAction.MainMenuFirst);
        AddMenuButton(elements, 1, GetAndroidMainMenuButtonBounds(1), UiAction.MainMenuSecond);
        AddMenuButton(elements, 2, GetAndroidMainMenuButtonBounds(2), UiAction.MainMenuThird);
    }

    private void AddMenuButton(List<UiElementSnapshot> elements, int index, UiRect finalBounds, UiAction action)
    {
        var assetName = CurrentButtonAsset(index);
        var animatedBounds = finalBounds with { X = GetAnimatedMenuButtonX(finalBounds.X) };
        elements.Add(new UiElementSnapshot(
            $"menu-{index}",
            UiElementKind.Sprite,
            animatedBounds,
            GetPressedColor(action),
            GetMenuButtonAlpha(),
            assetName,
            action));
    }

    private string CurrentButtonAsset(int index)
    {
        if (!IsSecondMenu)
        {
            return index switch
            {
                0 => DroidAssets.Play,
                1 => DroidAssets.Options,
                2 => DroidAssets.Exit,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
            };
        }

        return index switch
        {
            0 => DroidAssets.Solo,
            1 => DroidAssets.Multi,
            2 => DroidAssets.Back,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
        };
    }

    private void AddDownloaderTab(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var tab = DroidAssets.MainMenuManifest.Get(DroidAssets.BeatmapDownloader);
        elements.Add(new UiElementSnapshot(
            "beatmap-downloader",
            UiElementKind.Sprite,
            new UiRect(viewport.VirtualWidth - tab.NativeSize.Width, (viewport.VirtualHeight - tab.NativeSize.Height) / 2f, tab.NativeSize.Width, tab.NativeSize.Height),
            GetPressedColor(UiAction.MainMenuBeatmapDownloader),
            0.92f,
            DroidAssets.BeatmapDownloader,
            UiAction.MainMenuBeatmapDownloader));
    }

    private void AddReturnTransitionBackground(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (!isReturnTransitionActive)
            return;

        var background = DroidAssets.MainMenuManifest.Get(DroidAssets.MenuBackground);
        var scale = viewport.VirtualWidth / background.NativeSize.Width;
        var width = background.NativeSize.Width * scale;
        var height = background.NativeSize.Height * scale;
        var bounds = new UiRect((viewport.VirtualWidth - width) / 2f, (viewport.VirtualHeight - height) / 2f, width, height);
        if (returnTransitionBackgroundPath is not null)
        {
            elements.Add(new UiElementSnapshot(
                "return-background-fade",
                UiElementKind.Sprite,
                new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                white,
                1f - (float)Math.Clamp(returnTransitionMilliseconds / ReturnBackgroundFadeDurationMilliseconds, 0d, 1d),
                ExternalAssetPath: returnTransitionBackgroundPath,
                SpriteFit: UiSpriteFit.Cover));
            return;
        }

        elements.Add(new UiElementSnapshot(
            "return-background-fade",
            UiElementKind.Sprite,
            bounds,
            white,
            1f - (float)Math.Clamp(returnTransitionMilliseconds / ReturnBackgroundFadeDurationMilliseconds, 0d, 1d),
            DroidAssets.MenuBackground));
    }

    private void AddMusicControls(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var nowPlayingBounds = GetAndroidMusicNowPlayingBounds();
        var titleBounds = new UiRect(0f, nowPlayingBounds.Y, MusicNowPlayingTextRight, 35f);
        var titleStyle = new UiTextStyle(28f, false, UiTextAlignment.Right);
        var title = nowPlaying.ArtistTitle;

        elements.Add(new UiElementSnapshot(
            "music-now-playing",
            UiElementKind.Sprite,
            nowPlayingBounds,
            white,
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
                white,
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

    private void AddMusicControl(List<UiElementSnapshot> elements, string assetName, UiAction action, float legacyIndex)
    {
        elements.Add(new UiElementSnapshot(
            assetName,
            UiElementKind.Sprite,
            GetAndroidMusicControlBounds(legacyIndex),
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

        var ratio = GetMusicProgressRatio();
        if (ratio <= 0f)
            return;

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
        if (nowPlaying.LengthMilliseconds <= 0)
            return 0f;

        return Math.Clamp(nowPlaying.PositionMilliseconds / (float)nowPlaying.LengthMilliseconds, 0f, 1f);
    }


    private void AddExitOverlay(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var progress = GetExitProgress();
        if (progress <= 0f)
            return;

        elements.Add(new UiElementSnapshot(
            "exit-blackout",
            UiElementKind.Fill,
            new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
            UiColor.Opaque(0, 0, 0),
            progress));

        if (progress < 1f)
            return;

        elements.Add(new UiElementSnapshot(
            "exit-instruction",
            UiElementKind.Text,
            new UiRect(0f, viewport.VirtualHeight * 0.5f - 18f, viewport.VirtualWidth, 40f),
            white,
            1f,
            Text: "Done playing? Swipe this app away to close it.",
            TextStyle: new UiTextStyle(ExitInstructionTextSize, Alignment: UiTextAlignment.Center)));
    }

    private void AddDevelopmentBuildOverlay(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (!isDevelopmentBuild)
            return;

        var asset = DroidAssets.MainMenuManifest.Get(DroidAssets.DevBuildOverlay);
        var scale = viewport.VirtualWidth / asset.NativeSize.Width;
        var height = Math.Max(1f, asset.NativeSize.Height * scale);
        elements.Add(new UiElementSnapshot(
            "dev-build-overlay",
            UiElementKind.Sprite,
            new UiRect(0f, viewport.VirtualHeight - height, viewport.VirtualWidth, height),
            white,
            1f,
            DroidAssets.DevBuildOverlay));
        var textBounds = new UiRect(0f, viewport.VirtualHeight - height - 24f, viewport.VirtualWidth, 22f);
        elements.Add(new UiElementSnapshot(
            "dev-build-text-shadow",
            UiElementKind.Text,
            new UiRect(textBounds.X + 2f, textBounds.Y + 2f, textBounds.Width, textBounds.Height),
            UiColor.Opaque(0, 0, 0),
            0.5f,
            Text: "DEVELOPMENT BUILD",
            TextStyle: new UiTextStyle(16f, Alignment: UiTextAlignment.Center)));
        elements.Add(new UiElementSnapshot(
            "dev-build-text",
            UiElementKind.Text,
            textBounds,
            UiColor.Opaque(255, 237, 0),
            1f,
            Text: "DEVELOPMENT BUILD",
            TextStyle: new UiTextStyle(16f, Alignment: UiTextAlignment.Center)));
    }

}
