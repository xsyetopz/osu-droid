namespace OsuDroid.Game.Scenes.ModSelect;

public enum ModStatDirection
{
    Unchanged,
    Increased,
    Decreased,
    DifficultyAdjust,
}

public sealed record ModStatSnapshot(
    float ApproachRate,
    float OverallDifficulty,
    float CircleSize,
    float HpDrainRate,
    float? DroidStarRating,
    float? StandardStarRating,
    float BpmMax,
    float BpmMin,
    float MostCommonBpm,
    long Length,
    float ScoreMultiplier,
    bool IsRanked,
    bool HasChangedStats,
    ModStatDirection ApproachRateDirection,
    ModStatDirection OverallDifficultyDirection,
    ModStatDirection CircleSizeDirection,
    ModStatDirection HpDrainRateDirection,
    ModStatDirection BpmDirection,
    ModStatDirection DifficultyLineDirection
);
