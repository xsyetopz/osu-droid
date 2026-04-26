using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Input;

namespace OsuDroid.Game.Scenes.ModSelect;

public sealed partial class ModSelectScene
{
    public void FocusSearch(VirtualViewport viewport)
    {
        _textInputService.RequestTextInput(
            new TextInputRequest(
                _searchInputText,
                SetSearchTerm,
                SetSearchTerm,
                viewport.ToSurface(SearchBounds(viewport)),
                () => { },
                "Search..."
            )
        );
    }

    public void SetSearchTerm(string term)
    {
        string nextTerm = term.Trim();
        if (string.Equals(_searchInputText, nextTerm, StringComparison.Ordinal))
        {
            return;
        }

        _searchInputText = nextTerm;
        _searchDebounceElapsed = 0f;
    }

    public void ApplySearchNow()
    {
        _appliedSearchTerm = _searchInputText;
        _searchDebounceElapsed = SearchDebounceSeconds;
        ClampAllScrolls(VirtualViewport.AndroidReferenceLandscape);
    }

    private IEnumerable<ModCatalogEntry> VisibleEntries()
    {
        return string.IsNullOrWhiteSpace(_appliedSearchTerm)
            ? ModCatalog.Entries
            : ModCatalog.Entries.Where(entry =>
                string.Equals(entry.Acronym, _appliedSearchTerm, StringComparison.OrdinalIgnoreCase)
                || SearchContiguously(entry.Name, _appliedSearchTerm)
            );
    }

    private static bool SearchContiguously(string text, string searchTerm)
    {
        int searchIndex = 0;
        foreach (char candidate in text)
        {
            if (char.ToUpperInvariant(candidate) == char.ToUpperInvariant(searchTerm[searchIndex]))
            {
                searchIndex++;
                if (searchIndex == searchTerm.Length)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static UiRect SearchBounds(VirtualViewport viewport) =>
        new(viewport.VirtualWidth - 460f, 12f, 400f, 58f);
}
