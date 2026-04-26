#if ANDROID || IOS
using Microsoft.Maui.ApplicationModel;
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

public sealed partial class PlatformTextInputService : ITextInputService, IDisposable
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
            var input = (InputMethodManager?)
                Platform.CurrentActivity?.GetSystemService(Context.InputMethodService);
            input?.HideSoftInputFromWindow(editText.WindowToken, HideSoftInputFlags.None);
#endif
        });
    }

    public void Dispose() => Detach();

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
