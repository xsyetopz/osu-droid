#if ANDROID || IOS
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRect = Microsoft.Xna.Framework.Rectangle;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed partial class MonoGameUiRenderer
{
    private static void DrawSprite(SpriteBatch spriteBatch, Texture2D texture, XnaRect destination, XnaColor color, float rotationDegrees, UiSpriteFit fit, float rotationOriginX, float rotationOriginY, UiRect? explicitSource)
        {
            var source = explicitSource is null
                ? fit == UiSpriteFit.Stretch ? null : CalculateSourceRect(texture, destination, fit)
                : new XnaRect(
                    (int)MathF.Round(explicitSource.Value.X),
                    (int)MathF.Round(explicitSource.Value.Y),
                    Math.Max(1, (int)MathF.Round(explicitSource.Value.Width)),
                    Math.Max(1, (int)MathF.Round(explicitSource.Value.Height)));

            if (Math.Abs(rotationDegrees) < 0.001f)
            {
                spriteBatch.Draw(texture, destination, source, color);
                return;
            }

            var sourceWidth = source?.Width ?? texture.Width;
            var sourceHeight = source?.Height ?? texture.Height;
            var position = new Vector2(
                destination.X + destination.Width * rotationOriginX,
                destination.Y + destination.Height * rotationOriginY);
            var origin = new Vector2(sourceWidth * rotationOriginX, sourceHeight * rotationOriginY);
            var scale = new Vector2(destination.Width / (float)sourceWidth, destination.Height / (float)sourceHeight);
            spriteBatch.Draw(texture, position, source, color, MathHelper.ToRadians(rotationDegrees), origin, scale, SpriteEffects.None, 0f);
        }

    private static void DrawSpriteClipped(SpriteBatch spriteBatch, Texture2D texture, XnaRect destination, XnaRect? clip, XnaColor color, float rotationDegrees, UiSpriteFit fit, float rotationOriginX, float rotationOriginY, UiRect? explicitSource)
        {
            if (clip is null || Math.Abs(rotationDegrees) > 0.001f)
            {
                DrawSprite(spriteBatch, texture, destination, color, rotationDegrees, fit, rotationOriginX, rotationOriginY, explicitSource);
                return;
            }

            var clipped = Intersect(destination, clip.Value);
            if (clipped.Width <= 0 || clipped.Height <= 0)
                return;

            var source = explicitSource is null
                ? fit == UiSpriteFit.Stretch ? new XnaRect(0, 0, texture.Width, texture.Height) : CalculateSourceRect(texture, destination, fit) ?? new XnaRect(0, 0, texture.Width, texture.Height)
                : new XnaRect(
                    (int)MathF.Round(explicitSource.Value.X),
                    (int)MathF.Round(explicitSource.Value.Y),
                    Math.Max(1, (int)MathF.Round(explicitSource.Value.Width)),
                    Math.Max(1, (int)MathF.Round(explicitSource.Value.Height)));

            var left = (clipped.X - destination.X) / (float)destination.Width;
            var top = (clipped.Y - destination.Y) / (float)destination.Height;
            var right = (destination.Right - clipped.Right) / (float)destination.Width;
            var bottom = (destination.Bottom - clipped.Bottom) / (float)destination.Height;
            var sourceLeft = (int)MathF.Round(source.Width * left);
            var sourceTop = (int)MathF.Round(source.Height * top);
            var sourceRight = (int)MathF.Round(source.Width * right);
            var sourceBottom = (int)MathF.Round(source.Height * bottom);
            var clippedSource = new XnaRect(
                source.X + sourceLeft,
                source.Y + sourceTop,
                Math.Max(1, source.Width - sourceLeft - sourceRight),
                Math.Max(1, source.Height - sourceTop - sourceBottom));
            spriteBatch.Draw(texture, clipped, clippedSource, color);
        }

    private static XnaRect? CalculateSourceRect(Texture2D texture, XnaRect destination, UiSpriteFit fit)
        {
            var textureRatio = texture.Width / (float)texture.Height;
            var destinationRatio = destination.Width / (float)destination.Height;

            if (fit == UiSpriteFit.Contain)
                return null;

            if (destinationRatio > textureRatio)
            {
                var sourceHeight = Math.Max(1, (int)MathF.Round(texture.Width / destinationRatio));
                return new XnaRect(0, (texture.Height - sourceHeight) / 2, texture.Width, sourceHeight);
            }

            var sourceWidth = Math.Max(1, (int)MathF.Round(texture.Height * destinationRatio));
            return new XnaRect((texture.Width - sourceWidth) / 2, 0, sourceWidth, texture.Height);
        }
}
#endif
