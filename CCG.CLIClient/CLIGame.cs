namespace CCG.CLIClient;

using CCG.Client;
using CCG.Shared;

public class CLIGame : ClientGame {
    CLIClient client;

    public CLIGame(CLIClient client, ClientPlayer myPlayer, ClientPlayer player0, ClientPlayer player1) : base(myPlayer, player0, player1) {
        this.client = client;

        Log("Game started");
    }

    public void Log(string message) {
        client.Log(message);
    }

    public void Send(C2SMessage message) {
        client.Send(message);
    }

    public void DisplayHand() {
        client.Log("Hand:");
        foreach (Card card in myPlayer.hand)
            Log($"    {card.prototype.name} (id:{card.card_id})");

        Log($"Mulligans remaining: {myPlayer.mulligansRemaining}");
    }

    public void MulliganCard(int index) {
        Send(new C2SMulliganSwap { indexInHand = index });
    }

    public void DoneWithMulligan() {
        Send(new C2SDoneWithMulligan());
    }

    protected override void S2CMulliganResultHandler(S2CMulliganResult mulliganResult) {
        base.S2CMulliganResultHandler(mulliganResult);
        Log($"[Mulligan] p{mulliganResult.player} swapped {mulliganResult.indexInHand} -> {mulliganResult.newCardId}. {mulliganResult.mulligansRemaining} muls left.");
    }

    protected override void S2CMulliganDoneHandler(S2CMulliganDone mulliganDone) {
        Log("[Mulligan] Mulligan done.");
    }

    protected override void S2CDoneWithMulliganResultHandler(S2CDoneWithMulliganResult doneWithMulliganResult) {
        Log($"[Mulligan] p{doneWithMulliganResult.player} done with mulligan.");
    }
}
