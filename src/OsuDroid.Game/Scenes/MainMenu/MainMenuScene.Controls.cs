using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
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

}
