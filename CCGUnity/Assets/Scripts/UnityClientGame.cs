using UnityEngine;
using CCG.Shared;
using CCG.Client;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Assertions.Must;

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

                card.location = CardLocation.Hand(mulliganResult.player, mulliganResult.indexInHand);
                card.view.SetTarget(deckView.GetTransformProps());
                card.view.JumpToTarget();

                G.mulliganView.AddReplacedCard(card.view);

                await Task.Delay(250);
            });
        }
    }

    void PlaceCard(CardView cardView, CardLocation location) {
        CardLocation oldLocation = cardView.card.location;
        CardLocation newLocation = location;

        switch (oldLocation.type) {
            case CardLocationType.Board:
                board[oldLocation.val1, oldLocation.val2].card = null;
                break;
            default:
                break;
        }

        cardView.card.location = newLocation;

        switch (location.type) {
            case CardLocationType.Hand:
                G.myViews.hand.AddCard(cardView);
                break;
            case CardLocationType.Board:
                board[newLocation.val1, newLocation.val2].card = cardView.card;
                cardView.SetTarget(new TransformProps(G.Targets[newLocation].transform));
                break;
            default:
                break;
        }
    }

    IEnumerable<CardLocation> GetAllowedBlindStageFields(CardView cardView, bool allowHand) {
        List<CardLocation> locations = G.boardView.BoardTargets
            .Where(target => CheckCanPlayerPlayCardsOnField(myPlayer, target.location.BoardPosition))
            .Select(target => target.location)
            .ToList();

        if (allowHand)
            locations.Add(G.myViews.hand.target.location);

        return locations;
    }

    protected override void S2CBlindPhaseStartHandler(S2CBlindPhaseStart blindPhaseStart) {
        Animate(async () => {
            HandView handView = G.myViews.hand;

            List<CardView> handCards = G.mulliganView.Deactivate();

            await Task.WhenAll(handCards.Select(async (cardView, indexInhand) => {
                await Task.Delay(indexInhand * 25);
                cardView.card.location = CardLocation.Hand(myPlayer.index, indexInhand);
                handView.AddCard(cardView);
                handView.UpdateCardsPositions();
            }).ToArray());

            await Task.Delay(200);

            G.blindStageView.Activate(PlaceCard, GetAllowedBlindStageFields);

            handView.AllowPlayingFromHand((CardView cardView, bool fromDrag) => {
                handView.RemoveCard(cardView);
                G.blindStageView.BeginPlayingCard(cardView, fromDrag, false);
            });
        });
    }

    protected override void S2CDoneWithMulliganResultHandler(S2CDoneWithMulliganResult doneWithMulliganResult) {
        base.S2CDoneWithMulliganResultHandler(doneWithMulliganResult);
        // Debug.Log($"[Mulligan] p{doneWithMulliganResult.player} done with mulligan.");
    }

    protected override void S2CGameStartedHandler(S2CGameStarted gameStarted) {
        G.Init(this);

        foreach (CardInfo myHandInfo in gameStarted.myHandInfos) {
            RevealCard(myHandInfo);
        }

        for (int cardIndex = 0; cardIndex < gameStarted.myHand.Count; cardIndex++) {
            UnityCard card = (UnityCard)GetCard(gameStarted.myHand[cardIndex]);
            card.location = CardLocation.Hand(myPlayer.index, cardIndex);
            DrawCard(myPlayer, card);
        }

        for (int cardIndex = 0; cardIndex < gameStarted.opponentHand.Count; cardIndex++) {
            UnityCard card = (UnityCard)GetCard(gameStarted.opponentHand[cardIndex]);
            card.location = CardLocation.Hand(opponentPlayer.index, cardIndex);
            DrawCard(opponentPlayer, card);
        }

        Animate(async () => {
            Action doneAction = () => {
                client.Send(new C2SDoneWithMulligan());
                G.mulliganView.SetRemaining(0, 0);
            };

            Action<CardView> swapCardAction = (CardView card) => {
                G.mulliganView.SetRemaining(G.mulliganView.mulligansRemaining - 1, G.mulliganView.confirmedMulligansRemaining);
                client.Send(new C2SMulliganSwap {
                    cardID = card.card.card_id,
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
        ((UnityCard)GetCard(cardInfo.cardId)).view.OnCardInfoChanged();
    }
}

