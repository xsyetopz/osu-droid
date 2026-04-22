#if ANDROID
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Microsoft.Maui.ApplicationModel;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using Platform = Microsoft.Maui.ApplicationModel.Platform;

namespace OsuDroid.App.Platform.Input;

public sealed partial class PlatformTextInputService
{
    private void RequestAndroidTextInput(TextInputRequest request)
    {
        EnsureAndroidEditText();
        if (editText is null)
            return;

        ApplyAndroidBounds(request.SurfaceBounds);
        isUpdatingText = true;
        editText.Text = request.Text;
        editText.SetSelection(editText.Text?.Length ?? 0);
        isUpdatingText = false;
        editText.Visibility = ViewStates.Visible;
        editText.RequestFocus();
        var input = (InputMethodManager?)Platform.CurrentActivity?.GetSystemService(Context.InputMethodService);
        input?.ShowSoftInput(editText, ShowFlags.Implicit);
    }

    private void EnsureAndroidEditText()
    {
        if (editText is not null)
            return;

        var activity = Platform.CurrentActivity;
        var root = activity?.Window?.DecorView.RootView as ViewGroup;
        if (root is null)
            return;

        editText = new EditText(activity)
        {
            Alpha = 0.01f,
            Background = null,
            SingleLine = true,
            Visibility = ViewStates.Gone,
        };
        editText.SetTextColor(Color.Transparent);
        editText.TextChanged += OnAndroidTextChanged;
        editText.EditorAction += OnAndroidEditorAction;
        root.AddView(editText, new ViewGroup.LayoutParams(44, 44));
    }

    private void ApplyAndroidBounds(UiRect? surfaceBounds)
    {
        if (editText is null)
            return;

        var bounds = surfaceBounds ?? new UiRect(0f, 0f, 44f, 44f);
        editText.SetX(bounds.X);
        editText.SetY(bounds.Y);
        editText.LayoutParameters = new ViewGroup.LayoutParams(
            Math.Max(44, (int)MathF.Round(bounds.Width)),
            Math.Max(44, (int)MathF.Round(bounds.Height)));
    }

    private void OnAndroidTextChanged(object? sender, Android.Text.TextChangedEventArgs args)
    {
        if (isUpdatingText)
            return;

        activeRequest?.OnTextChanged(args.Text?.ToString() ?? string.Empty);
    }

    private void OnAndroidEditorAction(object? sender, TextView.EditorActionEventArgs args)
    {
        if (activeRequest is null)
            return;

        activeRequest.OnSubmitted(editText?.Text ?? string.Empty);
        HideTextInput();
        args.Handled = true;
    }
}
#endif
