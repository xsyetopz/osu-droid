using System.Globalization;
using OsuDroid.Game.UI.Geometry;
using static OsuDroid.Game.UI.Style.DroidUiMetrics;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    public static float MaxScrollOffset(VirtualViewport viewport) => MaxContentScrollOffset(viewport);



    public static float MaxContentScrollOffset(VirtualViewport viewport) => Math.Max(0f, CalculateContentHeight(s_generalCategories) - VisibleContentHeight(viewport));



    public static float MaxSectionScrollOffset(VirtualViewport viewport) => Math.Max(0f, CalculateSectionHeight() - VisibleContentHeight(viewport));



    public static bool IsSectionScrollPoint(UiPoint point) => point.X is >= ContentPaddingX and <= (ContentPaddingX + SectionRailWidth);



    public void Scroll(float deltaY, VirtualViewport viewport) => Scroll(deltaY, new UiPoint(ContentPaddingX + SectionRailWidth + ListGap, ContentTop), viewport);



    public void Scroll(float deltaY, UiPoint point, VirtualViewport viewport)
    {
        if (_activeSliderRowIndex is not null)
        {
            return;
        }

        if (IsSectionScrollPoint(point))
        {
            _sectionScrollOffset = Math.Clamp(_sectionScrollOffset + deltaY, 0f, MaxSectionScrollOffset(viewport));
        }
        else
        {
            _contentScrollOffset = Math.Clamp(_contentScrollOffset + deltaY, 0f, MaxActiveContentScrollOffset(viewport));
        }
    }



    public bool TryBeginScrollDrag(UiPoint point, VirtualViewport viewport, double? timestampSeconds = null)
    {
        if (_activeSliderRowIndex is not null)
        {
            return false;
        }

        if (IsSectionScrollPoint(point) && MaxSectionScrollOffset(viewport) > 0f)
        {
            _activeScrollTarget = OptionsScrollTarget.Sections;
            _sectionScroll.Begin(point, timestampSeconds ?? _elapsedSeconds);
            return true;
        }

        if (!IsSectionScrollPoint(point) && MaxActiveContentScrollOffset(viewport) > 0f)
        {
            _activeScrollTarget = OptionsScrollTarget.Content;
            _contentScroll.Begin(point, timestampSeconds ?? _elapsedSeconds);
            return true;
        }

        return false;
    }



    public bool UpdateScrollDrag(UiPoint point, VirtualViewport viewport, double? timestampSeconds = null)
    {
        double timestamp = timestampSeconds ?? _elapsedSeconds;
        return _activeScrollTarget switch
        {
            OptionsScrollTarget.Sections => _sectionScroll.Drag(point, timestamp, () => _sectionScrollOffset, value => _sectionScrollOffset = value, 0f, MaxSectionScrollOffset(viewport)),
            OptionsScrollTarget.Content => _contentScroll.Drag(point, timestamp, () => _contentScrollOffset, value => _contentScrollOffset = value, 0f, MaxActiveContentScrollOffset(viewport)),
            _ => false,
        };
    }



    public void EndScrollDrag(UiPoint point, VirtualViewport viewport, double? timestampSeconds = null)
    {
        double timestamp = timestampSeconds ?? _elapsedSeconds;
        switch (_activeScrollTarget)
        {
            case OptionsScrollTarget.Sections:
                _sectionScroll.End(point, timestamp, () => _sectionScrollOffset, value => _sectionScrollOffset = value, 0f, MaxSectionScrollOffset(viewport));
                _contentScroll.End();
                break;
            case OptionsScrollTarget.Content:
                _contentScroll.End(point, timestamp, () => _contentScrollOffset, value => _contentScrollOffset = value, 0f, MaxActiveContentScrollOffset(viewport));
                _sectionScroll.End();
                break;
            default:
                _sectionScroll.End();
                _contentScroll.End();
                break;
        }

        _activeScrollTarget = null;
        ClampScroll(viewport);
    }



    public bool TryBeginSliderDrag(string elementId, UiPoint point, VirtualViewport viewport)
    {
        if (!TryParseSliderRowIndex(elementId, out int rowIndex))
        {
            return false;
        }

        SettingsRow? row = RowAt(rowIndex);
        if (row?.Kind != SettingsRowKind.Slider || !IsInteractive(row))
        {
            return false;
        }

        _activeSliderRowIndex = rowIndex;
        return UpdateSliderDrag(point, viewport);
    }



    public bool UpdateSliderDrag(UiPoint point, VirtualViewport viewport)
    {
        if (_activeSliderRowIndex is not int rowIndex)
        {
            return false;
        }

        SettingsRow? row = RowAt(rowIndex);
        UiRect? bounds = FindRowBounds(rowIndex, viewport);
        if (row is null || bounds is null)
        {
            return false;
        }

        int next = SliderValueAtPoint(row, bounds.Value, point.X);
        if (GetIntValue(row.Key) == next)
        {
            return true;
        }

        _intValues[row.Key] = next;
        _settingsStore?.SetInt(row.Key, next);
        _changedSettingKey = row.Key;
        return true;
    }



    public void EndSliderDrag(UiPoint point, VirtualViewport viewport)
    {
        UpdateSliderDrag(point, viewport);
        _activeSliderRowIndex = null;
    }



    private UiRect? FindRowBounds(int targetRowIndex, VirtualViewport viewport)
    {
        float listX = ContentPaddingX + SectionRailWidth + ListGap;
        float listWidth = viewport.VirtualWidth - listX - ContentPaddingX;
        float y = ContentTop - _contentScrollOffset;
        int rowIndex = 0;

        foreach (SettingsCategory category in ActiveSectionData.Categories)
        {
            y += CategoryTopMargin + CategoryHeaderHeight;
            foreach (SettingsRow row in category.Rows)
            {
                float rowHeight = GetRowHeight(row);
                if (rowIndex == targetRowIndex)
                {
                    return new UiRect(listX, y, listWidth, rowHeight);
                }

                y += rowHeight;
                rowIndex++;
            }
        }

        return null;
    }



    private void ClampScroll(VirtualViewport viewport)
    {
        _contentScrollOffset = Math.Clamp(_contentScrollOffset, 0f, MaxActiveContentScrollOffset(viewport));
        _sectionScrollOffset = Math.Clamp(_sectionScrollOffset, 0f, MaxSectionScrollOffset(viewport));
    }



    private float MaxActiveContentScrollOffset(VirtualViewport viewport) => Math.Max(0f, CalculateContentHeight(ActiveSectionData.Categories) - VisibleContentHeight(viewport));



    private SettingsRow? RowAt(int rowIndex)
    {
        SettingsRow[] rows = ActiveSectionData.Categories.SelectMany(category => category.Rows).ToArray();
        return (uint)rowIndex < (uint)rows.Length ? rows[rowIndex] : null;
    }



    private static bool TryParseSliderRowIndex(string elementId, out int rowIndex)
    {
        rowIndex = -1;
        const string prefix = "options-row-";
        const string infix = "-slider-";
        if (!elementId.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        int suffixIndex = elementId.IndexOf(infix, prefix.Length, StringComparison.Ordinal);
        return suffixIndex < 0
            ? false
            : int.TryParse(elementId.AsSpan(prefix.Length, suffixIndex - prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out rowIndex);
    }



    private static int SliderValueAtPoint(SettingsRow row, UiRect bounds, float pointX)
    {
        float trackWidth = bounds.Width - SeekbarTrackMarginX * 2f;
        float trackX = bounds.X + SeekbarTrackMarginX;
        float normalized = Math.Clamp((pointX - trackX) / trackWidth, 0f, 1f);
        return ClampSliderValue(row, (int)MathF.Round(row.Min + normalized * (row.Max - row.Min)));
    }


}
