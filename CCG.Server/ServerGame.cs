using CCG.Shared;

namespace CCG.Server;

internal class ServerGame : Game<ServerPlayer,ServerGame> {

    public ServerGame(ServerPlayer player1, ServerPlayer player2) : base(player1, player2) {
    }

    public void Start() {
        SendToAll(new S2CGameStarted());
    }

    public void HandleMessage(ServerPlayer player, C2SMessage message) {
        switch (message) {
            default:
                break;
        }
    }

    public void SendToAll(S2CMessage message) {
        player1.SendMessage(message);
        player2.SendMessage(message);
    }
}
