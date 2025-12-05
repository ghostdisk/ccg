namespace CCG.Server;

using CCG.Shared;
using WebSocketSharp.Server;

static class Server {

    public static Matchmaker matchmaker = new Matchmaker();

    public static void Main(string[] args) {
        WebSocketServer wss = new WebSocketServer("ws://localhost:4444");
        wss.AddWebSocketService<Connection>("/ws");

        wss.Start();

        Console.WriteLine("Listening on port 4444");
        Console.WriteLine("Press Esc to exit...");
        for (;;) {
            var ki = Console.ReadKey(true);
            if (ki.Key == ConsoleKey.Escape) {
                break;
            }
        }
        wss.Stop();
    }

    public static void HandleMessage(Connection connection, C2SMessage message) {
        if (connection.player != null && connection.player.game.HandleMessage(connection.player, message)) {
            return;
        }
        switch (message) {
            case C2SJoinMatchmaking join: {
                matchmaker.AddClient(connection);
                connection.SendMessage(new S2CMatchmakingState { state = MatchmakingState.Joined });
                break;
            }
            case C2SLeaveMatchmaking leave: {
                matchmaker.RemoveClient(connection);
                connection.SendMessage(new S2CMatchmakingState { state = MatchmakingState.NotJoined });
                break;
            }
        }
    }

    public static void HandleConnectionClose(Connection connection) {
        if (connection.inMatchmaking) {
            matchmaker.RemoveClient(connection);
        }
    }

}
