using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Scenes.MainMenu;

public sealed partial class MainMenuScene
{
    public GameFrameSnapshot Snapshot => CreateSnapshot(VirtualViewport.LegacyLandscape);

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) => new(
        "MainMenu",
        "osu!droid",
        IsSecondMenu ? "legacy second menu" : "legacy first menu",
        CurrentEntries,
        _selectedIndex,
        IsSecondMenu,
        CreateUiFrame(viewport));

    public GameFrameSnapshot CreateAboutDialogSnapshot(VirtualViewport viewport) => new(
        "MainMenu",
        "osu!droid",
        IsSecondMenu ? "legacy second menu" : "legacy first menu",
        CurrentEntries,
        _selectedIndex,
        IsSecondMenu,
        CreateUiFrame(viewport, true));

    private string[] CurrentEntries => IsSecondMenu
        ? [_localizer["MainMenu_Solo"], _localizer["MainMenu_Multiplayer"], _localizer["MainMenu_Back"]]
        : [_localizer["MainMenu_Play"], _localizer["MainMenu_Options"], _localizer["MainMenu_Exit"]];

    public static UiRect GetAndroidCollapsedLogoBounds(VirtualViewport viewport) => GetCenteredLogoBounds(viewport);

    public static UiRect GetAndroidExpandedLogoBounds(VirtualViewport _) => GetExpandedLogoBounds();

    public static UiRect GetAndroidMainMenuButtonBounds(int index) => index switch
    {
        0 => ReferenceRect(ButtonReferenceX, FirstButtonReferenceY, ButtonReferenceWidth, ButtonReferenceHeight),
        1 => ReferenceRect(ButtonReferenceX, SecondButtonReferenceY, ButtonReferenceWidth, ButtonReferenceHeight),
        2 => ReferenceRect(ButtonReferenceX, ThirdButtonReferenceY, ButtonReferenceWidth, ButtonReferenceHeight),
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, null),
    };

    public static UiRect GetAndroidMusicControlBounds(float legacyIndex) => new(
        VirtualViewport.LegacyWidth - MusicControlStep * legacyIndex + MusicControlRightOffset,
        MusicControlY,
        MusicControlSize,
        MusicControlSize);

    public static UiRect GetAndroidMusicNowPlayingBounds()
    {
        UiAssetEntry nowPlayingAsset = DroidAssets.MainMenuManifest.Get(DroidAssets.MusicNowPlaying);
        float width = MusicNowPlayingHeight * nowPlayingAsset.NativeSize.Width / nowPlayingAsset.NativeSize.Height;
        return new UiRect(VirtualViewport.LegacyWidth - MusicNowPlayingXOffset, 0f, width, MusicNowPlayingHeight);
    }

    public UiRect GetVersionPillBounds(VirtualViewport viewport) => CreateVersionPillBounds(viewport, GetVersionText());

    private MainMenuRoute ActivateSelected()
    {
        return !IsSecondMenu
            ? _selectedIndex switch
            {
                0 => ShowSecondMenu(),
                1 => MainMenuRoute.Settings,
                2 => BeginExitAnimation(),
                _ => MainMenuRoute.None,
            }
            : _selectedIndex switch
            {
                0 => MainMenuRoute.Solo,
                1 => MainMenuRoute.Multiplayer,
                2 => BackToFirstMenu(),
                _ => MainMenuRoute.None,
            };
    }

    private MainMenuRoute BeginExitAnimation()
    {
        ShowFirstMenu();
        _menuVisibility = MenuVisibility.Exiting;
        _transitionMilliseconds = 0d;
        _shownMilliseconds = 0d;
        _exitMilliseconds = 0d;
        _hasPendingExitRoute = false;
        _exitRoutePublished = false;
        _pressedAction = UiAction.None;
        return MainMenuRoute.None;
    }

    private MainMenuRoute ShowSecondMenu()
    {
        IsSecondMenu = true;
        _selectedIndex = 0;
        return MainMenuRoute.None;
    }

    private MainMenuRoute BackToFirstMenu()
    {
        ShowFirstMenu();
        return MainMenuRoute.None;
    }

    private void ShowFirstMenu()
    {
        IsSecondMenu = false;
        _selectedIndex = 0;
    }

    private void BeginExpand()
    {
        ShowFirstMenu();
        _menuVisibility = MenuVisibility.Expanding;
        _pressedAction = UiAction.None;
        _transitionMilliseconds = 0d;
        _shownMilliseconds = 0d;
    }

    private void BeginCollapse()
    {
        ShowFirstMenu();
        _menuVisibility = MenuVisibility.Collapsing;
        _pressedAction = UiAction.None;
        _transitionMilliseconds = 0d;
    }

    private UiFrameSnapshot CreateUiFrame(VirtualViewport viewport, bool forceAboutDialog = false)
    {
        var elements = new List<UiElementSnapshot>
        {
            new(
                "background-color",
                UiElementKind.Fill,
                new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                s_backgroundColor,
                1f),
        };

        AddBackground(elements, viewport);
        AddReturnTransitionBackground(elements, viewport);
        AddMenuButtons(elements);
        AddLogo(elements, viewport);
        AddProfileShell(elements);
        AddVersionPill(elements, viewport);
        AddDownloaderTab(elements, viewport);
        AddMusicControls(elements);
        AddDevelopmentBuildOverlay(elements, viewport);
        AddExitOverlay(elements, viewport);

        if (_isAboutDialogOpen || forceAboutDialog)
        {
            AddAboutDialog(elements, viewport, _displayVersion);
        }

        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }

    private static void AddBackground(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        UiAssetEntry background = DroidAssets.MainMenuManifest.Get(DroidAssets.MenuBackground);
        float scale = viewport.VirtualWidth / background.NativeSize.Width;
        float width = background.NativeSize.Width * scale;
        float height = background.NativeSize.Height * scale;
        elements.Add(new UiElementSnapshot(
            "menu-background",
            UiElementKind.Sprite,
            new UiRect((viewport.VirtualWidth - width) / 2f, (viewport.VirtualHeight - height) / 2f, width, height),
            s_white,
            1f,
            DroidAssets.MenuBackground));
    }

    private void AddProfileShell(List<UiElementSnapshot> elements)
    {
        OnlineProfilePanelSnapshots.Add(
            elements,
            "profile",
            new UiRect(OnlinePanelX, OnlinePanelY, OnlinePanelWidth, OnlinePanelHeight),
            OnlinePanelAvatarFooterSize,
            _profile);
    }

    private void AddVersionPill(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        string text = GetVersionText();
        UiRect bounds = CreateVersionPillBounds(viewport, text);
        elements.Add(new UiElementSnapshot(
            "version-pill",
            UiElementKind.Fill,
            bounds,
            s_translucentBlack,
            1f,
            Action: UiAction.MainMenuVersionPill,
            CornerRadius: VersionPillCornerRadius));

        elements.Add(new UiElementSnapshot(
            "version-pill-text",
            UiElementKind.Text,
            new UiRect(bounds.X + VersionPillTextXInset, bounds.Y + VersionPillTextYInset, bounds.Width - VersionPillTextXInset * 2f, bounds.Height - VersionPillTextYInset * 2f),
            s_white,
            1f,
            Text: text,
            TextStyle: new UiTextStyle(VersionPillTextSize)));
    }

    private void AddLogo(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        UiRect baseLogoBounds = GetLogoBounds(viewport);
        UiRect logoBounds = ScaleFromCenter(baseLogoBounds, GetLogoHeartbeatScale());
        UiRect overlayBounds = ScaleFromCenter(baseLogoBounds, LogoBeatScale);
        float exitProgress = GetExitProgress();
        if (exitProgress > 0f)
        {
            logoBounds = ScaleFromCenter(logoBounds, Lerp(1f, ExitLogoScale, exitProgress));
            overlayBounds = ScaleFromCenter(overlayBounds, Lerp(1f, ExitLogoScale, exitProgress));
        }

        float rotation = Lerp(0f, ExitLogoRotationDegrees, exitProgress);

        AddLogoSpectrumBars(elements, baseLogoBounds);

        elements.Add(new UiElementSnapshot(
            "logo",
            UiElementKind.Sprite,
            logoBounds,
            s_white,
            1f,
            DroidAssets.Logo,
            UiAction.MainMenuCookie,
            RotationDegrees: rotation));

        elements.Add(new UiElementSnapshot(
            "logo-glow",
            UiElementKind.Sprite,
            overlayBounds,
            s_white,
            0.2f,
            DroidAssets.Logo,
            RotationDegrees: rotation));
    }

    private void AddLogoSpectrumBars(List<UiElementSnapshot> elements, UiRect baseLogoBounds)
    {
        if (!_nowPlaying.IsPlaying || !_hasRawSpectrum)
        {
            return;
        }

        float centerX = baseLogoBounds.X + baseLogoBounds.Width * 0.5f;
        float centerY = baseLogoBounds.Y + baseLogoBounds.Height * 0.5f;

        const float barHeight = 10f;
        const float baselineWidth = 250f;
        for (int i = 0; i < SpectrumBarCount; i++)
        {
            float width = baselineWidth + _spectrumPeakLevel[i];
            if (width <= 0.1f || _spectrumPeakAlpha[i] <= 0.001f)
            {
                continue;
            }

            float angle = -220f + i * 3f;
            float alpha = Math.Clamp(_spectrumPeakAlpha[i], 0f, 0.4f);

            elements.Add(new UiElementSnapshot(
                $"logo-spectrum-{i}",
                UiElementKind.Fill,
                new UiRect(centerX, centerY - barHeight * 0.5f, width, barHeight),
                s_white,
                alpha,
                CornerRadius: 0f,
                RotationDegrees: angle,
                RotationOriginX: 0f,
                RotationOriginY: 0.5f));
        }
    }

}
