namespace CCG.CLIClient;

using CCG.Client;
using CCG.Shared;

public class CLIClient : Client {
    private int clientIndex;

    public CLIClient(int index) {
        clientIndex = index;
    }

    public void Log(string message) {
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
        DisplayHand();
    }

    public void DisplayHand() {
        if (game == null) {
            Log("Game not started yet.");
            return;
        }
        Log("Hand:");
        foreach (Card card in game.player1.hand)
            Log($"    {card.prototype.name} (id:{card.card_id})");

        Log($"Mulligans remaining: {game.player1.mulligansRemaining}");
    }

    public void MulliganCard(int index) {
        if (game == null) {
            Log("Game not started yet.");
            return;
        }
        Send(new C2SMulliganSwap { indexInHand = index });
    }

    public void DoneWithMulligan() {
        if (game == null) {
            Log("Game not started yet.");
            return;
        }
        Send(new C2SDoneWithMulligan());
    }

}