package moe.osudroid.ui;

import java.util.List;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.ScreenAdapter;
import com.badlogic.gdx.assets.AssetManager;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.scenes.scene2d.Stage;
import com.badlogic.gdx.scenes.scene2d.ui.CheckBox;
import com.badlogic.gdx.scenes.scene2d.ui.Label;
import com.badlogic.gdx.scenes.scene2d.ui.ScrollPane;
import com.badlogic.gdx.scenes.scene2d.ui.Skin;
import com.badlogic.gdx.scenes.scene2d.ui.Table;
import com.badlogic.gdx.scenes.scene2d.ui.TextField;
import com.badlogic.gdx.scenes.scene2d.ui.TextButton;
import com.badlogic.gdx.scenes.scene2d.utils.ClickListener;
import com.badlogic.gdx.utils.ScreenUtils;
import com.badlogic.gdx.utils.viewport.ScreenViewport;

import moe.osudroid.app.AppServices;
import moe.osudroid.assets.ThemeResolver;
import moe.osudroid.assets.ui.UiAssetKey;
import moe.osudroid.assets.ui.UiResourceCatalog;
import moe.osudroid.service.FriendPresence;
import moe.osudroid.service.account.AccountProfile;
import moe.osudroid.service.beatmap.BeatmapCard;
import moe.osudroid.service.multiplayer.MultiplayerRoomDetails;
import moe.osudroid.service.multiplayer.MultiplayerRoomSummary;
import moe.osudroid.service.session.SessionSnapshot;
import moe.osudroid.service.social.SocialNotification;

public final class AppShellScreen extends ScreenAdapter {
    private final AssetManager assetManager;
    private final AppServices appServices;
    private final UiResourceCatalog uiResources;

    private Stage stage;
    private Skin skin;
    private Texture chromePixel;
    private ScreenRoute route;
    private Table contentTable;
    private Table overlayTable;
    private String loginUsername = "";
    private String loginPassword = "";
    private String onlineBeatmapQuery = "";

    public AppShellScreen(AssetManager assetManager, AppServices appServices, ThemeResolver themeResolver) {
        this.assetManager = assetManager;
        this.appServices = appServices;
        this.uiResources = themeResolver.resolve();
        this.route = parseRoute(uiResources.getManifest().getDefaultRoute());
    }

    @Override
    public void show() {
        chromePixel = assetManager.get(uiResources.pathFor(UiAssetKey.CHROME_PIXEL), Texture.class);
        skin = UiSkinFactory.create(uiResources, chromePixel);
        stage = new Stage(new ScreenViewport());
        Gdx.input.setInputProcessor(stage);
        rebuildStage();
    }

    @Override
    public void render(float delta) {
        ScreenUtils.clear(parseColor(uiResources.getManifest().getPalette().getBackground()));
        stage.act(delta);
        stage.draw();
    }

    @Override
    public void resize(int width, int height) {
        if (stage != null) {
            stage.getViewport().update(width, height, true);
        }
    }

    @Override
    public void dispose() {
        if (stage != null) {
            stage.dispose();
        }
        if (skin != null) {
            skin.dispose();
        }
    }

    private void rebuildStage() {
        stage.clear();

        Table root = new Table();
        root.setFillParent(true);
        root.pad(18f);
        root.defaults().pad(8f);

        root.add(buildHeader()).growX().colspan(3).row();
        root.add(buildNavigation()).width(240f).growY();

        contentTable = new Table();
        contentTable.top().left().defaults().pad(8f);
        contentTable.setBackground(skin.getDrawable("panel"));
        root.add(new ScrollPane(contentTable, skin)).grow();

        overlayTable = new Table();
        overlayTable.top().defaults().pad(6f).left();
        overlayTable.setBackground(skin.getDrawable("panel"));
        root.add(new ScrollPane(overlayTable, skin)).width(300f).growY();

        stage.addActor(root);
        rebuildContent();
        rebuildOverlay();
    }

    private Table buildHeader() {
        Table header = new Table();
        header.defaults().left().pad(4f);
        Label title = new Label(uiResources.getManifest().getBrandName(), skin, "title");
        Label subtitle = new Label(uiResources.getManifest().getBrandSubtitle(), skin);
        SessionSnapshot session = appServices.getSessionService().currentSession();
        Label status = new Label(
                session.isSignedIn()
                        ? session.getDisplayName() + " • " + session.getStatusMessage()
                        : session.getStatusMessage(),
                skin);
        header.add(title).left().expandX();
        header.add(status).right().row();
        header.add(subtitle).colspan(2).left();
        return header;
    }

    private Table buildNavigation() {
        Table nav = new Table();
        nav.top();
        nav.defaults().growX().pad(6f);
        nav.setBackground(skin.getDrawable("panel"));

        for (final ScreenRoute nextRoute : ScreenRoute.values()) {
            if (nextRoute == ScreenRoute.MULTIPLAYER_ROOM) {
                continue;
            }
            TextButton button = new TextButton(nextRoute.getLabel(), skin);
            button.setChecked(route == nextRoute);
            button.addListener(new ClickListener() {
                @Override
                public void clicked(com.badlogic.gdx.scenes.scene2d.InputEvent event, float x, float y) {
                    route = nextRoute;
                    rebuildStage();
                }
            });
            nav.add(button).row();
        }
        return nav;
    }

    private void rebuildContent() {
        contentTable.clearChildren();
        switch (route) {
            case LOGIN:
                buildLogin();
                break;
            case MAIN_MENU:
                buildMainMenu();
                break;
            case SONG_SELECT:
                buildSongSelect();
                break;
            case SETTINGS:
                buildSettings();
                break;
            case MULTIPLAYER_LOBBY:
                buildMultiplayerLobby();
                break;
            case MULTIPLAYER_ROOM:
                buildMultiplayerRoom();
                break;
            case GAMEPLAY_LOADER:
                buildGameplayLoader();
                break;
            default:
                buildMainMenu();
                break;
        }
    }

    private void rebuildOverlay() {
        overlayTable.clearChildren();
        overlayTable.add(new Label("Shell Status", skin, "title")).left().row();

        for (SocialNotification notification : appServices.getSocialService().snapshot().getNotifications()) {
            overlayTable.add(block(notification.getTitle(), notification.getBody())).growX().row();
        }

        List<FriendPresence> friends = appServices.getSocialService().snapshot().getFriends();
        if (friends.isEmpty()) {
            overlayTable.add(block(
                    "Presence",
                    "The legacy rewrite lane has real session and beatmap discovery now. Non-multiplayer social presence is not wired yet."))
                    .growX()
                    .row();
            return;
        }

        overlayTable.add(new Label("Friends", skin, "title")).left().padTop(12f).row();
        for (FriendPresence friend : friends) {
            String line = friend.getUsername() + " — " + friend.getActivity();
            overlayTable.add(new Label(line, skin)).left().row();
        }
    }

    private void buildLogin() {
        AccountProfile profile = appServices.getAccountService().currentProfile();
        SessionSnapshot session = appServices.getSessionService().currentSession();
        if (loginUsername.isEmpty() && !session.getUsername().isEmpty()) {
            loginUsername = session.getUsername();
        }

        contentTable.add(new Label("Account Shell", skin, "title")).left().row();
        contentTable.add(block(
                "Session",
                profile.getStatus() + (profile.isOnline()
                        ? " • #" + profile.getRank() + " • " + profile.getPp() + "pp • " + profile.getAccuracy() + "%"
                        : "")))
                .growX()
                .row();

        final TextField usernameField = new TextField(loginUsername, skin);
        usernameField.setMessageText("osudroid.moe username");
        final TextField passwordField = new TextField(loginPassword, skin);
        passwordField.setPasswordMode(true);
        passwordField.setPasswordCharacter('*');
        passwordField.setMessageText("password");
        contentTable.add(new Label("Username", skin)).left().row();
        contentTable.add(usernameField).width(360f).left().row();
        contentTable.add(new Label("Password", skin)).left().row();
        contentTable.add(passwordField).width(360f).left().row();

        TextButton authButton = new TextButton(session.isSignedIn() ? "Sign Out" : "Sign In", skin, "accent");
        authButton.addListener(new ClickListener() {
            @Override
            public void clicked(com.badlogic.gdx.scenes.scene2d.InputEvent event, float x, float y) {
                if (session.isSignedIn()) {
                    appServices.getSessionService().signOut();
                    route = ScreenRoute.LOGIN;
                } else {
                    loginUsername = usernameField.getText();
                    loginPassword = passwordField.getText();
                    SessionSnapshot updated = appServices.getSessionService().signIn(loginUsername, loginPassword);
                    route = updated.isSignedIn() ? ScreenRoute.MAIN_MENU : ScreenRoute.LOGIN;
                }
                rebuildStage();
            }
        });
        contentTable.add(authButton).left().row();
        contentTable.add(block(
                "Legacy target",
                "Login mirrors the historical osudroid.moe flow: login.php, legacy password transform, and session fields uid/ssid/rank/score/pp/accuracy/avatar."))
                .growX().row();
        contentTable
                .add(block(
                        "Cross-platform",
                        "The same shared core login shell runs on desktop, Android, and iOS. Environment restore also reads OSUDROID_ONLINE_USERNAME and OSUDROID_ONLINE_PASSWORD."))
                .growX()
                .row();
    }

    private void buildMainMenu() {
        AccountProfile profile = appServices.getAccountService().currentProfile();
        contentTable.add(new Label("Main Menu", skin, "title")).left().row();
        contentTable.add(block(
                "Shared pre-game shell",
                "This route owns navigation into song select, settings, multiplayer, and the gameplay loader boundary."))
                .growX()
                .row();
        contentTable.add(block(
                "Profile",
                profile.isOnline()
                        ? profile.getDisplayName() + " • #" + profile.getRank() + " • " + profile.getPp() + "pp"
                        : profile.getStatus()))
                .growX()
                .row();

        contentTable.add(actionButton("Go to song select", ScreenRoute.SONG_SELECT, "accent")).left().row();
        contentTable.add(actionButton("Open settings", ScreenRoute.SETTINGS, "default")).left().row();
        contentTable.add(actionButton("Browse multiplayer", ScreenRoute.MULTIPLAYER_LOBBY, "default")).left().row();
        contentTable.add(actionButton("Enter gameplay loader", ScreenRoute.GAMEPLAY_LOADER, "default")).left().row();
    }

    private void buildSongSelect() {
        contentTable.add(new Label("Song Select", skin, "title")).left().row();
        contentTable.add(block(
                "Local library",
                "Songs root: " + appServices.getBeatmapLibraryService().getSongsRoot()))
                .growX()
                .row();

        List<BeatmapCard> installedBeatmaps = appServices.getBeatmapLibraryService().installedBeatmaps();
        contentTable.add(new Label("Installed Beatmaps", skin, "title")).left().row();
        if (installedBeatmaps.isEmpty()) {
            contentTable.add(block(
                    "No local beatmaps found",
                    "Point OSUDROID_SONGS_PATH at your Songs directory or place beatmaps under the platform storage root."))
                    .growX()
                    .row();
        } else {
            for (BeatmapCard beatmap : installedBeatmaps) {
                contentTable.add(block(beatmap.getTitle(), beatmapSummary(beatmap))).growX().row();
            }
        }

        contentTable.add(new Label("Discover Online", skin, "title")).left().padTop(12f).row();
        final TextField queryField = new TextField(onlineBeatmapQuery, skin);
        queryField.setMessageText("Search osu.direct");
        contentTable.add(queryField).width(360f).left().row();
        TextButton searchButton = new TextButton("Search Online", skin, "accent");
        searchButton.addListener(new ClickListener() {
            @Override
            public void clicked(com.badlogic.gdx.scenes.scene2d.InputEvent event, float x, float y) {
                onlineBeatmapQuery = queryField.getText();
                rebuildStage();
            }
        });
        contentTable.add(searchButton).left().row();
        List<BeatmapCard> onlineBeatmaps = appServices.getBeatmapLibraryService().searchOnline(onlineBeatmapQuery);
        contentTable.add(block("Online status", appServices.getBeatmapLibraryService().getOnlineStatus())).growX().row();
        for (BeatmapCard beatmap : onlineBeatmaps) {
            contentTable.add(block(beatmap.getTitle(), beatmapSummary(beatmap))).growX().row();
        }

        contentTable.add(actionButton("Queue current selection", ScreenRoute.GAMEPLAY_LOADER, "accent")).left().row();
    }

    private void buildSettings() {
        contentTable.add(new Label("Settings", skin, "title")).left().row();
        contentTable
                .add(new Label(
                        "Non-gameplay interactables live in scene2d. The actual setting storage contract comes later.",
                        skin))
                .left()
                .row();

        CheckBox skinParity = new CheckBox("Prefer upstream-based theme bundle", skin);
        skinParity.setChecked(true);
        CheckBox notifications = new CheckBox("Show social overlay globally", skin);
        notifications.setChecked(true);
        CheckBox multiplayer = new CheckBox("Enable multiplayer shell in navigation", skin);
        multiplayer.setChecked(true);
        contentTable.add(skinParity).left().row();
        contentTable.add(notifications).left().row();
        contentTable.add(multiplayer).left().row();
    }

    private void buildMultiplayerLobby() {
        contentTable.add(new Label("Multiplayer Lobby", skin, "title")).left().row();
        contentTable.add(block(
                "Fixture lane",
                "Multiplayer is intentionally still fixture-backed in this milestone while session/account and song select move to real services."))
                .growX()
                .row();
        for (final MultiplayerRoomSummary room : appServices.getMultiplayerService().lobby()) {
            Table block = block(
                    room.getDisplayName(),
                    room.getPlayerCount() + "/" + room.getCapacity() + " players • "
                            + (room.isOpen() ? "Open" : "Locked"));
            TextButton join = new TextButton("Join", skin, "accent");
            join.addListener(new ClickListener() {
                @Override
                public void clicked(com.badlogic.gdx.scenes.scene2d.InputEvent event, float x, float y) {
                    appServices.getMultiplayerService().joinRoom(room.getRoomId());
                    route = ScreenRoute.MULTIPLAYER_ROOM;
                    rebuildStage();
                }
            });
            block.add(join).right().padTop(8f);
            contentTable.add(block).growX().row();
        }

        TextButton create = new TextButton("Create local room", skin);
        create.addListener(new ClickListener() {
            @Override
            public void clicked(com.badlogic.gdx.scenes.scene2d.InputEvent event, float x, float y) {
                appServices.getMultiplayerService().createRoom("Rewrite Test Room");
                route = ScreenRoute.MULTIPLAYER_ROOM;
                rebuildStage();
            }
        });
        contentTable.add(create).left().row();
    }

    private void buildMultiplayerRoom() {
        MultiplayerRoomDetails room = appServices.getMultiplayerService().currentRoom();
        if (room == null) {
            route = ScreenRoute.MULTIPLAYER_LOBBY;
            buildMultiplayerLobby();
            return;
        }

        contentTable.add(new Label(room.getDisplayName(), skin, "title")).left().row();
        contentTable.add(new Label(room.getPlayerCount() + "/" + room.getCapacity() + " players", skin)).left().row();
        for (String player : room.getPlayers()) {
            contentTable.add(new Label(player, skin)).left().row();
        }
        contentTable.add(actionButton("Back to lobby", ScreenRoute.MULTIPLAYER_LOBBY, "default")).left().row();
        contentTable.add(actionButton("Queue multiplayer gameplay loader", ScreenRoute.GAMEPLAY_LOADER, "accent"))
                .left().row();
    }

    private void buildGameplayLoader() {
        contentTable.add(new Label("Gameplay Loader Boundary", skin, "title")).left().row();
        contentTable.add(block(
                "Pre-game parity complete",
                "This route is the handoff boundary into beatmap loading and gameplay simulation, which remains outside this implementation lane."))
                .growX()
                .row();
        contentTable.add(actionButton("Return to main menu", ScreenRoute.MAIN_MENU, "default")).left().row();
    }

    private TextButton actionButton(String label, final ScreenRoute nextRoute, String styleName) {
        TextButton button = new TextButton(label, skin, styleName);
        button.addListener(new ClickListener() {
            @Override
            public void clicked(com.badlogic.gdx.scenes.scene2d.InputEvent event, float x, float y) {
                route = nextRoute;
                rebuildStage();
            }
        });
        return button;
    }

    private Table block(String title, String body) {
        Table table = new Table();
        table.left().top().defaults().left().pad(4f);
        table.setBackground(skin.getDrawable("panel-alt"));
        table.add(new Label(title, skin)).left().row();
        Label bodyLabel = new Label(body, skin);
        bodyLabel.setWrap(true);
        table.add(bodyLabel).width(420f).left();
        return table;
    }

    private static String beatmapSummary(BeatmapCard beatmap) {
        StringBuilder builder = new StringBuilder();
        builder.append(beatmap.getArtist())
                .append(" • mapped by ")
                .append(beatmap.getMapper());
        if (!beatmap.getDifficultyName().isEmpty()) {
            builder.append(" • ").append(beatmap.getDifficultyName());
        }
        builder.append(" • ")
                .append(beatmap.getStarRating())
                .append("★ • ")
                .append(beatmap.getBpm())
                .append(" BPM • ")
                .append(beatmap.getStatus())
                .append(" • ")
                .append(beatmap.getSource());
        if (!beatmap.getLocalPath().isEmpty()) {
            builder.append(" • ").append(beatmap.getLocalPath());
        }
        return builder.toString();
    }

    private static ScreenRoute parseRoute(String routeName) {
        if (routeName == null || routeName.isEmpty()) {
            return ScreenRoute.LOGIN;
        }
        try {
            return ScreenRoute.valueOf(routeName);
        } catch (IllegalArgumentException e) {
            return ScreenRoute.LOGIN;
        }
    }

    private static Color parseColor(String hex) {
        return Color.valueOf(hex.replace("#", ""));
    }
}
