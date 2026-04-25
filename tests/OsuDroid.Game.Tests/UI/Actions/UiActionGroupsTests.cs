using NUnit.Framework;
using OsuDroid.Game.UI.Actions;
namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{
    [Test]
    public void IndexedActionMapsRoundTripAcrossRanges()
    {
        AssertRoundTrip(
            8,
            static (int index, out UiAction action) => UiActionGroups.TryGetDownloaderCardAction(index, out action),
            static (UiAction action, out int index) => UiActionGroups.TryGetDownloaderCardIndex(action, out index));
        AssertRoundTrip(
            8,
            static (int index, out UiAction action) => UiActionGroups.TryGetDownloaderPreviewAction(index, out action),
            static (UiAction action, out int index) => UiActionGroups.TryGetDownloaderPreviewIndex(action, out index));
        AssertRoundTrip(
            16,
            static (int index, out UiAction action) => UiActionGroups.TryGetDownloaderDetailsDifficultyAction(index, out action),
            static (UiAction action, out int index) => UiActionGroups.TryGetDownloaderDetailsDifficultyIndex(action, out index));
        AssertRoundTrip(
            8,
            static (int index, out UiAction action) => UiActionGroups.TryGetSongSelectSetAction(index, out action),
            static (UiAction action, out int index) => UiActionGroups.TryGetSongSelectSetIndex(action, out index));
        AssertRoundTrip(
            16,
            static (int index, out UiAction action) => UiActionGroups.TryGetSongSelectDifficultyAction(index, out action),
            static (UiAction action, out int index) => UiActionGroups.TryGetSongSelectDifficultyIndex(action, out index));
        AssertRoundTrip(
            8,
            static (int index, out UiAction action) => UiActionGroups.TryGetSongSelectCollectionToggleAction(index, out action),
            static (UiAction action, out int index) => UiActionGroups.TryGetSongSelectCollectionToggleIndex(action, out index));
        AssertRoundTrip(
            8,
            static (int index, out UiAction action) => UiActionGroups.TryGetSongSelectCollectionDeleteAction(index, out action),
            static (UiAction action, out int index) => UiActionGroups.TryGetSongSelectCollectionDeleteIndex(action, out index));
        AssertRoundTrip(
            64,
            static (int index, out UiAction action) => UiActionGroups.TryGetOptionsRowAction(index, out action),
            static (UiAction action, out int index) => UiActionGroups.TryGetOptionsRowIndex(action, out index));
    }

    [Test]
    public void SongSelectFirstSetAliasResolvesToSetZero()
    {
        Assert.That(UiActionGroups.TryGetSongSelectSetIndex(UiAction.SongSelectFirstSet, out int index), Is.True);
        Assert.That(index, Is.Zero);
        Assert.That(UiActionGroups.TryGetSongSelectSetAction(index, out UiAction action), Is.True);
        Assert.That(action, Is.EqualTo(UiAction.SongSelectSet0));
    }

    [Test]
    public void IndexedActionMapsRejectOutOfRangeIndex()
    {
        Assert.That(UiActionGroups.TryGetDownloaderCardAction(-1, out _), Is.False);
        Assert.That(UiActionGroups.TryGetDownloaderCardAction(8, out _), Is.False);
        Assert.That(UiActionGroups.TryGetOptionsRowAction(-1, out _), Is.False);
        Assert.That(UiActionGroups.TryGetOptionsRowAction(64, out _), Is.False);
    }

    private delegate bool TryGetActionByIndex(int index, out UiAction action);

    private delegate bool TryGetIndexByAction(UiAction action, out int index);

    private static void AssertRoundTrip(int count, TryGetActionByIndex tryGetAction, TryGetIndexByAction tryGetIndex)
    {
        for (int index = 0; index < count; index++)
        {
            Assert.That(tryGetAction(index, out UiAction action), Is.True, $"Index {index}");
            Assert.That(tryGetIndex(action, out int mappedIndex), Is.True, action.ToString());
            Assert.That(mappedIndex, Is.EqualTo(index), action.ToString());
        }
    }
}
