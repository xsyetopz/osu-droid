namespace OsuDroid.App.Host;

public static class MobileHostContract
{
    public const string Runtime = ".NET 9 MAUI host";
    public const string RenderSurface = "MonoGame";
    public static readonly string[] Platforms = ["Android", "iOS"];
}
