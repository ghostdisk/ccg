using CCG.Server;
using CCG.Shared;

internal class ServerPlayer : Player<ServerPlayer,ServerGame> {
    Client client;

    public ServerPlayer(Client client) {
        this.client = client;
    }

    public void SendMessage(S2CMessage message) {
        client.SendMessage(message);
    }

    public void HandleMessage(C2SMessage message) {
        game.HandleMessage(this, message);
    }
}
