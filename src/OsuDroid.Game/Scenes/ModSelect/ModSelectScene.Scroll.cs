using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Scrolling;
using OsuDroid.Game.UI.Style;
namespace OsuDroid.Game.Scenes.ModSelect;

internal enum ModScrollAxis
{
    Horizontal,
    Vertical,
    Undecided,
}

internal sealed record ScrollDragTarget(string Key, ModScrollAxis Axis, UiPoint LastPoint, double LastTimestampSeconds);

public sealed partial class ModSelectScene
{
    public void Scroll(float deltaX, float deltaY, UiPoint startPoint, VirtualViewport viewport)
    {
        _lastViewport = viewport;
        if (SelectedModsBounds().Contains(startPoint))
        {
            _selectedModsVelocityX = 0f;
            _selectedModsScrollX = Math.Clamp(_selectedModsScrollX + deltaX, 0f, MaxSelectedModsScroll());
            ShowSelectedModsScrollbar();
            return;
        }

        UiRect rail = SectionRailBounds(viewport);
        if (!rail.Contains(startPoint))
        {
            return;
        }

        if (MathF.Abs(deltaX) > MathF.Abs(deltaY))
        {
            _railVelocityX = 0f;
            _sectionRailScrollX = Math.Clamp(_sectionRailScrollX + deltaX, 0f, MaxSectionRailScroll(viewport));
            ShowRailScrollbar();
            return;
        }

        string? sectionKey = SectionKeyAt(startPoint, viewport);
        if (sectionKey is null)
        {
            return;
        }

        _sectionVelocities[sectionKey] = 0f;
        _sectionScrolls[sectionKey] = Math.Clamp(SectionScroll(sectionKey) + deltaY, 0f, MaxSectionScroll(sectionKey, viewport));
        ShowSectionScrollbar(sectionKey);
    }



    public bool TryBeginScrollDrag(UiPoint point, VirtualViewport viewport, double? timestampSeconds = null)
    {
        _lastViewport = viewport;
        double timestamp = timestampSeconds ?? _elapsedSeconds;
        if (SelectedModsBounds().Contains(point) && MaxSelectedModsScroll() > 0f)
        {
            _selectedModsVelocityX = 0f;
            _dragTarget = new ScrollDragTarget("selected", ModScrollAxis.Horizontal, point, timestamp);
            ShowSelectedModsScrollbar();
            return true;
        }

        if (!SectionRailBounds(viewport).Contains(point))
        {
            return false;
        }

        string? sectionKey = SectionKeyAt(point, viewport);
        if (sectionKey is not null && (MaxSectionScroll(sectionKey, viewport) > 0f || MaxSectionRailScroll(viewport) > 0f))
        {
            _sectionVelocities[sectionKey] = 0f;
            _railVelocityX = 0f;
            _dragTarget = new ScrollDragTarget(sectionKey, ModScrollAxis.Undecided, point, timestamp);
            return true;
        }

        if (MaxSectionRailScroll(viewport) <= 0f)
        {
            return false;
        }

        _railVelocityX = 0f;
        _dragTarget = new ScrollDragTarget("rail", ModScrollAxis.Horizontal, point, timestamp);
        ShowRailScrollbar();
        return true;
    }



    public bool UpdateScrollDrag(UiPoint point, VirtualViewport viewport, double? timestampSeconds = null)
    {
        _lastViewport = viewport;
        if (_dragTarget is not { } target)
        {
            return false;
        }

        double timestamp = timestampSeconds ?? _elapsedSeconds;
        float deltaX = target.LastPoint.X - point.X;
        float deltaY = target.LastPoint.Y - point.Y;
        float elapsed = Math.Max(1f / 120f, (float)(timestamp - target.LastTimestampSeconds));

        if (target.Key == "selected")
        {
            _selectedModsScrollX = Math.Clamp(_selectedModsScrollX + deltaX, 0f, MaxSelectedModsScroll());
            _selectedModsVelocityX = Math.Clamp(deltaX / elapsed, -DroidUiTheme.Scroll.MaxVelocity, DroidUiTheme.Scroll.MaxVelocity);
            ShowSelectedModsScrollbar();
        }
        else if (target.Key == "rail" || target.Axis == ModScrollAxis.Horizontal || (target.Axis == ModScrollAxis.Undecided && MathF.Abs(deltaX) > MathF.Abs(deltaY)))
        {
            _sectionRailScrollX = Math.Clamp(_sectionRailScrollX + deltaX, 0f, MaxSectionRailScroll(viewport));
            _railVelocityX = Math.Clamp(deltaX / elapsed, -DroidUiTheme.Scroll.MaxVelocity, DroidUiTheme.Scroll.MaxVelocity);
            ShowRailScrollbar();
            target = target with { Key = "rail", Axis = ModScrollAxis.Horizontal };
        }
        else
        {
            _sectionScrolls[target.Key] = Math.Clamp(SectionScroll(target.Key) + deltaY, 0f, MaxSectionScroll(target.Key, viewport));
            _sectionVelocities[target.Key] = Math.Clamp(deltaY / elapsed, -DroidUiTheme.Scroll.MaxVelocity, DroidUiTheme.Scroll.MaxVelocity);
            ShowSectionScrollbar(target.Key);
            target = target with { Axis = ModScrollAxis.Vertical };
        }

        _dragTarget = target with { LastPoint = point, LastTimestampSeconds = timestamp };
        return true;
    }



    public void EndScrollDrag(UiPoint point, VirtualViewport viewport, double? timestampSeconds = null)
    {
        _lastViewport = viewport;
        if (_dragTarget is null)
        {
            return;
        }

        UpdateScrollDrag(point, viewport, timestampSeconds);
        ClampAllScrolls(viewport);
        _dragTarget = null;
    }



    private void ClampAllScrolls(VirtualViewport viewport)
    {
        _sectionRailScrollX = Math.Clamp(_sectionRailScrollX, 0f, MaxSectionRailScroll(viewport));
        _selectedModsScrollX = Math.Clamp(_selectedModsScrollX, 0f, MaxSelectedModsScroll());
        foreach (string sectionKey in _sectionScrolls.Keys.ToArray())
        {
            _sectionScrolls[sectionKey] = Math.Clamp(_sectionScrolls[sectionKey], 0f, MaxSectionScroll(sectionKey, viewport));
        }
    }



    private float MaxSectionRailScroll(VirtualViewport viewport)
    {
        int sectionCount = VisibleEntries().GroupBy(entry => entry.SectionKey).Count();

        float contentWidth = SidePadding * 2f + PresetSectionWidth + SectionGap + sectionCount * SectionWidth + Math.Max(0, sectionCount - 1) * SectionGap;
        return Math.Max(0f, contentWidth - viewport.VirtualWidth);
    }



    private float MaxSectionScroll(string sectionKey, VirtualViewport viewport)
    {
        int entryCount = string.Equals(sectionKey, "presets", StringComparison.Ordinal)
            ? VisiblePresets().Count() + 1
            : VisibleEntries().Count(entry => string.Equals(entry.SectionKey, sectionKey, StringComparison.Ordinal));
        float contentHeight = entryCount * ToggleHeight + Math.Max(0, entryCount - 1) * ToggleGap + 12f;
        float listHeight = viewport.VirtualHeight - TopBarHeight - BottomBarHeight - SectionHeaderHeight - 12f;
        return Math.Max(0f, contentHeight - listHeight);
    }



    private float MaxSelectedModsScroll()
    {
        int count = _selectedAcronyms.Count;
        float contentWidth = count == 0 ? 0f : count * SelectedModIconSize + Math.Max(0, count - 1) * SelectedModIconSpacing;
        return Math.Max(0f, contentWidth - SelectedModsBounds().Width);
    }



    private float SectionScroll(string sectionKey) => _sectionScrolls.TryGetValue(sectionKey, out float scroll) ? scroll : 0f;



    private string? SectionKeyAt(UiPoint point, VirtualViewport viewport)
    {
        float x = SidePadding - _sectionRailScrollX;
        if (new UiRect(x, TopBarHeight, PresetSectionWidth, viewport.VirtualHeight - TopBarHeight - BottomBarHeight).Contains(point))
        {
            return "presets";
        }

        x += PresetSectionWidth + SectionGap;
        foreach (IGrouping<string, ModCatalogEntry> section in VisibleEntries().GroupBy(entry => entry.SectionKey))
        {
            if (new UiRect(x, TopBarHeight, SectionWidth, viewport.VirtualHeight - TopBarHeight - BottomBarHeight).Contains(point))
            {
                return section.Key;
            }

            x += SectionWidth + SectionGap;
        }

        return null;
    }



    private void UpdateScrollInertia(float elapsedSeconds, VirtualViewport viewport)
    {
        if (_dragTarget is not null || elapsedSeconds <= 0f)
        {
            return;
        }

        if (MathF.Abs(_railVelocityX) > DroidUiTheme.Scroll.VelocityStop)
        {
            _sectionRailScrollX = Math.Clamp(_sectionRailScrollX + _railVelocityX * elapsedSeconds, 0f, MaxSectionRailScroll(viewport));
            _railVelocityX = DecayVelocity(_railVelocityX, elapsedSeconds);
            ShowRailScrollbar();
        }
        else
        {
            _railVelocityX = 0f;
        }

        if (MathF.Abs(_selectedModsVelocityX) > DroidUiTheme.Scroll.VelocityStop)
        {
            _selectedModsScrollX = Math.Clamp(_selectedModsScrollX + _selectedModsVelocityX * elapsedSeconds, 0f, MaxSelectedModsScroll());
            _selectedModsVelocityX = DecayVelocity(_selectedModsVelocityX, elapsedSeconds);
            ShowSelectedModsScrollbar();
        }
        else
        {
            _selectedModsVelocityX = 0f;
        }

        foreach (KeyValuePair<string, float> sectionVelocity in _sectionVelocities.ToArray())
        {
            string sectionKey = sectionVelocity.Key;
            float velocity = sectionVelocity.Value;
            if (MathF.Abs(velocity) <= DroidUiTheme.Scroll.VelocityStop)
            {
                _sectionVelocities[sectionKey] = 0f;
                continue;
            }

            _sectionScrolls[sectionKey] = Math.Clamp(SectionScroll(sectionKey) + velocity * elapsedSeconds, 0f, MaxSectionScroll(sectionKey, viewport));
            _sectionVelocities[sectionKey] = DecayVelocity(velocity, elapsedSeconds);
            ShowSectionScrollbar(sectionKey);
        }
    }



    private static float DecayVelocity(float velocity, float elapsedSeconds)
    {
        float decay = MathF.Pow(DroidUiTheme.Scroll.DecelerationPerFrame, elapsedSeconds * 60f);
        return velocity * decay;
    }



    private static void AddHorizontalScrollbar(List<UiElementSnapshot> elements, string id, UiRect viewportBounds, float scroll, float maxScroll, bool visible)
    {
        if (!visible)
        {
            return;
        }

        UiElementSnapshot? indicator = DroidScrollIndicator.Horizontal($"{id}-scrollbar", viewportBounds, scroll, maxScroll, s_text);
        if (indicator is not null)
        {
            elements.Add(indicator);
        }
    }



    private void AddVerticalScrollbar(List<UiElementSnapshot> elements, string id, UiRect sectionBounds, string sectionKey, VirtualViewport viewport)
    {
        float maxScroll = MaxSectionScroll(sectionKey, viewport);
        if (!IsSectionScrollbarVisible(sectionKey) || maxScroll <= 0f)
        {
            return;
        }

        UiRect clip = ListClipBounds(sectionBounds);
        UiElementSnapshot? indicator = DroidScrollIndicator.Vertical($"{id}-scrollbar", clip, SectionScroll(sectionKey), maxScroll, s_text);
        if (indicator is not null)
        {
            elements.Add(indicator);
        }
    }



    private bool IsRailScrollbarVisible() => _elapsedSeconds <= _railScrollbarVisibleUntil;



    private bool IsSelectedModsScrollbarVisible() => _elapsedSeconds <= _selectedModsScrollbarVisibleUntil;



    private bool IsSectionScrollbarVisible(string sectionKey) =>
            _sectionScrollbarVisibleUntil.TryGetValue(sectionKey, out double visibleUntil) && _elapsedSeconds <= visibleUntil;



    private void ShowRailScrollbar() => _railScrollbarVisibleUntil = _elapsedSeconds + DroidUiTheme.Scroll.IndicatorVisibleSeconds;



    private void ShowSelectedModsScrollbar() => _selectedModsScrollbarVisibleUntil = _elapsedSeconds + DroidUiTheme.Scroll.IndicatorVisibleSeconds;



    private void ShowSectionScrollbar(string sectionKey) => _sectionScrollbarVisibleUntil[sectionKey] = _elapsedSeconds + DroidUiTheme.Scroll.IndicatorVisibleSeconds;


}
