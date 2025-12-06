using CCG.Server;
using CCG.Shared;

class ServerPlayer : Player<ServerPlayer,ServerGame> {
    public Connection connection;
    public bool blindStageDone = false;

    public ServerPlayer(Connection connection, int index) : base(index) {
        this.connection = connection;
        this.connection.player = this;
    }

    public void SendMessage(S2CMessage message) {
        connection.SendMessage(message);
    }
}
