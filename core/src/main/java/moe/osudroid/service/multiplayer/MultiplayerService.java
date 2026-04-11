package moe.osudroid.service.multiplayer;

import java.util.List;

public interface MultiplayerService {
    List<MultiplayerRoomSummary> lobby();

    MultiplayerRoomDetails joinRoom(String roomId);

    MultiplayerRoomDetails createRoom(String displayName);

    MultiplayerRoomDetails currentRoom();
}
