using OsuDroid.Game.UI.Frames;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport)
    {
        ClampScroll(viewport);
        return CreateSnapshot(viewport, ActiveSectionData, _contentScrollOffset, _sectionScrollOffset);
    }



    public GameFrameSnapshot CreateSnapshotForSection(OptionsSection section, VirtualViewport viewport)
    {
        SettingsSection sectionData = s_sections.Single(settingsSection => settingsSection.Section == section);
        return CreateSnapshot(viewport, sectionData, 0f, 0f);
    }



    private GameFrameSnapshot CreateSnapshot(VirtualViewport viewport, SettingsSection sectionData, float activeContentScrollOffset, float activeSectionScrollOffset)
    {
        return new GameFrameSnapshot(
            "Options",
            _localizer["Options_Title"],
            _localizer["Options_Subtitle"],
            Sections,
            (int)sectionData.Section,
            false,
            CreateUiFrame(viewport, sectionData, activeContentScrollOffset, activeSectionScrollOffset));
    }



    private SettingsSection ActiveSectionData => s_sections.Single(section => section.Section == _activeSection);



    private static IEnumerable<SettingsRow> AllRows() => s_sections.SelectMany(section => section.Categories).SelectMany(category => category.Rows);


}
