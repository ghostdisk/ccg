using UnityEngine;
using CCG.Shared;
using CCG.Client;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

class UnityClientGame : ClientGame {
    private GameController GC;
    private UnityClient client;
    private Queue<Func<Task>> animationTimeline = new Queue<Func<Task>>();
    bool areAnimationsRunning = false;

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

    protected override void S2CGameStartedHandler(S2CGameStarted gameStarted) {
        base.S2CGameStartedHandler(gameStarted);
        Animate(async () => {
            List<CardView> hand = GC.myViews.hand.RemoveAllCards();
            await GC.mulliganView.Activate(hand);
        });
    }

    protected override Card CreateCard(CardPrototype proto, int cardId) {
        UnityCard card = new UnityCard(proto, cardId);
        card.view = UnityEngine.Object.Instantiate(GC.cardViewPrefab);
        card.view.card = card;
        return card;
    }

    protected override void DrawCard(ClientPlayer player, Card _card) {
        base.DrawCard(player, _card);

        Animate(async () => {
            PlayerViews views = GC.GetPlayerViews(player);
            UnityCard card = (UnityCard)_card;

            card.view.SetTarget(views.deck.GetTransformProps());
            card.view.JumpToTarget();

            views.hand.AddCard(card.view);
            views.hand.UpdateCardsPositions();

            await Task.Delay(175);
        });
    }

    public void Animate(Func<Task> func) {
        animationTimeline.Enqueue(func);
        if (!areAnimationsRunning) {
            _ = ProcessAnimationQueueAsync();
        }
    }

    async Task ProcessAnimationQueueAsync() {
        areAnimationsRunning = true;
        Func<Task> func;
        while (animationTimeline.TryDequeue(out func)) {
            await func();
        }
        areAnimationsRunning = false;
    }
}

