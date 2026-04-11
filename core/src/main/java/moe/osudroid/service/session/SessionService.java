package moe.osudroid.service.session;

public interface SessionService {
    SessionSnapshot restoreSession();

    SessionSnapshot currentSession();

    SessionSnapshot signIn(String username, String password);

    SessionSnapshot signOut();
}
