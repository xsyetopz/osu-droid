namespace OsuDroid.Game.UI;

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
    UiTextVerticalAlignment VerticalAlignment = UiTextVerticalAlignment.Top);
