using CCG.Server;
using CCG.Shared;

internal class ServerPlayer : Player<ServerPlayer,ServerGame> {
    public Client client;

    public ServerPlayer(Client client, int index) : base(index) {
        this.client = client;
        this.client.player = this;
    }

    public void SendMessage(S2CMessage message) {
        client.SendMessage(message);
    }
}
