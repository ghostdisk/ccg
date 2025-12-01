namespace CCG.CLIClient;

using CCG.Client;
using CCG.Shared;

public class CLIClient : Client {
    private int clientIndex;

    public CLIClient(int index) {
        clientIndex = index;
    }

    private void Log(string message) {
        Console.WriteLine($"[{clientIndex}] {message}");
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

    public override void OnGameStarted() {
        Log("Game started!");
    }

}