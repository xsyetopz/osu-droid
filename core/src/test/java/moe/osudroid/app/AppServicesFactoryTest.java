package moe.osudroid.app;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;

import moe.osudroid.service.beatmap.BeatmapCard;
import org.junit.jupiter.api.Test;

final class AppServicesFactoryTest {
    @Test
    void fixtureServicesExposeRewriteOwnedSessionAndBeatmapData() {
        AppServices services = AppServicesFactory.createFixtureServices(false);

        assertFalse(services.getSessionService().currentSession().isSignedIn());
        services.getSessionService().signIn("fixture-user", "fixture-pass");
        assertFalse(services.getSessionService().currentSession().isSignedIn());

        BeatmapCard firstBeatmap = services.getBeatmapLibraryService().searchOnline("").get(0);
        assertNotNull(firstBeatmap.getSource());
        assertEquals("offline", firstBeatmap.getSource());

        assertTrue(services.getAccountService().currentProfile().getDisplayName().length() > 0);
        assertEquals(3, services.getMultiplayerService().lobby().size());
    }
}
