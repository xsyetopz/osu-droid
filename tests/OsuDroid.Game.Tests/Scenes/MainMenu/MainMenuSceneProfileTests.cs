using NUnit.Framework;
using OsuDroid.Game.Scenes.MainMenu;
using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{
    [Test]
    public void MainMenuDoesNotDrawProfileBadgeWhenServerConnectionIsOff()
    {
        var scene = new MainMenuScene();
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "profile-panel"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "profile-player"), Is.False);
    }

    [Test]
    public void MainMenuDrawsOnlinePanelAboveLogoWhenServerConnectionIsOn()
    {
        var scene = new MainMenuScene(onlinePanelState: OnlineProfilePanelState.Connecting);
        var elements = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame.Elements.ToList();

        int logoIndex = elements.FindIndex(element => element.Id == "logo");
        int panelIndex = elements.FindIndex(element => element.Id == "profile-panel");
        int footerIndex = elements.FindIndex(element => element.Id == "profile-avatar-footer");

        Assert.That(panelIndex, Is.GreaterThan(logoIndex));
        Assert.That(footerIndex, Is.GreaterThan(panelIndex));
    }

    [Test]
    public void MainMenuOnlinePanelUsesAndroidOnlinePanelGeometry()
    {
        var scene = new MainMenuScene(onlinePanelState: OnlineProfilePanelState.Connecting);
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        UiElementSnapshot panel = frame.Elements.Single(element => element.Id == "profile-panel");
        UiElementSnapshot avatarFooter = frame.Elements.Single(element =>
            element.Id == "profile-avatar-footer"
        );

        Assert.That(
            panel.Bounds,
            Is.EqualTo(
                new UiRect(
                    MainMenuScene.OnlinePanelX,
                    MainMenuScene.OnlinePanelY,
                    MainMenuScene.OnlinePanelWidth,
                    MainMenuScene.OnlinePanelHeight
                )
            )
        );
        Assert.That(
            avatarFooter.Bounds,
            Is.EqualTo(
                new UiRect(
                    MainMenuScene.OnlinePanelX,
                    MainMenuScene.OnlinePanelY,
                    MainMenuScene.OnlinePanelAvatarFooterSize,
                    MainMenuScene.OnlinePanelAvatarFooterSize
                )
            )
        );
        UiElementSnapshot message = frame.Elements.Single(element =>
            element.Id == "profile-message"
        );

        Assert.That(panel.Color, Is.EqualTo(new UiColor(51, 51, 51, 128)));
        Assert.That(avatarFooter.Color, Is.EqualTo(new UiColor(51, 51, 51, 204)));
        Assert.That(message.Text, Is.EqualTo("Logging in..."));
        Assert.That(message.TextStyle?.Size, Is.EqualTo(35f));
        Assert.That(message.Bounds.Height, Is.EqualTo(44f));
        Assert.That(message.Bounds.X, Is.EqualTo(115f));
        Assert.That(message.Bounds.Y, Is.EqualTo(10f));
        Assert.That(frame.Elements.Any(element => element.Id == "profile-submessage"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "profile-avatar"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "profile-player"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "profile-pp"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "profile-acc"), Is.False);
    }

    [Test]
    public void MainMenuOnlinePanelShowsLoginFailureState()
    {
        var scene = new MainMenuScene(
            onlinePanelState: OnlineProfilePanelState.Failed("Wrong name or password")
        );
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        Assert.That(
            frame.Elements.Single(element => element.Id == "profile-message").Text,
            Is.EqualTo("Cannot log in")
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "profile-message").TextStyle?.Size,
            Is.EqualTo(35f)
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "profile-submessage").Text,
            Is.EqualTo("Wrong name or password")
        );
        UiElementSnapshot submessage = frame.Elements.Single(element =>
            element.Id == "profile-submessage"
        );
        Assert.That(submessage.TextStyle?.Size, Is.EqualTo(21f));
        Assert.That(submessage.Bounds.Height, Is.EqualTo(30f));
    }

    [Test]
    public void MainMenuOnlinePanelShowsRetryState()
    {
        var scene = new MainMenuScene(onlinePanelState: OnlineProfilePanelState.Retrying());
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        Assert.That(
            frame.Elements.Single(element => element.Id == "profile-message").Text,
            Is.EqualTo("Login failed")
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "profile-message").TextStyle?.Size,
            Is.EqualTo(35f)
        );
        Assert.That(
            frame.Elements.Single(element => element.Id == "profile-submessage").Text,
            Is.EqualTo("Retrying in 5 sec")
        );
        UiElementSnapshot submessage = frame.Elements.Single(element =>
            element.Id == "profile-submessage"
        );
        Assert.That(submessage.TextStyle?.Size, Is.EqualTo(21f));
        Assert.That(submessage.Bounds.Height, Is.EqualTo(30f));
    }

    [Test]
    public void MainMenuLoggedInProfileBadgeShowsPerformanceAndAccuracy()
    {
        var scene = new MainMenuScene(
            onlinePanelState: new OnlineProfilePanelState(
                new OnlineProfileSnapshot(
                    "Player",
                    DroidAssets.EmptyAvatar,
                    Rank: 42,
                    PerformancePoints: 12345,
                    Accuracy: 98.76f
                )
            )
        );
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        Assert.That(
            frame.Elements.Single(element => element.Id == "profile-player").Text,
            Is.EqualTo("Player")
        );
        UiElementSnapshot player = frame.Elements.Single(element => element.Id == "profile-player");
        Assert.That(player.TextStyle?.Size, Is.EqualTo(35f));
        Assert.That(player.Bounds.Height, Is.EqualTo(44f));
        UiElementSnapshot rank = frame.Elements.Single(element => element.Id == "profile-rank");
        UiElementSnapshot performance = frame.Elements.Single(element =>
            element.Id == "profile-pp"
        );
        UiElementSnapshot accuracy = frame.Elements.Single(element => element.Id == "profile-acc");

        Assert.That(rank.Text, Is.EqualTo("#42"));
        Assert.That(rank.Bounds.Y, Is.EqualTo(60f));
        Assert.That(rank.Color, Is.EqualTo(new UiColor(153, 153, 153, 230)));
        Assert.That(rank.TextStyle?.Alignment, Is.EqualTo(UiTextAlignment.Right));
        Assert.That(performance.Text, Is.EqualTo("Performance: 12,345pp"));
        Assert.That(performance.TextStyle?.Size, Is.EqualTo(21f));
        Assert.That(performance.Bounds.Height, Is.EqualTo(30f));
        Assert.That(performance.Bounds.X, Is.EqualTo(125f));
        Assert.That(performance.Bounds.Y, Is.EqualTo(55f));
        Assert.That(accuracy.Text, Is.EqualTo("Accuracy: 98.76%"));
        Assert.That(accuracy.TextStyle?.Size, Is.EqualTo(21f));
        Assert.That(accuracy.Bounds.Height, Is.EqualTo(30f));
        Assert.That(accuracy.Bounds.X, Is.EqualTo(125f));
        Assert.That(accuracy.Bounds.Y, Is.EqualTo(80f));
    }

    [Test]
    public void MainMenuProfileBadgeSuppressesAvatarWhenOptionIsOff()
    {
        var scene = new MainMenuScene(
            onlinePanelState: new OnlineProfilePanelState(
                new OnlineProfileSnapshot(
                    "Player",
                    DroidAssets.EmptyAvatar,
                    AvatarPath: "/tmp/avatar.png",
                    PerformancePoints: 12345
                ),
                LoadAvatar: false
            )
        );
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "profile-avatar"), Is.False);
        Assert.That(
            frame.Elements.Single(element => element.Id == "profile-player").Text,
            Is.EqualTo("Player")
        );
        Assert.That(frame.Elements.Any(element => element.Id == "profile-avatar-footer"), Is.True);
    }

    [Test]
    public void OnlineProfilePanelStateKeepsAnnouncementOption()
    {
        OnlineProfilePanelState state = OnlineProfilePanelState.FromOptionalProfile(
            new OnlineProfileSnapshot("Player"),
            receiveAnnouncements: false
        )!;

        Assert.That(state.ReceiveAnnouncements, Is.False);
    }

    [Test]
    public void MainMenuProfileBadgeStaysTopLeftOnWidePhoneViewport()
    {
        var scene = new MainMenuScene(onlinePanelState: OnlineProfilePanelState.Connecting);
        UiFrameSnapshot frame = scene
            .CreateSnapshot(VirtualViewport.FromSurface(2532, 1170))
            .UiFrame;

        UiElementSnapshot panel = frame.Elements.Single(element => element.Id == "profile-panel");

        Assert.That(panel.Bounds.X, Is.EqualTo(MainMenuScene.OnlinePanelX));
        Assert.That(panel.Bounds.Y, Is.EqualTo(MainMenuScene.OnlinePanelY));
        Assert.That(panel.Bounds.Width, Is.EqualTo(MainMenuScene.OnlinePanelWidth));
        Assert.That(panel.Bounds.Height, Is.EqualTo(MainMenuScene.OnlinePanelHeight));
    }
}
