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
            static (int index, out UiAction action) =>
                UiActionGroups.TryGetDownloaderResultCardSlotAction(index, out action),
            static (UiAction action, out int index) =>
                UiActionGroups.TryGetDownloaderResultCardSlotIndex(action, out index)
        );
        AssertRoundTrip(
            8,
            static (int index, out UiAction action) =>
                UiActionGroups.TryGetDownloaderResultPreviewSlotAction(index, out action),
            static (UiAction action, out int index) =>
                UiActionGroups.TryGetDownloaderResultPreviewSlotIndex(action, out index)
        );
        AssertRoundTrip(
            16,
            static (int index, out UiAction action) =>
                UiActionGroups.TryGetDownloaderDetailsDifficultySlotAction(index, out action),
            static (UiAction action, out int index) =>
                UiActionGroups.TryGetDownloaderDetailsDifficultySlotIndex(action, out index)
        );
        AssertRoundTrip(
            8,
            static (int index, out UiAction action) =>
                UiActionGroups.TryGetSongSelectVisibleSetSlotAction(index, out action),
            static (UiAction action, out int index) =>
                UiActionGroups.TryGetSongSelectVisibleSetSlotIndex(action, out index)
        );
        AssertRoundTrip(
            16,
            static (int index, out UiAction action) =>
                UiActionGroups.TryGetSongSelectVisibleDifficultySlotAction(index, out action),
            static (UiAction action, out int index) =>
                UiActionGroups.TryGetSongSelectVisibleDifficultySlotIndex(action, out index)
        );
        AssertRoundTrip(
            8,
            static (int index, out UiAction action) =>
                UiActionGroups.TryGetSongSelectCollectionToggleSlotAction(index, out action),
            static (UiAction action, out int index) =>
                UiActionGroups.TryGetSongSelectCollectionToggleSlotIndex(action, out index)
        );
        AssertRoundTrip(
            8,
            static (int index, out UiAction action) =>
                UiActionGroups.TryGetSongSelectCollectionDeleteSlotAction(index, out action),
            static (UiAction action, out int index) =>
                UiActionGroups.TryGetSongSelectCollectionDeleteSlotIndex(action, out index)
        );
        AssertRoundTrip(
            64,
            static (int index, out UiAction action) =>
                UiActionGroups.TryGetOptionsActiveRowAction(index, out action),
            static (UiAction action, out int index) =>
                UiActionGroups.TryGetOptionsActiveRowIndex(action, out index)
        );
    }

    [Test]
    public void SongSelectFirstSetAliasResolvesToSetZero()
    {
        Assert.That(
            UiActionGroups.TryGetSongSelectVisibleSetSlotIndex(
                UiAction.SongSelectFirstSet,
                out int index
            ),
            Is.True
        );
        Assert.That(index, Is.Zero);
        Assert.That(
            UiActionGroups.TryGetSongSelectVisibleSetSlotAction(index, out UiAction action),
            Is.True
        );
        Assert.That(action, Is.EqualTo(UiAction.SongSelectVisibleSetSlot0));
    }

    [Test]
    public void IndexedActionMapsRejectOutOfRangeIndex()
    {
        Assert.That(UiActionGroups.TryGetDownloaderResultCardSlotAction(-1, out _), Is.False);
        Assert.That(UiActionGroups.TryGetDownloaderResultCardSlotAction(8, out _), Is.False);
        Assert.That(UiActionGroups.TryGetOptionsActiveRowAction(-1, out _), Is.False);
        Assert.That(UiActionGroups.TryGetOptionsActiveRowAction(64, out _), Is.False);
    }

    private delegate bool TryGetActionByIndex(int index, out UiAction action);

    private delegate bool TryGetIndexByAction(UiAction action, out int index);

    private static void AssertRoundTrip(
        int count,
        TryGetActionByIndex tryGetAction,
        TryGetIndexByAction tryGetIndex
    )
    {
        for (int index = 0; index < count; index++)
        {
            Assert.That(tryGetAction(index, out UiAction action), Is.True, $"Index {index}");
            Assert.That(tryGetIndex(action, out int mappedIndex), Is.True, action.ToString());
            Assert.That(mappedIndex, Is.EqualTo(index), action.ToString());
        }
    }
}
