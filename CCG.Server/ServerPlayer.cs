using CCG.Server;
using CCG.Shared;

internal class ServerPlayer : Player<ServerPlayer,ServerGame> {
    public Client client;

    public ServerPlayer(Client client) {
        this.client = client;
        this.client.player = this;
    }

    public void SendMessage(S2CMessage message) {
        // ServerPlayer needs to flip playerIndex for the client
        if (message is S2CMulliganResult mulliganResult) {
            mulliganResult.playerIndex = (this == game.player1) ? 0 : 1;
        }
        client.SendMessage(message);
    }

    public void HandleMessage(C2SMessage message) {
        game.HandleMessage(this, message);
    }
}
