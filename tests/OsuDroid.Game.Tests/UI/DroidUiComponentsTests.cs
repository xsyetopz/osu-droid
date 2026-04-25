using NUnit.Framework;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;
namespace OsuDroid.Game.Tests.UI;

public sealed class DroidUiComponentsTests
{
    [Test]
    public void SearchFieldUsesSharedThemeAndStableParts()
    {
        var elements = new List<UiElementSnapshot>();
        DroidUiComponents.AddSearchField(elements, "shared-search", new UiRect(10f, 20f, 240f, 58f), string.Empty, UiAction.DownloaderSearchBox);

        Assert.That(elements.Single(element => element.Id == "shared-search-background").Color, Is.EqualTo(DroidUiColors.SurfaceInput));
        Assert.That(elements.Single(element => element.Id == "shared-search-text").Text, Is.EqualTo("Search..."));
        Assert.That(elements.Single(element => element.Id == "shared-search-text").Color, Is.EqualTo(DroidUiColors.TextSecondary));
        Assert.That(elements.Single(element => element.Id == "shared-search-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Search));
    }

    [Test]
    public void DropdownOptionUsesSharedSelectionTokens()
    {
        var elements = new List<UiElementSnapshot>();
        DroidUiComponents.AddDropdownOption(elements, "shared-option", new UiRect(0f, 0f, 180f, 44f), "Ranked", UiAction.DownloaderStatusRanked, true);

        Assert.That(elements.Single(element => element.Id == "shared-option-selected").Color, Is.EqualTo(DroidUiColors.DropdownSelected));
        Assert.That(elements.Single(element => element.Id == "shared-option-text").Color, Is.EqualTo(DroidUiColors.TextPrimary));
        Assert.That(elements.Single(element => element.Id == "shared-option-check").Color, Is.EqualTo(DroidUiColors.Accent));
    }

    [Test]
    public void StatusPillUsesSharedSurfaceAndStatusColor()
    {
        var elements = new List<UiElementSnapshot>();
        DroidUiComponents.AddStatusPill(elements, "shared-status", new UiRect(0f, 0f, 80f, 24f), "Ranked", DroidUiTheme.BeatmapStatus.Ranked);

        Assert.That(elements.Single(element => element.Id == "shared-status-background").Color, Is.EqualTo(DroidUiColors.SurfaceRow));
        Assert.That(elements.Single(element => element.Id == "shared-status-text").Color, Is.EqualTo(DroidUiTheme.BeatmapStatus.Ranked));
    }
}
