namespace OsuDroid.Game.UI.Elements;

public enum UiTextAlignment
{
    Left,
    Center,
    Right,
}

public enum UiTextVerticalAlignment
{
    Top,
    Middle,
}

public sealed record UiTextStyle(
    float Size,
    bool Bold = false,
    UiTextAlignment Alignment = UiTextAlignment.Left,
    bool Underline = false,
    UiTextVerticalAlignment VerticalAlignment = UiTextVerticalAlignment.Top,
    UiTextAutoScroll? AutoScroll = null
);

public sealed record UiTextAutoScroll(
    double ElapsedSeconds,
    float Speed = 15f,
    float TimeoutSeconds = 3f
);
