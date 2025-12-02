namespace CCG.CLIClient;

using CCG.Client;
using CCG.Shared;

public class CLIClient : Client<CLIGame> {
    private int clientIndex;
    public bool quiet = false;

    public CLIClient(int index) {
        clientIndex = index;
    }

    public void Log(string message, bool force = false) {
        if (!quiet || force) {
            Console.WriteLine($"[{clientIndex}] {message}");
        }
    }

    protected override void OnConnected() {
        Log("Connected to server");
    }

    protected override void OnError(string error) {
        Log($"Error: {error}");
    }

    protected override void OnLostConnection(string reason) {
        Log($"Lost connection: {reason}");
    }

    protected override void OnMatchmakingStateChanged(MatchmakingState state) {
        Log($"Matchmaking state: {state}");
    }

    protected override CLIGame CreateGame(ClientPlayer myPlayer, ClientPlayer player0, ClientPlayer player1) {
        return new CLIGame(this, myPlayer, player0, player1);
    }

}