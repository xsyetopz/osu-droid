package moe.osudroid.ui;

public enum ScreenRoute {
    LOGIN("Login"),
    MAIN_MENU("Main Menu"),
    SONG_SELECT("Song Select"),
    SETTINGS("Settings"),
    MULTIPLAYER_LOBBY("Multiplayer"),
    MULTIPLAYER_ROOM("Room"),
    GAMEPLAY_LOADER("Gameplay Loader");

    private final String label;

    ScreenRoute(String label) {
        this.label = label;
    }

    public String getLabel() {
        return label;
    }
}
