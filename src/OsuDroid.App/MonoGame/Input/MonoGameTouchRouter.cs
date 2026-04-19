#if ANDROID || IOS
using Microsoft.Xna.Framework.Input.Touch;
using OsuDroid.Game;
using OsuDroid.Game.UI;

namespace OsuDroid.App.MonoGame.Input;

internal sealed class MonoGameTouchRouter(OsuDroidGameCore core)
{
    private const float TouchDragThreshold = 10f;

    private int? activeTouchId;
    private UiPoint touchStart;
    private UiPoint previousTouch;
    private bool isTouchDragging;

    public bool IsPointerActive => activeTouchId is not null || isTouchDragging;

    public void Route(UiFrameSnapshot currentFrame)
    {
        foreach (var touch in TouchPanel.GetState())
        {
            var virtualPoint = currentFrame.Viewport.ToVirtual(touch.Position.X, touch.Position.Y);
            if (touch.State == TouchLocationState.Pressed)
            {
                activeTouchId = touch.Id;
                touchStart = virtualPoint;
                previousTouch = virtualPoint;
                isTouchDragging = false;
                var pressedElement = currentFrame.HitTest(virtualPoint);
                core.PressUiAction(pressedElement?.Action ?? UiAction.None);
                continue;
            }

            if (activeTouchId != touch.Id)
                continue;

            if (touch.State == TouchLocationState.Moved)
            {
                var movedX = virtualPoint.X - touchStart.X;
                var movedY = virtualPoint.Y - touchStart.Y;
                if (!isTouchDragging && MathF.Sqrt(movedX * movedX + movedY * movedY) > TouchDragThreshold)
                {
                    isTouchDragging = true;
                    core.ReleaseUiAction();
                }

                if (isTouchDragging)
                    core.ScrollActiveScene(previousTouch.Y - virtualPoint.Y, touchStart, currentFrame.Viewport);

                previousTouch = virtualPoint;
                continue;
            }

            if (touch.State is not (TouchLocationState.Released or TouchLocationState.Invalid))
                continue;

            activeTouchId = null;
            core.ReleaseUiAction();
            if (isTouchDragging)
            {
                isTouchDragging = false;
                continue;
            }

            var element = currentFrame.HitTest(virtualPoint);
            if (element is null || element.Action == UiAction.None)
                continue;

            core.HandleUiAction(element.Action, currentFrame.Viewport);
            break;
        }
    }
}
#endif
