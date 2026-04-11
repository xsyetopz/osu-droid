package moe.osudroid.app;

import java.util.Arrays;
import java.util.Collections;
import java.util.List;

import moe.osudroid.platform.PlatformServices;
import moe.osudroid.service.FriendPresence;
import moe.osudroid.service.account.AccountProfile;
import moe.osudroid.service.account.SessionBackedAccountService;
import moe.osudroid.service.beatmap.BeatmapLibraryService;
import moe.osudroid.service.beatmap.OsuDirectBeatmapLibraryService;
import moe.osudroid.service.multiplayer.MultiplayerRoomDetails;
import moe.osudroid.service.multiplayer.MultiplayerRoomSummary;
import moe.osudroid.service.multiplayer.MultiplayerService;
import moe.osudroid.service.session.OnlineSessionService;
import moe.osudroid.service.session.SessionService;
import moe.osudroid.service.session.SessionSnapshot;
import moe.osudroid.service.social.SocialNotification;
import moe.osudroid.service.social.SocialService;
import moe.osudroid.service.social.SocialSnapshot;

public final class AppServicesFactory {
    private AppServicesFactory() {
    }

    public static AppServices createDefaultServices(PlatformServices platformServices) {
        OnlineSessionService sessionService = new OnlineSessionService();
        sessionService.restoreSession();
        OsuDirectBeatmapLibraryService beatmapLibraryService =
                new OsuDirectBeatmapLibraryService(platformServices.getStorageBackend().writableRoot() + "/Songs", true);
        SessionBackedAccountService accountService = new SessionBackedAccountService(sessionService);
        return new AppServices(
                sessionService,
                accountService,
                new StatusSocialService(sessionService, accountService, beatmapLibraryService),
                new FixtureMultiplayerService(),
                beatmapLibraryService);
    }

    public static AppServices createFixtureServices() {
        return createFixtureServices(true);
    }

    static AppServices createFixtureServices(boolean enableOnlineBeatmapFetch) {
        OnlineSessionService sessionService = new OnlineSessionService();
        return new AppServices(
                sessionService,
                new SessionBackedAccountService(sessionService),
                new FixtureSocialService(),
                new FixtureMultiplayerService(),
                new OsuDirectBeatmapLibraryService("/Songs", enableOnlineBeatmapFetch));
    }

    private static final class FixtureSocialService implements SocialService {
        @Override
        public SocialSnapshot snapshot() {
            List<SocialNotification> notifications = Arrays.asList(
                    new SocialNotification("Build", "Pinned upstream sync required before mirroring lazer visuals."),
                    new SocialNotification("Social", "2 friends are online in the rewrite fixture network."));
            List<FriendPresence> friends = Arrays.asList(
                    new FriendPresence("Aoko", "Browsing song select", true),
                    new FriendPresence("Mika", "Hosting a lobby", true),
                    new FriendPresence("Ren", "Offline", false));
            return new SocialSnapshot(notifications, friends);
        }
    }

    private static final class StatusSocialService implements SocialService {
        private final SessionService sessionService;
        private final SessionBackedAccountService accountService;
        private final BeatmapLibraryService beatmapLibraryService;

        private StatusSocialService(
                SessionService sessionService,
                SessionBackedAccountService accountService,
                BeatmapLibraryService beatmapLibraryService) {
            this.sessionService = sessionService;
            this.accountService = accountService;
            this.beatmapLibraryService = beatmapLibraryService;
        }

        @Override
        public SocialSnapshot snapshot() {
            SessionSnapshot session = sessionService.currentSession();
            AccountProfile profile = accountService.currentProfile();
            List<SocialNotification> notifications = Arrays.asList(
                    new SocialNotification("Session", session.getStatusMessage()),
                    new SocialNotification("Beatmaps", "Songs root: " + beatmapLibraryService.getSongsRoot()),
                    new SocialNotification("Online discovery", beatmapLibraryService.getOnlineStatus()),
                    new SocialNotification(
                            "Profile",
                            profile.isOnline()
                                    ? profile.getDisplayName() + " • #" + profile.getRank() + " • " + profile.getPp() + "pp"
                                    : profile.getStatus()));
            return new SocialSnapshot(notifications, Collections.<FriendPresence>emptyList());
        }
    }

    private static final class FixtureMultiplayerService implements MultiplayerService {
        private MultiplayerRoomDetails currentRoom;

        @Override
        public List<MultiplayerRoomSummary> lobby() {
            return Arrays.asList(
                    new MultiplayerRoomSummary("room-001", "Parity Sprint", 4, 8, true),
                    new MultiplayerRoomSummary("room-002", "Lazer Skin Review", 2, 6, false),
                    new MultiplayerRoomSummary("room-003", "Evening Multi", 7, 8, true));
        }

        @Override
        public MultiplayerRoomDetails joinRoom(String roomId) {
            for (MultiplayerRoomSummary room : lobby()) {
                if (room.getRoomId().equals(roomId)) {
                    currentRoom = new MultiplayerRoomDetails(
                            room.getRoomId(),
                            room.getDisplayName(),
                            room.getPlayerCount(),
                            room.getCapacity(),
                            Arrays.asList("Rewrite Pilot", "Aoko", "Mika", "Ren"),
                            room.isOpen());
                    return currentRoom;
                }
            }
            currentRoom = new MultiplayerRoomDetails(
                    roomId,
                    "Unknown room",
                    1,
                    8,
                    Collections.singletonList("Rewrite Pilot"),
                    false);
            return currentRoom;
        }

        @Override
        public MultiplayerRoomDetails createRoom(String displayName) {
            currentRoom = new MultiplayerRoomDetails(
                    "room-local",
                    displayName,
                    1,
                    8,
                    Collections.singletonList("Rewrite Pilot"),
                    true);
            return currentRoom;
        }

        @Override
        public MultiplayerRoomDetails currentRoom() {
            return currentRoom;
        }
    }

}
