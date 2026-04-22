#if ANDROID || IOS
using Microsoft.Xna.Framework.Input.Touch;
using OsuDroid.Game;
using OsuDroid.Game.UI;

namespace OsuDroid.App.MonoGame.Input;

internal sealed class MonoGameTouchRouter(OsuDroidGameCore core)
{
    private const float TouchDragThreshold = 10f;
    private static readonly TimeSpan LongPressDelay = TimeSpan.FromMilliseconds(500);

    private int? activeTouchId;
    private UiPoint touchStart;
    private UiPoint previousTouch;
    private DateTime touchStartedUtc;
    private UiAction pressedAction;
    private bool isTouchDragging;
    private bool longPressFired;

    public bool IsPointerActive => activeTouchId is not null || isTouchDragging;

    public void Route(UiFrameSnapshot currentFrame)
    {
        if (activeTouchId is not null && !isTouchDragging && !longPressFired && DateTime.UtcNow - touchStartedUtc >= LongPressDelay)
        {
            if (core.HandleUiLongPress(pressedAction, currentFrame.Viewport))
            {
                longPressFired = true;
                core.ReleaseUiAction();
            }
        }

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
                pressedAction = pressedElement?.Action ?? UiAction.None;
                touchStartedUtc = DateTime.UtcNow;
                longPressFired = false;
                core.PressUiAction(pressedAction);
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
                    longPressFired = false;
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
            if (isTouchDragging || longPressFired)
            {
                isTouchDragging = false;
                longPressFired = false;
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
