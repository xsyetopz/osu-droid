using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.ModSelect;

public sealed partial class ModSelectScene
{
    private void AddPresetDialog(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (_isPresetFormOpen)
        {
            AddPresetFormDialog(elements, viewport);
            return;
        }

        if (_isPresetDeleteDialogOpen)
        {
            AddPresetDeleteDialog(elements, viewport);
        }
    }

    private void AddPresetFormDialog(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        AddModalBackdrop(elements, viewport);
        UiRect panel = PresetDialogPanelBounds(viewport);
        elements.Add(Fill("modselect-preset-dialog", panel, s_badge, 1f, radius: 16f));
        elements.Add(
            Text(
                "modselect-preset-dialog-title",
                "New mod preset",
                new UiRect(panel.X, panel.Y + 16f, panel.Width, 30f),
                21f,
                s_accent,
                bold: true,
                alignment: UiTextAlignment.Center,
                alpha: 0.7f
            )
        );
        elements.Add(
            Fill(
                "modselect-preset-dialog-divider-top",
                new UiRect(panel.X, panel.Y + 62f, panel.Width, 1f),
                s_accent,
                0.1f
            )
        );

        elements.Add(
            Text(
                "modselect-preset-dialog-name-label",
                "Name",
                new UiRect(panel.X + 24f, panel.Y + 82f, panel.Width - 48f, 28f),
                19f,
                s_accent
            )
        );
        UiRect input = PresetNameInputBounds(viewport);
        elements.Add(
            Fill(
                "modselect-preset-dialog-name-input",
                input,
                s_search,
                1f,
                UiAction.ModSelectPresetNameInput,
                12f
            )
        );
        elements.Add(
            Fill(
                "modselect-preset-dialog-name-input-outline",
                input,
                s_accent,
                0.4f,
                UiAction.ModSelectPresetNameInput,
                12f
            )
        );
        elements.Add(
            Text(
                "modselect-preset-dialog-name-text",
                _presetNameInput,
                new UiRect(input.X + 12f, input.Y + 9f, input.Width - 24f, 28f),
                19f,
                s_accent,
                UiAction.ModSelectPresetNameInput,
                clipToBounds: true
            )
        );

        AddPresetDialogModsIndicator(elements, panel);
        elements.Add(
            Fill(
                "modselect-preset-dialog-divider-bottom",
                new UiRect(panel.X, panel.Bottom - 82f, panel.Width, 1f),
                s_accent,
                0.1f
            )
        );
        AddDialogButton(
            elements,
            "modselect-preset-dialog-save",
            new UiRect(panel.X + 24f, panel.Bottom - 66f, (panel.Width - 60f) / 2f, 42f),
            "Save",
            UiAction.ModSelectPresetSave,
            true
        );
        AddDialogButton(
            elements,
            "modselect-preset-dialog-cancel",
            new UiRect(
                panel.X + 36f + (panel.Width - 60f) / 2f,
                panel.Bottom - 66f,
                (panel.Width - 60f) / 2f,
                42f
            ),
            "Cancel",
            UiAction.ModSelectPresetCancel,
            false
        );
    }

    private void AddPresetDeleteDialog(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        AddModalBackdrop(elements, viewport);
        ModPreset? preset = VisiblePresetAt(_pendingPresetDeleteIndex);
        string text = preset is null ? "Delete preset?" : $"Delete preset \"{preset.Name}\"?";
        UiRect panel = new(
            (viewport.VirtualWidth - viewport.VirtualWidth * 0.5f) / 2f,
            (viewport.VirtualHeight - 210f) / 2f,
            viewport.VirtualWidth * 0.5f,
            210f
        );
        elements.Add(Fill("modselect-preset-delete-dialog", panel, s_badge, 1f, radius: 16f));
        elements.Add(
            Text(
                "modselect-preset-delete-title",
                "Delete preset",
                new UiRect(panel.X, panel.Y + 16f, panel.Width, 30f),
                21f,
                s_accent,
                bold: true,
                alignment: UiTextAlignment.Center,
                alpha: 0.7f
            )
        );
        elements.Add(
            Fill(
                "modselect-preset-delete-divider-top",
                new UiRect(panel.X, panel.Y + 62f, panel.Width, 1f),
                s_accent,
                0.1f
            )
        );
        elements.Add(
            Text(
                "modselect-preset-delete-message",
                text,
                new UiRect(panel.X + 24f, panel.Y + 82f, panel.Width - 48f, 34f),
                19f,
                s_accent,
                alignment: UiTextAlignment.Center,
                clipToBounds: true
            )
        );
        elements.Add(
            Fill(
                "modselect-preset-delete-divider-bottom",
                new UiRect(panel.X, panel.Bottom - 82f, panel.Width, 1f),
                s_accent,
                0.1f
            )
        );
        AddDialogButton(
            elements,
            "modselect-preset-delete-confirm",
            new UiRect(panel.X + 24f, panel.Bottom - 66f, (panel.Width - 60f) / 2f, 42f),
            "Delete",
            UiAction.ModSelectPresetDeleteConfirm,
            true
        );
        AddDialogButton(
            elements,
            "modselect-preset-delete-cancel",
            new UiRect(
                panel.X + 36f + (panel.Width - 60f) / 2f,
                panel.Bottom - 66f,
                (panel.Width - 60f) / 2f,
                42f
            ),
            "Cancel",
            UiAction.ModSelectPresetDeleteCancel,
            false
        );
    }

    private static void AddModalBackdrop(
        List<UiElementSnapshot> elements,
        VirtualViewport viewport
    ) =>
        elements.Add(
            Fill(
                "modselect-preset-dialog-backdrop",
                new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                s_black,
                0.3f,
                UiAction.ModSelectPresetBackdrop
            )
        );

    private void AddPresetDialogModsIndicator(List<UiElementSnapshot> elements, UiRect panel)
    {
        float x = panel.X + 24f;
        float y = panel.Y + 190f;
        foreach (
            string acronym in ModCatalog
                .Entries.Select(entry => entry.Acronym)
                .Where(_selectedAcronyms.Contains)
        )
        {
            ModCatalogEntry? entry = EntryByAcronym(acronym);
            if (entry is not null)
            {
                AddModIcon(
                    elements,
                    $"modselect-preset-dialog-mod-{acronym}",
                    entry,
                    new UiRect(x, y, 42f, 42f),
                    UiAction.None,
                    new UiRect(panel.X + 24f, panel.Y + 174f, panel.Width - 48f, 54f)
                );
                x += 37f;
            }
        }
    }

    private void AddDialogButton(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        string text,
        UiAction action,
        bool selected
    )
    {
        UiColor fill = selected ? s_accent : s_button;
        UiColor color = selected ? s_selectedText : s_accent;
        elements.Add(Fill(id, bounds, fill, 1f, action, 12f));
        elements.Add(
            Text(
                $"{id}-text",
                text,
                bounds,
                20f,
                color,
                action,
                bold: true,
                alignment: UiTextAlignment.Center,
                clipToBounds: true
            )
        );
    }
}
