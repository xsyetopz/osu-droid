using OsuDroid.Game.UI.Geometry;
namespace OsuDroid.Game.UI.Input;

public interface ITextInputService
{
    void RequestTextInput(TextInputRequest request);

    void HideTextInput();
}

public sealed record TextInputRequest(
    string Text,
    Action<string> OnTextChanged,
    Action<string> OnSubmitted,
    UiRect? SurfaceBounds = null,
    Action? OnCanceled = null,
    string? Title = null);

public sealed class NoOpTextInputService : ITextInputService
{
    public void RequestTextInput(TextInputRequest request)
    {
    }

    public void HideTextInput()
    {
    }
}
