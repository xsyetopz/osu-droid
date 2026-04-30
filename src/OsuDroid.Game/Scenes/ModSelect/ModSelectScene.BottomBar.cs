using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.ModSelect;

public sealed partial class ModSelectScene
{
    private void AddBottomBar(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float y = viewport.VirtualHeight - 56f;
        float leftX = SidePadding;
        ModStatSnapshot stats = CurrentStats();
        leftX = AddLabeledBadge(
            elements,
            "ar",
            "AR",
            FormatStat(stats.ApproachRate),
            leftX,
            y,
            stats.ApproachRateDirection
        );
        leftX = AddLabeledBadge(
            elements,
            "od",
            "OD",
            FormatStat(stats.OverallDifficulty),
            leftX + 10f,
            y,
            stats.OverallDifficultyDirection
        );
        leftX = AddLabeledBadge(
            elements,
            "cs",
            "CS",
            FormatStat(stats.CircleSize),
            leftX + 10f,
            y,
            stats.CircleSizeDirection
        );
        leftX = AddLabeledBadge(
            elements,
            "hp",
            "HP",
            FormatStat(stats.HpDrainRate),
            leftX + 10f,
            y,
            stats.HpDrainRateDirection
        );
        AddLabeledBadge(
            elements,
            "bpm",
            "BPM",
            stats.MostCommonBpm.ToString("0", System.Globalization.CultureInfo.InvariantCulture),
            leftX + 10f,
            y,
            stats.BpmDirection
        );

        string scoreValue =
            stats.ScoreMultiplier.ToString(
                "0.00",
                System.Globalization.CultureInfo.InvariantCulture
            ) + "x";
        float scoreWidth = LabeledBadgeWidth("Score", scoreValue);
        float scoreX = viewport.VirtualWidth - SidePadding - scoreWidth;
        AddLabeledBadge(elements, "score", "Score", scoreValue, scoreX, y);
        UiColor rankedFill = stats.IsRanked ? s_ranked : s_button;
        UiColor rankedText = stats.IsRanked ? s_rankedText : s_accent;
        string rankedTextValue = stats.IsRanked ? "Ranked" : "Unranked";
        float rankedWidth = BadgeWidth(rankedTextValue);
        float rankedX = scoreX - 10f - rankedWidth;
        elements.Add(
            Fill(
                "modselect-ranked-badge",
                new UiRect(rankedX, y, rankedWidth, 44f),
                rankedFill,
                1f,
                radius: 12f
            )
        );
        elements.Add(
            Text(
                "modselect-ranked-badge-text",
                rankedTextValue,
                new UiRect(rankedX + 12f, y + 8f, rankedWidth - 24f, 28f),
                18f,
                rankedText,
                bold: true,
                alignment: UiTextAlignment.Center
            )
        );

        float starValue = stats.DroidStarRating ?? stats.StandardStarRating ?? 0f;
        UiColor starFill = StarRatingColor(starValue);
        UiColor starText = starValue >= 6.5f ? StarRatingTextColor(starValue) : s_black;
        float starAlpha = starValue >= 6.5f ? 1f : 0.75f;
        string starValueText = FormatStat(starValue);
        float starWidth = 88f;
        float starX = rankedX - 10f - starWidth;
        elements.Add(
            Fill(
                "modselect-star-badge",
                new UiRect(starX, y, starWidth, 44f),
                starFill,
                starAlpha,
                radius: 12f
            )
        );
        elements.Add(
            UiElementFactory.Sprite(
                "modselect-star-icon",
                DroidAssets.SongSelectStar,
                new UiRect(starX + 12f, y + 12f, 20f, 20f),
                starText,
                1f,
                spriteFit: UiSpriteFit.Contain
            )
        );
        elements.Add(
            Text(
                "modselect-star-value",
                starValueText,
                new UiRect(starX + 36f, y + 9f, starWidth - 44f, 24f),
                18f,
                starText,
                alignment: UiTextAlignment.Center
            )
        );
    }
}
