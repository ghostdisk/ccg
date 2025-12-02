using UnityEngine;
using CCG.Shared;
using CCG.Client;

public class UnityClientGame : ClientGame {
    UnityClient client;
    GameController GC;

    public UnityClientGame(UnityClient client, ClientPlayer myPlayer, ClientPlayer player0, ClientPlayer player1) : base(myPlayer, player0, player1) {
        this.client = client;
        this.GC = client.GC;
        GC.menuUiRoot.SetActive(false);
    }

    public void MulliganCard(int index) {
        Debug.Log($"UnityClientGame: Mulligan Card at index {index}");
        client.Send(new C2SMulliganSwap { indexInHand = index });
    }

    public void DoneWithMulligan() {
        Debug.Log("UnityClientGame: Done with mulligan");
        client.Send(new C2SDoneWithMulligan());
    }

    protected override void S2CMulliganResultHandler(S2CMulliganResult mulliganResult) {
        base.S2CMulliganResultHandler(mulliganResult);
        Debug.Log($"UnityClientGame: [Mulligan] p{mulliganResult.player} swapped {mulliganResult.indexInHand} -> {mulliganResult.newCardId}. {mulliganResult.mulligansRemaining} muls left.");
    }

    protected override void S2CMulliganDoneHandler(S2CMulliganDone mulliganDone) {
        Debug.Log("UnityClientGame: [Mulligan] Mulligan done.");
    }

    protected override void S2CDoneWithMulliganResultHandler(S2CDoneWithMulliganResult doneWithMulliganResult) {
        Debug.Log($"UnityClientGame: [Mulligan] p{doneWithMulliganResult.player} done with mulligan.");
    }
}

