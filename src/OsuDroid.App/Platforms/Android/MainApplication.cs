using Android.App;
using Android.Runtime;

namespace OsuDroid.App;

[Application(Icon = "@drawable/ic_launcher")]
public sealed class MainApplication(IntPtr handle, JniHandleOwnership ownership)
    : MauiApplication(handle, ownership)
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
