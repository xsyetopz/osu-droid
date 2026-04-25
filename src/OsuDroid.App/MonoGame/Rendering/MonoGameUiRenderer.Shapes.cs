#if ANDROID || IOS
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRect = Microsoft.Xna.Framework.Rectangle;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed partial class MonoGameUiRenderer
{
    private void DrawFillClipped(
            SpriteBatch spriteBatch,
            XnaRect bounds,
            XnaRect? clip,
            XnaColor color,
            float radius,
            UiCornerMode cornerMode = UiCornerMode.All,
            RenderCacheMetrics? metrics = null,
            float rotationDegrees = 0f,
            float rotationOriginX = 0.5f,
            float rotationOriginY = 0.5f)
        {
            if (clip is null || Math.Abs(rotationDegrees) > 0.001f)
            {
                DrawFill(spriteBatch, bounds, color, radius, cornerMode, metrics, rotationDegrees, rotationOriginX, rotationOriginY);
                return;
            }

            var clipped = Intersect(bounds, clip.Value);
            if (pixel is null || clipped.Width <= 0 || clipped.Height <= 0 || color.A == 0)
                return;

            if (radius <= 1f || cornerMode == UiCornerMode.None)
            {
                spriteBatch.Draw(pixel, clipped, color);
                return;
            }

            var texture = shapeStore.GetRoundedFill(bounds.Width, bounds.Height, radius, cornerMode, color, metrics);
            var source = new XnaRect(clipped.X - bounds.X, clipped.Y - bounds.Y, clipped.Width, clipped.Height);
            spriteBatch.Draw(texture, clipped, source, XnaColor.White);
        }
}
#endif
