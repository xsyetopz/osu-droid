package moe.osudroid.service.social;

public final class SocialNotification {
    private final String title;
    private final String body;

    public SocialNotification(String title, String body) {
        this.title = title;
        this.body = body;
    }

    public String getTitle() {
        return title;
    }

    public String getBody() {
        return body;
    }
}
