using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
    public void Update(TimeSpan elapsed)
    {
        float elapsedSeconds = (float)elapsed.TotalSeconds;
        _elapsedSeconds += elapsedSeconds;
        _setListScroll.UpdateLinear(
            elapsedSeconds,
            SongMenuScrollDecelerationPerSecond,
            () => scrollY,
            value => scrollY = value,
            MinSetScroll(VirtualViewport.AndroidReferenceLandscape),
            MaxSetScroll()
        );
        _collectionListScroll.Update(
            elapsedSeconds,
            () => collectionScrollY,
            value => collectionScrollY = value,
            0f,
            MaxCollectionScroll(VirtualViewport.AndroidReferenceLandscape)
        );
        selectedSetExpansion = Math.Clamp(selectedSetExpansion + elapsedSeconds * 2f, 0f, 1f);
        selectedBackgroundLuminance += elapsedSeconds * BackgroundLuminancePerSecond;
        ApplyCompletedLibraryRefresh();
        ApplyCompletedDifficultyUpdates();
    }

    public void Scroll(float deltaY, UiPoint point, VirtualViewport viewport)
    {
        if (_collectionsOpen)
        {
            collectionScrollY = Math.Clamp(
                collectionScrollY + deltaY,
                0f,
                MaxCollectionScroll(viewport)
            );
            return;
        }

        if (_propertiesOpen || _beatmapOptionsOpen)
        {
            return;
        }

        if (point.X < viewport.VirtualWidth * ScrollTouchMinimumXRatio)
        {
            return;
        }

        Scroll(deltaY, viewport);
    }

    public void Scroll(float deltaY, VirtualViewport viewport)
    {
        if (_collectionsOpen)
        {
            collectionScrollY = Math.Clamp(
                collectionScrollY + deltaY,
                0f,
                MaxCollectionScroll(viewport)
            );
            return;
        }

        if (_propertiesOpen || _beatmapOptionsOpen)
        {
            return;
        }

        scrollY = ClampScroll(scrollY + deltaY);
    }

    public bool TryBeginScrollDrag(
        UiPoint point,
        VirtualViewport viewport,
        double? timestampSeconds = null
    )
    {
        double timestamp = timestampSeconds ?? _elapsedSeconds;
        if (_collectionsOpen && MaxCollectionScroll(viewport) > 0f)
        {
            _activeScrollTarget = SongSelectScrollTarget.Collections;
            _collectionListScroll.Begin(point, timestamp);
            return true;
        }

        if (
            _propertiesOpen
            || _beatmapOptionsOpen
            || point.X < viewport.VirtualWidth * ScrollTouchMinimumXRatio
        )
        {
            return false;
        }

        if (MaxSetScroll() <= MinSetScroll(viewport))
        {
            return false;
        }

        _activeScrollTarget = SongSelectScrollTarget.Sets;
        _setListScroll.Begin(point, timestamp);
        return true;
    }

    public bool UpdateScrollDrag(
        UiPoint point,
        VirtualViewport viewport,
        double? timestampSeconds = null
    )
    {
        double timestamp = timestampSeconds ?? _elapsedSeconds;
        return _activeScrollTarget switch
        {
            SongSelectScrollTarget.Collections => _collectionListScroll.Drag(
                point,
                timestamp,
                () => collectionScrollY,
                value => collectionScrollY = value,
                0f,
                MaxCollectionScroll(viewport)
            ),
            SongSelectScrollTarget.Sets => _setListScroll.Drag(
                point,
                timestamp,
                () => scrollY,
                value => scrollY = value,
                MinSetScroll(viewport),
                MaxSetScroll()
            ),
            _ => false,
        };
    }

    public void EndScrollDrag(
        UiPoint point,
        VirtualViewport viewport,
        double? timestampSeconds = null
    )
    {
        double timestamp = timestampSeconds ?? _elapsedSeconds;
        switch (_activeScrollTarget)
        {
            case SongSelectScrollTarget.Collections:
                _collectionListScroll.End(
                    point,
                    timestamp,
                    () => collectionScrollY,
                    value => collectionScrollY = value,
                    0f,
                    MaxCollectionScroll(viewport)
                );
                _setListScroll.End();
                break;
            case SongSelectScrollTarget.Sets:
                _setListScroll.End(
                    point,
                    timestamp,
                    () => scrollY,
                    value => scrollY = value,
                    MinSetScroll(viewport),
                    MaxSetScroll()
                );
                _collectionListScroll.End();
                break;
            default:
                _setListScroll.End();
                _collectionListScroll.End();
                break;
        }

        _activeScrollTarget = null;
        scrollY = ClampScroll(scrollY);
        collectionScrollY = Math.Clamp(collectionScrollY, 0f, MaxCollectionScroll(viewport));
    }

    private static float MinSetScroll(VirtualViewport viewport) => -viewport.VirtualHeight * 0.5f;

    private float MaxSetScroll() =>
        Math.Max(
            0f,
            RowBaseY
                + CalculateTotalScrollHeight()
                - VirtualViewport.AndroidReferenceLandscape.VirtualHeight * 0.5f
        );
}
