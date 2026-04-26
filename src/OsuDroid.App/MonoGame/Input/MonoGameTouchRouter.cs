#if ANDROID || IOS
using System.Diagnostics;
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
    private bool isUiDragCaptured;
    private bool isSceneScrollCandidate;

    public bool IsPointerActive => activeTouchId is not null || isTouchDragging;

    public void Route(UiFrameSnapshot currentFrame)
    {
        if (
            activeTouchId is not null
            && !isTouchDragging
            && !longPressFired
            && DateTime.UtcNow - touchStartedUtc >= LongPressDelay
        )
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
            double timestampSeconds = TimestampSeconds();
            if (touch.State == TouchLocationState.Pressed)
            {
                activeTouchId = touch.Id;
                touchStart = virtualPoint;
                previousTouch = virtualPoint;
                isTouchDragging = false;
                var pressedElement = currentFrame.HitTest(virtualPoint);
                pressedAction = pressedElement?.Action ?? UiAction.None;
                isUiDragCaptured = false;
                isSceneScrollCandidate = false;
                if (
                    pressedElement is not null
                    && core.TryBeginUiDrag(pressedElement.Id, virtualPoint, currentFrame.Viewport)
                )
                {
                    pressedAction = UiAction.None;
                    isTouchDragging = false;
                    longPressFired = false;
                    isUiDragCaptured = true;
                    continue;
                }

                isSceneScrollCandidate = core.TryBeginSceneScrollDrag(
                    virtualPoint,
                    currentFrame.Viewport,
                    timestampSeconds
                );
                touchStartedUtc = DateTime.UtcNow;
                longPressFired = false;
                core.PressUiAction(pressedAction);
                if (PerfDiagnostics.Enabled)
                    Console.WriteLine($"osu!droid perf phase=input.pressed action={pressedAction}");
                continue;
            }

            if (activeTouchId != touch.Id)
                continue;

            if (touch.State == TouchLocationState.Moved)
            {
                if (isUiDragCaptured)
                {
                    core.UpdateUiDrag(virtualPoint, currentFrame.Viewport);
                    previousTouch = virtualPoint;
                    continue;
                }

                var movedX = virtualPoint.X - touchStart.X;
                var movedY = virtualPoint.Y - touchStart.Y;
                if (
                    !isTouchDragging
                    && MathF.Sqrt(movedX * movedX + movedY * movedY) > TouchDragThreshold
                )
                {
                    isTouchDragging = true;
                    longPressFired = false;
                    core.ReleaseUiAction();
                }

                if (isTouchDragging && isSceneScrollCandidate)
                {
                    core.UpdateSceneScrollDrag(
                        virtualPoint,
                        currentFrame.Viewport,
                        timestampSeconds
                    );
                }
                else if (isTouchDragging)
                {
                    core.ScrollActiveScene(
                        previousTouch.X - virtualPoint.X,
                        previousTouch.Y - virtualPoint.Y,
                        touchStart,
                        currentFrame.Viewport
                    );
                }

                previousTouch = virtualPoint;
                continue;
            }

            if (touch.State is not (TouchLocationState.Released or TouchLocationState.Invalid))
                continue;

            activeTouchId = null;
            core.ReleaseUiAction();
            if (isUiDragCaptured)
            {
                core.EndUiDrag(virtualPoint, currentFrame.Viewport);
                isUiDragCaptured = false;
                isTouchDragging = false;
                longPressFired = false;
                continue;
            }

            if (isTouchDragging || longPressFired)
            {
                if (isSceneScrollCandidate)
                {
                    core.EndSceneScrollDrag(virtualPoint, currentFrame.Viewport, timestampSeconds);
                }

                isSceneScrollCandidate = false;
                isTouchDragging = false;
                longPressFired = false;
                continue;
            }

            if (isSceneScrollCandidate)
            {
                core.EndSceneScrollDrag(virtualPoint, currentFrame.Viewport, timestampSeconds);
                isSceneScrollCandidate = false;
            }

            var element = currentFrame.HitTest(virtualPoint);
            if (element is null || element.Action == UiAction.None)
                continue;

            var start = PerfDiagnostics.Start();
            core.HandleUiAction(element.Action, currentFrame.Viewport);
            PerfDiagnostics.Log(
                "input.releasedAction",
                start,
                $"pressed={pressedAction} released={element.Action}"
            );
            break;
        }
    }

    private static double TimestampSeconds() =>
        Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;
}
#endif
