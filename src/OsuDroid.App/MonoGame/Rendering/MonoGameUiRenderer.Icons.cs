#if ANDROID || IOS
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OsuDroid.Game.UI;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRect = Microsoft.Xna.Framework.Rectangle;

namespace OsuDroid.App.MonoGame.Rendering;

internal sealed partial class MonoGameUiRenderer
{
    private void DrawIcon(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color, UiIcon icon)
    {
        switch (icon)
        {
            case UiIcon.BackArrow:
                DrawLine(spriteBatch, bounds.Right - bounds.Width * 0.25f, bounds.Y + bounds.Height * 0.15f, bounds.X + bounds.Width * 0.25f, bounds.Y + bounds.Height * 0.5f, color, bounds.Width * 0.12f);
                DrawLine(spriteBatch, bounds.X + bounds.Width * 0.25f, bounds.Y + bounds.Height * 0.5f, bounds.Right - bounds.Width * 0.25f, bounds.Bottom - bounds.Height * 0.15f, color, bounds.Width * 0.12f);
                break;

            case UiIcon.Check:
                DrawLine(spriteBatch, bounds.X + bounds.Width * 0.18f, bounds.Y + bounds.Height * 0.52f, bounds.X + bounds.Width * 0.42f, bounds.Bottom - bounds.Height * 0.22f, color, bounds.Width * 0.11f);
                DrawLine(spriteBatch, bounds.X + bounds.Width * 0.42f, bounds.Bottom - bounds.Height * 0.22f, bounds.Right - bounds.Width * 0.14f, bounds.Y + bounds.Height * 0.2f, color, bounds.Width * 0.11f);
                break;

            case UiIcon.CheckboxChecked:
                DrawFill(spriteBatch, bounds, color, bounds.Width * 0.08f);
                DrawIcon(spriteBatch, Inset(bounds, bounds.Width * 0.12f), XnaColor.Black * 0.75f, UiIcon.Check);
                break;

            case UiIcon.CheckboxUnchecked:
                DrawBorder(spriteBatch, bounds, color, Math.Max(2, bounds.Width / 12));
                break;

            case UiIcon.ChevronRight:
                DrawLine(spriteBatch, bounds.X + bounds.Width * 0.35f, bounds.Y + bounds.Height * 0.2f, bounds.Right - bounds.Width * 0.28f, bounds.Y + bounds.Height * 0.5f, color, bounds.Width * 0.12f);
                DrawLine(spriteBatch, bounds.Right - bounds.Width * 0.28f, bounds.Y + bounds.Height * 0.5f, bounds.X + bounds.Width * 0.35f, bounds.Bottom - bounds.Height * 0.2f, color, bounds.Width * 0.12f);
                break;

            case UiIcon.ChevronDown:
                DrawLine(spriteBatch, bounds.X + bounds.Width * 0.18f, bounds.Y + bounds.Height * 0.35f, bounds.X + bounds.Width * 0.5f, bounds.Bottom - bounds.Height * 0.25f, color, bounds.Width * 0.12f);
                DrawLine(spriteBatch, bounds.X + bounds.Width * 0.5f, bounds.Bottom - bounds.Height * 0.25f, bounds.Right - bounds.Width * 0.18f, bounds.Y + bounds.Height * 0.35f, color, bounds.Width * 0.12f);
                break;

            case UiIcon.Grid:
                DrawGrid(spriteBatch, bounds, color);
                break;

            case UiIcon.Square:
                DrawBorder(spriteBatch, CenterSquare(bounds), color, Math.Max(2, bounds.Width / 10));
                break;

            case UiIcon.Display:
                DrawDisplay(spriteBatch, bounds, color);
                break;

            case UiIcon.Headphones:
                DrawHeadphones(spriteBatch, bounds, color);
                break;

            case UiIcon.MusicLibrary:
                DrawMusic(spriteBatch, bounds, color);
                break;

            case UiIcon.Input:
                DrawInput(spriteBatch, bounds, color);
                break;

            case UiIcon.Gear:
                DrawGear(spriteBatch, bounds, color);
                break;
        }
    }

    private void DrawGrid(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color)
    {
        var cell = Math.Max(3, bounds.Width / 5);
        var gap = Math.Max(2, bounds.Width / 8);
        var startX = bounds.X + (bounds.Width - cell * 3 - gap * 2) / 2;
        var startY = bounds.Y + (bounds.Height - cell * 3 - gap * 2) / 2;
        for (var y = 0; y < 3; y++)
        for (var x = 0; x < 3; x++)
            DrawFill(spriteBatch, new XnaRect(startX + x * (cell + gap), startY + y * (cell + gap), cell, cell), color, 0f);
    }

    private void DrawDisplay(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color)
    {
        var screen = new XnaRect(bounds.X + bounds.Width / 8, bounds.Y + bounds.Height / 6, bounds.Width * 3 / 4, bounds.Height / 2);
        DrawBorder(spriteBatch, screen, color, Math.Max(2, bounds.Width / 12));
        DrawFill(spriteBatch, new XnaRect(bounds.X + bounds.Width * 2 / 5, screen.Bottom, bounds.Width / 5, bounds.Height / 6), color, 0f);
        DrawFill(spriteBatch, new XnaRect(bounds.X + bounds.Width / 3, bounds.Bottom - bounds.Height / 8, bounds.Width / 3, Math.Max(2, bounds.Height / 10)), color, 0f);
    }

    private void DrawHeadphones(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color)
    {
        var thickness = Math.Max(2, bounds.Width / 10);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.25f, bounds.Y + bounds.Height * 0.5f, bounds.X + bounds.Width * 0.25f, bounds.Bottom - bounds.Height * 0.18f, color, thickness);
        DrawLine(spriteBatch, bounds.Right - bounds.Width * 0.25f, bounds.Y + bounds.Height * 0.5f, bounds.Right - bounds.Width * 0.25f, bounds.Bottom - bounds.Height * 0.18f, color, thickness);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.25f, bounds.Y + bounds.Height * 0.5f, bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.22f, color, thickness);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.22f, bounds.Right - bounds.Width * 0.25f, bounds.Y + bounds.Height * 0.5f, color, thickness);
    }

    private void DrawMusic(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color)
    {
        var thickness = Math.Max(2, bounds.Width / 10);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.62f, bounds.Y + bounds.Height * 0.18f, bounds.X + bounds.Width * 0.62f, bounds.Bottom - bounds.Height * 0.28f, color, thickness);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.62f, bounds.Y + bounds.Height * 0.18f, bounds.Right - bounds.Width * 0.2f, bounds.Y + bounds.Height * 0.26f, color, thickness);
        DrawCircle(spriteBatch, bounds.X + bounds.Width * 0.45f, bounds.Bottom - bounds.Height * 0.25f, Math.Max(4, bounds.Width / 6), color);
    }

    private void DrawInput(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color)
    {
        var thickness = Math.Max(2, bounds.Width / 12);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.15f, bounds.Bottom - bounds.Height * 0.25f, bounds.Right - bounds.Width * 0.15f, bounds.Bottom - bounds.Height * 0.25f, color, thickness);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.25f, bounds.Bottom - bounds.Height * 0.25f, bounds.X + bounds.Width * 0.35f, bounds.Y + bounds.Height * 0.3f, color, thickness);
        DrawLine(spriteBatch, bounds.X + bounds.Width * 0.35f, bounds.Y + bounds.Height * 0.3f, bounds.Right - bounds.Width * 0.2f, bounds.Y + bounds.Height * 0.3f, color, thickness);
    }

    private void DrawGear(SpriteBatch spriteBatch, XnaRect bounds, XnaColor color)
    {
        DrawBorder(spriteBatch, Inset(bounds, bounds.Width * 0.2f), color, Math.Max(2, bounds.Width / 10));
        var center = CenterSquare(Inset(bounds, bounds.Width * 0.35f));
        DrawFill(spriteBatch, center, color, center.Width / 2f);
    }

}
#endif
