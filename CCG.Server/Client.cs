using CCG.Shared;
using MessagePack;

namespace CCG.Server;

internal class Client {
    public ServerGame? game;
    public ServerPlayer? player;

    private bool closed;
    private ClientWebSocket ws;

    public Client(ClientWebSocket ws) {
        this.ws = ws;
    }

    public void HandleMessage(C2SMessage message) {
        if (player != null) {
            player.HandleMessage(message);
        }
        switch (message) {
            case C2SJoinMatchmaking join: {
                Program.matchmaker.AddClient(this);
                SendMessage(new S2CMatchmakingState { state = MatchmakingState.Joined });
                break;
            }
            case C2SLeaveMatchmaking leave: {
                Program.matchmaker.RemoveClient(this);
                SendMessage(new S2CMatchmakingState { state = MatchmakingState.NotJoined });
                break;
            }
            case C2SMulliganSwap mulliganSwap:
            case C2SDoneWithMulligan doneWithMulligan:
                if (player != null) {
                    player.HandleMessage(message);
                }
                break;
        }
    }

    public void SendMessage(S2CMessage message) {
        byte[] data = MessagePackSerializer.Serialize(message);
        ws.Send2(data);
    }

    public void OnClose() {
        if (!closed) {
            closed = true;
        }
    }
}
