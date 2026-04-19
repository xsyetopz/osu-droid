#if ANDROID || IOS
using XnaRect = Microsoft.Xna.Framework.Rectangle;

namespace OsuDroid.App.MonoGame.Rendering;

internal readonly record struct RenderBoundsDiagnostics(
    XnaRect ViewportBounds,
    XnaRect ClientBounds,
    int DisplayWidth,
    int DisplayHeight,
    int PreferredBackBufferWidth,
    int PreferredBackBufferHeight);
#endif
