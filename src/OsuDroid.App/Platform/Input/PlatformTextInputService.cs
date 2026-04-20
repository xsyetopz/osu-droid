#if ANDROID || IOS
using Microsoft.Maui.ApplicationModel;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
#if IOS
using CoreGraphics;
using Microsoft.Xna.Framework.Input;
using UIKit;
#elif ANDROID
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Platform = Microsoft.Maui.ApplicationModel.Platform;
#endif

namespace OsuDroid.App.Platform.Input;

public sealed class PlatformTextInputService : ITextInputService, IDisposable
{
    private TextInputRequest? activeRequest;
#if IOS
    private UITextField? textField;
    private bool keyboardFallbackActive;
#elif ANDROID
    private EditText? editText;
    private bool isUpdatingText;
#endif

    public void Attach()
    {
        activeRequest = null;
    }

    public void Detach()
    {
        RunOnMainThread(() =>
        {
            activeRequest = null;
#if IOS
            CancelKeyboardFallback();
            if (textField is null)
                return;

            textField.ResignFirstResponder();
            textField.EditingChanged -= OnIosEditingChanged;
            textField.RemoveFromSuperview();
            textField.Dispose();
            textField = null;
#elif ANDROID
            if (editText is null)
                return;

            editText.TextChanged -= OnAndroidTextChanged;
            editText.EditorAction -= OnAndroidEditorAction;
            editText.ClearFocus();
            (editText.Parent as ViewGroup)?.RemoveView(editText);
            editText.Dispose();
            editText = null;
#endif
        });
    }

    public void RequestTextInput(TextInputRequest request)
    {
        activeRequest = request;
        RunOnMainThread(() =>
        {
#if IOS
            RequestIosTextInput(request);
#elif ANDROID
            RequestAndroidTextInput(request);
#endif
        });
    }

    public void HideTextInput()
    {
        activeRequest = null;
        RunOnMainThread(() =>
        {
#if IOS
            textField?.ResignFirstResponder();
            if (textField is not null)
                textField.Hidden = true;
            CancelKeyboardFallback();
#elif ANDROID
            if (editText is null)
                return;

            editText.ClearFocus();
            var input = (InputMethodManager?)Platform.CurrentActivity?.GetSystemService(Context.InputMethodService);
            input?.HideSoftInputFromWindow(editText.WindowToken, HideSoftInputFlags.None);
#endif
        });
    }

    public void Dispose() => Detach();

#if IOS
    private void RequestIosTextInput(TextInputRequest request)
    {
        activeRequest = request;
        CancelKeyboardFallback();
        EnsureIosTextField();
        if (textField is null)
        {
            StartKeyboardFallback(request);
            return;
        }

        ApplyIosBounds(request.SurfaceBounds);
        textField.Text = request.Text;
        textField.Hidden = false;
        textField.UserInteractionEnabled = true;
        textField.Enabled = true;
        textField.Superview?.BringSubviewToFront(textField);

        var focused = textField.BecomeFirstResponder();
        Log($"ios-search-focus focused={focused} frame={textField.Frame}");
        if (!focused)
            StartKeyboardFallback(request);
    }

    private void EnsureIosTextField()
    {
        var window = FindActiveWindow();
        if (window is null)
        {
            Log("ios-search-window-missing");
            return;
        }

        var host = window.RootViewController?.View ?? window;
        if (textField is not null)
        {
            if (textField.Superview != host)
            {
                textField.RemoveFromSuperview();
                host.AddSubview(textField);
            }

            host.BringSubviewToFront(textField);
            return;
        }

        textField = new UITextField(new CGRect(0, 0, 44, 44))
        {
            Alpha = 0.02f,
            AutocorrectionType = UITextAutocorrectionType.No,
            BackgroundColor = UIColor.Clear,
            Enabled = true,
            Hidden = true,
            KeyboardType = UIKeyboardType.Default,
            ReturnKeyType = UIReturnKeyType.Search,
            SpellCheckingType = UITextSpellCheckingType.No,
            TextColor = UIColor.Clear,
            TintColor = UIColor.Clear,
            UserInteractionEnabled = true,
        };
        textField.EditingChanged += OnIosEditingChanged;
        textField.ShouldReturn = SubmitIosText;
        host.AddSubview(textField);
        host.BringSubviewToFront(textField);
        Log($"ios-search-attached host={host.Bounds}");
    }

    private void ApplyIosBounds(UiRect? surfaceBounds)
    {
        if (textField is null)
            return;

        var scale = Math.Max(1d, UIScreen.MainScreen.NativeScale);
        var bounds = surfaceBounds ?? new UiRect(0f, 0f, 44f, 44f);
        textField.Frame = new CGRect(
            bounds.X / scale,
            bounds.Y / scale,
            Math.Max(44d, bounds.Width / scale),
            Math.Max(44d, bounds.Height / scale));
    }

    private void OnIosEditingChanged(object? sender, EventArgs args) =>
        activeRequest?.OnTextChanged(textField?.Text ?? string.Empty);

    private bool SubmitIosText(UITextField field)
    {
        var request = activeRequest;
        if (request is null)
            return true;

        activeRequest = null;
        field.ResignFirstResponder();
        field.Hidden = true;
        Log("ios-search-submit");
        request.OnSubmitted(field.Text ?? string.Empty);
        return true;
    }

    private async void StartKeyboardFallback(TextInputRequest request)
    {
        if (keyboardFallbackActive || !ReferenceEquals(activeRequest, request))
            return;

        keyboardFallbackActive = true;
        Log("ios-search-fallback-open");
        try
        {
            var result = await KeyboardInput.Show("Search beatmaps", string.Empty, request.Text, false).ConfigureAwait(false);
            RunOnMainThread(() =>
            {
                if (!ReferenceEquals(activeRequest, request))
                    return;

                activeRequest = null;
                if (result is null)
                {
                    Log("ios-search-fallback-cancel");
                    request.OnCanceled?.Invoke();
                    return;
                }

                Log("ios-search-fallback-submit");
                request.OnTextChanged(result);
                request.OnSubmitted(result);
            });
        }
        catch (Exception exception)
        {
            Log($"ios-search-fallback-error {exception.GetType().Name}: {exception.Message}");
        }
        finally
        {
            keyboardFallbackActive = false;
        }
    }

    private static void CancelKeyboardFallback()
    {
        try
        {
            if (KeyboardInput.IsVisible)
                KeyboardInput.Cancel(string.Empty);
        }
        catch (Exception)
        {
        }
    }

    private static UIWindow? FindActiveWindow()
    {
        foreach (var scene in UIApplication.SharedApplication.ConnectedScenes)
        {
            if (scene is not UIWindowScene windowScene || scene.ActivationState != UISceneActivationState.ForegroundActive)
                continue;

            UIWindow? activeWindow = null;
            foreach (var window in windowScene.Windows)
            {
                if (window.IsKeyWindow)
                {
                    activeWindow = window;
                    break;
                }
            }

            if (activeWindow is null && windowScene.Windows.Length > 0)
                activeWindow = windowScene.Windows[0];

            if (activeWindow is not null)
                return activeWindow;
        }

        return null;
    }
#elif ANDROID
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
#endif

    private static void RunOnMainThread(Action action)
    {
#if IOS
        UIApplication.SharedApplication.InvokeOnMainThread(action);
#elif ANDROID
        MainThread.BeginInvokeOnMainThread(action);
#endif
    }

    private static void Log(string message)
    {
#if DEBUG
        Console.WriteLine($"osu!droid text-input {message}");
#endif
    }
}
#endif
