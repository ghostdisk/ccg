namespace CCG.Server;

using WebSocketSharp.Server;

class Program {

    public static Matchmaker matchmaker = new Matchmaker();

    public static void Main(string[] args) {
        WebSocketServer wss = new WebSocketServer("ws://localhost:4444");
        wss.AddWebSocketService<ClientWebSocket>("/ws");

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

}
