#if IOS
using CoreGraphics;
using Microsoft.Xna.Framework.Input;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using UIKit;

namespace OsuDroid.App.Platform.Input;

public sealed partial class PlatformTextInputService
{
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
}
#endif
