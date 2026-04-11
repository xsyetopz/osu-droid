package moe.osudroid.service.account;

import moe.osudroid.service.session.SessionService;
import moe.osudroid.service.session.SessionSnapshot;

public final class SessionBackedAccountService implements AccountService {
    private final SessionService sessionService;

    public SessionBackedAccountService(SessionService sessionService) {
        this.sessionService = sessionService;
    }

    @Override
    public AccountProfile currentProfile() {
        SessionSnapshot session = sessionService.currentSession();
        if (!session.isSignedIn()) {
            return AccountProfile.signedOut();
        }

        if (session.isOnlineAuthenticated()) {
            return new AccountProfile(
                    session.getDisplayName(),
                    session.getStatusMessage(),
                    "Online",
                    session.getRankedScore(),
                    session.getAccuracy(),
                    session.getRank(),
                    session.getPp(),
                    session.getAvatarUrl(),
                    true);
        }

        return new AccountProfile(
                session.getDisplayName(),
                session.getStatusMessage(),
                "Offline",
                0L,
                0f,
                0L,
                0f,
                "",
                false);
    }
}
