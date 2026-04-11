package moe.osudroid.app;

import moe.osudroid.service.account.AccountService;
import moe.osudroid.service.beatmap.BeatmapLibraryService;
import moe.osudroid.service.multiplayer.MultiplayerService;
import moe.osudroid.service.session.SessionService;
import moe.osudroid.service.social.SocialService;

public final class AppServices {
    private final SessionService sessionService;
    private final AccountService accountService;
    private final SocialService socialService;
    private final MultiplayerService multiplayerService;
    private final BeatmapLibraryService beatmapLibraryService;

    public AppServices(
            SessionService sessionService,
            AccountService accountService,
            SocialService socialService,
            MultiplayerService multiplayerService,
            BeatmapLibraryService beatmapLibraryService) {
        this.sessionService = sessionService;
        this.accountService = accountService;
        this.socialService = socialService;
        this.multiplayerService = multiplayerService;
        this.beatmapLibraryService = beatmapLibraryService;
    }

    public SessionService getSessionService() {
        return sessionService;
    }

    public AccountService getAccountService() {
        return accountService;
    }

    public SocialService getSocialService() {
        return socialService;
    }

    public MultiplayerService getMultiplayerService() {
        return multiplayerService;
    }

    public BeatmapLibraryService getBeatmapLibraryService() {
        return beatmapLibraryService;
    }
}
