using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Scenes.ModSelect;

public sealed partial class ModSelectScene
{
    private void AddTopBar(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        AddButton(
            elements,
            "modselect-back",
            new UiRect(SidePadding, 12f, 120f, 58f),
            _localizer["OsuDroidLanguagePack_menu_mod_back"],
            UiAction.ModSelectBack,
            leadingAsset: DroidAssets.CommonBackArrow
        );
        AddButton(
            elements,
            "modselect-customize",
            new UiRect(190f, 12f, 170f, 58f),
            "Customize",
            UiAction.ModSelectCustomize,
            _selectedAcronyms.Any(ModHasCustomization),
            leadingAsset: DroidAssets.CommonTune
        );
        AddButton(
            elements,
            "modselect-clear",
            new UiRect(370f, 12f, 120f, 58f),
            "Clear",
            UiAction.ModSelectClear,
            true,
            leadingAsset: DroidAssets.CommonBackspace,
            fillOverride: s_clearButton,
            textOverride: DroidUiColors.DangerText
        );
        AddSelectedModsIndicator(elements);

        UiRect search = SearchBounds(viewport);
        elements.Add(
            Fill("modselect-search", search, s_search, 1f, UiAction.ModSelectSearchBox, 12f)
        );
        elements.Add(
            Text(
                "modselect-search-text",
                string.IsNullOrWhiteSpace(_searchInputText) ? "Search..." : _searchInputText,
                new UiRect(search.X + 18f, search.Y + 15f, search.Width - 74f, 28f),
                24f,
                string.IsNullOrWhiteSpace(_searchInputText) ? s_searchPlaceholder : s_accent,
                UiAction.ModSelectSearchBox,
                clipToBounds: true
            )
        );
        elements.Add(
            UiElementFactory.Sprite(
                "modselect-search-icon",
                DroidAssets.CommonSearchSmall,
                new UiRect(search.Right - 52f, search.Y + 13f, 32f, 32f),
                s_accent,
                1f,
                UiAction.ModSelectSearchBox,
                spriteFit: UiSpriteFit.Contain
            )
        );
    }

    private void AddSelectedModsIndicator(List<UiElementSnapshot> elements)
    {
        UiRect strip = SelectedModsBounds();
        float x = strip.X - _selectedModsScrollX;
        foreach (
            string acronym in ModCatalog
                .Entries.Select(entry => entry.Acronym)
                .Where(_selectedAcronyms.Contains)
        )
        {
            UiRect icon = new(x, strip.Y + 8f, SelectedModIconSize, SelectedModIconSize);
            if (icon.Right >= strip.X && icon.X <= strip.Right)
            {
                ModCatalogEntry? entry = EntryByAcronym(acronym);
                if (entry is not null)
                {
                    AddModIcon(
                        elements,
                        $"modselect-selected-{acronym}",
                        entry,
                        icon,
                        UiAction.None,
                        strip
                    );
                }
            }

            x += SelectedModIconSize + SelectedModIconSpacing;
        }

        AddHorizontalScrollbar(
            elements,
            "modselect-selected-mods",
            strip,
            _selectedModsScrollX,
            MaxSelectedModsScroll(),
            IsSelectedModsScrollbarVisible()
        );
    }
}
