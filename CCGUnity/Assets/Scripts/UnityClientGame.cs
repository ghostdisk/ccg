using UnityEngine;
using CCG.Shared;
using CCG.Client;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

public class UnityClientGame : ClientGame {
    private GameView G;
    private UnityClient client;
    private Queue<Func<Task>> animationTimeline = new Queue<Func<Task>>();
    bool areAnimationsRunning = false;

    public UnityClientGame(UnityClient client, ClientPlayer myPlayer, ClientPlayer player0, ClientPlayer player1) : base(myPlayer, player0, player1) {
        this.client = client;
        G = client.G;
        G.menuUiRoot.SetActive(false);
    }

    protected override void S2CMulliganResultHandler(S2CMulliganResult mulliganResult) {
        base.S2CMulliganResultHandler(mulliganResult);
        // Debug.Log($"[Mulligan] p{mulliganResult.player} swapped {mulliganResult.indexInHand} -> {mulliganResult.newCardId}. {mulliganResult.mulligansRemaining} muls left.");

        ClientPlayer player = GetPlayer(mulliganResult.player);
        if (player == myPlayer) {
            G.mulliganView.SetRemaining(G.mulliganView.mulligansRemaining, mulliganResult.mulligansRemaining);

            Animate(async () => {
                DeckView deckView = G.GetPlayerViews(player).deck;
                UnityCard card = (UnityCard)GetCard(mulliganResult.newCardId);

                card.view.SetTarget(deckView.GetTransformProps());
                card.view.JumpToTarget();

                G.mulliganView.AddReplacedCard(card.view, mulliganResult.indexInHand);

                await Task.Delay(250);
            });
        }
    }

    protected override void S2CBlindPhaseStartHandler(S2CBlindPhaseStart blindPhaseStart) {
        Animate(async () => {
            List<CardView> hand = G.mulliganView.Deactivate();

            await Task.WhenAll(hand.Select(async (card, index) => {
                await Task.Delay(index * 25);

                G.myViews.hand.AddCard(card);
                G.myViews.hand.UpdateCardsPositions();
            }).ToArray());

            await Task.Delay(200);

            foreach (CardView card in hand) {
                card.InteractionMode = CardViewInteractionMode.Click | CardViewInteractionMode.DragFromHand;
            }
        });
    }

    protected override void S2CDoneWithMulliganResultHandler(S2CDoneWithMulliganResult doneWithMulliganResult) {
        base.S2CDoneWithMulliganResultHandler(doneWithMulliganResult);
        // Debug.Log($"[Mulligan] p{doneWithMulliganResult.player} done with mulligan.");
    }

    protected override void S2CGameStartedHandler(S2CGameStarted gameStarted) {
        base.S2CGameStartedHandler(gameStarted);
        Animate(async () => {
            Action doneAction = () => {
                client.Send(new C2SDoneWithMulligan());
                G.mulliganView.SetRemaining(0, 0);
            };

            Action<CardView, int> swapCardAction = (CardView card, int indexInHand) => {
                G.mulliganView.SetRemaining(G.mulliganView.mulligansRemaining - 1, G.mulliganView.confirmedMulligansRemaining);

                client.Send(new C2SMulliganSwap {
                    indexInHand = indexInHand,
                });
            };

            List<CardView> hand = G.myViews.hand.RemoveAllCards();
            await G.mulliganView.Activate(hand, doneAction, swapCardAction, gameStarted.myMulligans);
        });
    }

    protected override Card CreateCard(CardPrototype proto, int cardId) {
        UnityCard card = new UnityCard(proto, cardId);
        card.view = UnityEngine.Object.Instantiate(G.cardViewPrefab);
        card.view.gameObject.SetActive(false);
        card.view.card = card;
        return card;
    }

    protected override void DrawCard(ClientPlayer player, Card _card) {
        base.DrawCard(player, _card);

        Animate(async () => {
            PlayerViews views = G.GetPlayerViews(player);
            UnityCard card = (UnityCard)_card;

            card.view.SetTarget(views.deck.GetTransformProps());
            card.view.JumpToTarget();
            card.view.gameObject.SetActive(true);

            views.hand.AddCard(card.view);
            views.hand.UpdateCardsPositions();

            // await Task.Delay(175);
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

    public override void RevealCard(CardInfo cardInfo) {
        base.RevealCard(cardInfo);
        ((UnityCard)GetCard(cardInfo.cardId)).view.OnCardUpdate();
    }
}

