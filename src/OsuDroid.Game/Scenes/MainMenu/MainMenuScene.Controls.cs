namespace OsuDroid.Game.Scenes.MainMenu;

public sealed partial class MainMenuScene
{
    private void AddMenuButtons(List<UiElementSnapshot> elements)
    {
        if (!IsMenuShown)
        {
            return;
        }

        AddMenuButton(elements, 0, GetAndroidMainMenuButtonBounds(0), UiAction.MainMenuFirst);
        AddMenuButton(elements, 1, GetAndroidMainMenuButtonBounds(1), UiAction.MainMenuSecond);
        AddMenuButton(elements, 2, GetAndroidMainMenuButtonBounds(2), UiAction.MainMenuThird);
    }

    private void AddMenuButton(List<UiElementSnapshot> elements, int index, UiRect finalBounds, UiAction action)
    {
        string assetName = CurrentButtonAsset(index);
        UiRect animatedBounds = finalBounds with { X = GetAnimatedMenuButtonX(finalBounds.X) };
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
        return !IsSecondMenu
            ? index switch
            {
                0 => DroidAssets.Play,
                1 => DroidAssets.Options,
                2 => DroidAssets.Exit,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
            }
            : index switch
            {
                0 => DroidAssets.Solo,
                1 => DroidAssets.Multi,
                2 => DroidAssets.Back,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
            };
    }

    private void AddDownloaderTab(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        UiAssetEntry tab = DroidAssets.MainMenuManifest.Get(DroidAssets.BeatmapDownloader);
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
        if (!_isReturnTransitionActive)
        {
            return;
        }

        UiAssetEntry background = DroidAssets.MainMenuManifest.Get(DroidAssets.MenuBackground);
        float scale = viewport.VirtualWidth / background.NativeSize.Width;
        float width = background.NativeSize.Width * scale;
        float height = background.NativeSize.Height * scale;
        var bounds = new UiRect((viewport.VirtualWidth - width) / 2f, (viewport.VirtualHeight - height) / 2f, width, height);
        if (_returnTransitionBackgroundPath is not null)
        {
            elements.Add(new UiElementSnapshot(
                "return-background-fade",
                UiElementKind.Sprite,
                new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                s_white,
                1f - (float)Math.Clamp(_returnTransitionMilliseconds / ReturnBackgroundFadeDurationMilliseconds, 0d, 1d),
                ExternalAssetPath: _returnTransitionBackgroundPath,
                SpriteFit: UiSpriteFit.Cover));
            return;
        }

        elements.Add(new UiElementSnapshot(
            "return-background-fade",
            UiElementKind.Sprite,
            bounds,
            s_white,
            1f - (float)Math.Clamp(_returnTransitionMilliseconds / ReturnBackgroundFadeDurationMilliseconds, 0d, 1d),
            DroidAssets.MenuBackground));
    }

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

    private void AddExitDialog(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        UiRect panel = CreateExitDialogPanelBounds(viewport);
        elements.Add(new UiElementSnapshot(
            "exit-dialog-scrim",
            UiElementKind.Fill,
            new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
            s_modalScrim,
            1f,
            Action: UiAction.MainMenuExitDialogPanel));

        elements.Add(new UiElementSnapshot(
            "exit-dialog-panel",
            UiElementKind.Fill,
            panel,
            s_modalPanel,
            1f,
            Action: UiAction.MainMenuExitDialogPanel,
            CornerRadius: ExitDialogPanelRadius));

        AddAboutText(elements, panel.X, panel.Y + 18f, "exit-dialog-title", _localizer["MainMenu_ExitDialogTitle"], panel.Width, 26f, true, s_white, UiTextAlignment.Center);
        AddAboutDivider(elements, panel.X, panel.Y + ExitDialogTitleBarHeight, panel.Width, 1f, "exit-dialog-title-divider");

        AddAboutText(
            elements,
            panel.X + ExitDialogContentInset,
            panel.Y + ExitDialogTitleBarHeight + 34f,
            "exit-dialog-message",
            _localizer["MainMenu_ExitDialogMessage"],
            panel.Width - ExitDialogContentInset * 2f,
            ExitDialogTextSize,
            false,
            s_white,
            UiTextAlignment.Center);

        float buttonWidth = (panel.Width - ExitDialogContentInset * 2f - ExitDialogButtonGap) / 2f;
        float buttonY = panel.Bottom - ExitDialogContentInset - ExitDialogButtonHeight;
        AddExitDialogButton(
            elements,
            "exit-dialog-confirm",
            new UiRect(panel.X + ExitDialogContentInset, buttonY, buttonWidth, ExitDialogButtonHeight),
            _localizer["MainMenu_ExitDialogConfirm"],
            UiAction.MainMenuExitConfirm,
            UiColor.Opaque(196, 205, 255),
            UiColor.Opaque(24, 24, 38));
        AddExitDialogButton(
            elements,
            "exit-dialog-cancel",
            new UiRect(panel.X + ExitDialogContentInset + buttonWidth + ExitDialogButtonGap, buttonY, buttonWidth, ExitDialogButtonHeight),
            _localizer["MainMenu_ExitDialogCancel"],
            UiAction.MainMenuExitCancel,
            UiColor.Opaque(58, 58, 88),
            s_white);
    }

    private static UiRect CreateExitDialogPanelBounds(VirtualViewport viewport)
    {
        float width = Math.Min(ExitDialogPanelWidth, viewport.VirtualWidth - 80f);
        float height = Math.Min(ExitDialogPanelHeight, viewport.VirtualHeight - 80f);
        return new UiRect(
            (viewport.VirtualWidth - width) / 2f,
            (viewport.VirtualHeight - height) / 2f,
            width,
            height);
    }

    private static void AddExitDialogButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action, UiColor fillColor, UiColor textColor)
    {
        elements.Add(new UiElementSnapshot(
            id,
            UiElementKind.Fill,
            bounds,
            fillColor,
            1f,
            Action: action,
            CornerRadius: 9f));
        elements.Add(new UiElementSnapshot(
            $"{id}-text",
            UiElementKind.Text,
            new UiRect(bounds.X, bounds.Y + 14f, bounds.Width, bounds.Height - 14f),
            textColor,
            1f,
            Text: text,
            TextStyle: new UiTextStyle(25f, true, UiTextAlignment.Center)));
    }


    private void AddExitOverlay(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float progress = GetExitProgress();
        if (progress <= 0f)
        {
            return;
        }

        elements.Add(new UiElementSnapshot(
            "exit-blackout",
            UiElementKind.Fill,
            new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
            UiColor.Opaque(0, 0, 0),
            progress));

        if (progress < 1f)
        {
            return;
        }

        elements.Add(new UiElementSnapshot(
            "exit-instruction",
            UiElementKind.Text,
            new UiRect(0f, viewport.VirtualHeight * 0.5f - 18f, viewport.VirtualWidth, 40f),
            s_white,
            1f,
            Text: _localizer["MainMenu_ExitInstruction"],
            TextStyle: new UiTextStyle(ExitInstructionTextSize, Alignment: UiTextAlignment.Center)));
    }

    private void AddDevelopmentBuildOverlay(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (!_isDevelopmentBuild)
        {
            return;
        }

        UiAssetEntry asset = DroidAssets.MainMenuManifest.Get(DroidAssets.DevBuildOverlay);
        float scale = viewport.VirtualWidth / asset.NativeSize.Width;
        float height = Math.Max(1f, asset.NativeSize.Height * scale);
        elements.Add(new UiElementSnapshot(
            "dev-build-overlay",
            UiElementKind.Sprite,
            new UiRect(0f, viewport.VirtualHeight - height, viewport.VirtualWidth, height),
            s_white,
            1f,
            DroidAssets.DevBuildOverlay));
        var textBounds = new UiRect(0f, viewport.VirtualHeight - height - 24f, viewport.VirtualWidth, 22f);
        elements.Add(new UiElementSnapshot(
            "dev-build-text-shadow",
            UiElementKind.Text,
            new UiRect(textBounds.X + 2f, textBounds.Y + 2f, textBounds.Width, textBounds.Height),
            UiColor.Opaque(0, 0, 0),
            0.5f,
            Text: _localizer["MainMenu_DevelopmentBuild"],
            TextStyle: new UiTextStyle(16f, Alignment: UiTextAlignment.Center)));
        elements.Add(new UiElementSnapshot(
            "dev-build-text",
            UiElementKind.Text,
            textBounds,
            UiColor.Opaque(255, 237, 0),
            1f,
            Text: _localizer["MainMenu_DevelopmentBuild"],
            TextStyle: new UiTextStyle(16f, Alignment: UiTextAlignment.Center)));
    }

}
