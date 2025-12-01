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
        Log("Current Hand:");
        for (int i = 0; i < hand.Count; i++) {
            Log($"  {i}: {hand[i].card_id}");
        }
        Log($"Mulligans remaining: {mulligansRemaining}");
    }

    public void MulliganCard(int index) {
        Send(new C2SMulliganSwap { indexInHand = index });
    }

    public void DoneWithMulligan() {
        Send(new C2SDoneWithMulligan());
    }

}