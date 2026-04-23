namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{

    [Test]
    public void MainMenuDrawsProfileBadgeAboveLogo()
    {
        var scene = new MainMenuScene();
        var elements = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements.ToList();

        int logoIndex = elements.FindIndex(element => element.Id == "logo");
        int panelIndex = elements.FindIndex(element => element.Id == "profile-panel");
        int footerIndex = elements.FindIndex(element => element.Id == "profile-avatar-footer");

        Assert.That(panelIndex, Is.GreaterThan(logoIndex));
        Assert.That(footerIndex, Is.GreaterThan(panelIndex));
    }
    [Test]
    public void MainMenuProfileBadgeUsesAndroidOnlinePanelGeometry()
    {
        var scene = new MainMenuScene();
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        UiElementSnapshot panel = frame.Elements.Single(element => element.Id == "profile-panel");
        UiElementSnapshot avatarFooter = frame.Elements.Single(element => element.Id == "profile-avatar-footer");

        Assert.That(panel.Bounds, Is.EqualTo(new UiRect(MainMenuScene.OnlinePanelX, MainMenuScene.OnlinePanelY, MainMenuScene.OnlinePanelWidth, MainMenuScene.OnlinePanelHeight)));
        Assert.That(avatarFooter.Bounds, Is.EqualTo(new UiRect(MainMenuScene.OnlinePanelX, MainMenuScene.OnlinePanelY, MainMenuScene.OnlinePanelAvatarFooterSize, MainMenuScene.OnlinePanelAvatarFooterSize)));
        Assert.That(frame.Elements.Any(element => element.Id == "profile-name-placeholder"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "profile-status-placeholder"), Is.False);
        UiElementSnapshot avatar = frame.Elements.Single(element => element.Id == "profile-avatar");

        Assert.That(avatar.Bounds, Is.EqualTo(avatarFooter.Bounds));
        Assert.That(avatar.AssetName, Is.EqualTo(DroidAssets.EmptyAvatar));
        Assert.That(frame.Elements.Single(element => element.Id == "profile-player").Text, Is.EqualTo("Guest"));
        Assert.That(frame.Elements.Any(element => element.Id == "profile-pp"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "profile-acc"), Is.False);
    }
    [Test]
    public void MainMenuLoggedInProfileBadgeShowsPerformanceAndAccuracy()
    {
        var scene = new MainMenuScene(profile: new OnlineProfileSnapshot("Player", DroidAssets.EmptyAvatar, PerformancePoints: 12345, Accuracy: 98.76f));
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "profile-player").Text, Is.EqualTo("Player"));
        Assert.That(frame.Elements.Single(element => element.Id == "profile-pp").Text, Does.Contain("12,345pp"));
        Assert.That(frame.Elements.Single(element => element.Id == "profile-acc").Text, Is.EqualTo("98.76%"));
    }
    [Test]
    public void MainMenuProfileBadgeStaysTopLeftOnWidePhoneViewport()
    {
        var scene = new MainMenuScene();
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(2532, 1170)).UiFrame;

        UiElementSnapshot panel = frame.Elements.Single(element => element.Id == "profile-panel");

        Assert.That(panel.Bounds.X, Is.EqualTo(MainMenuScene.OnlinePanelX));
        Assert.That(panel.Bounds.Y, Is.EqualTo(MainMenuScene.OnlinePanelY));
        Assert.That(panel.Bounds.Width, Is.EqualTo(MainMenuScene.OnlinePanelWidth));
        Assert.That(panel.Bounds.Height, Is.EqualTo(MainMenuScene.OnlinePanelHeight));
    }
}
