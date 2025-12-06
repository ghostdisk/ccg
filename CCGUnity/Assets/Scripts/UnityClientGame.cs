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

                card.view.indexInHand = mulliganResult.indexInHand;
                card.view.SetTarget(deckView.GetTransformProps());
                card.view.JumpToTarget();

                G.mulliganView.AddReplacedCard(card.view, mulliganResult.indexInHand);

                await Task.Delay(250);
            });
        }
    }

    void BlindPhase_BeginPlayingCard(CardView card, bool fromDrag, bool allowHand, Action cancelAction) {
        G.myViews.hand.RemoveCard(card);

        List<Target> targets = G.boardView.fieldTargets
            .Cast<Target>()
            .Where(target => CheckCanPlayerPlayCardsOnField(myPlayer, target.position))
            .ToList();

        if (allowHand) {
            targets.Add(G.myViews.hand.target);
        }

        G.playCardView.Activate(card, fromDrag, targets, (CardView cardView, Target target) => {
            Target.DeactivateAll();
            G.myViews.hand.IsInteractive = true;

            if (target) {
                BlindPhase_FinishPlayingCard(cardView, target);
            }
            else {
                cancelAction();
            }
        });
    }

    void BlindPhase_RemoveFromBoard(CardView cardView) {
        UnityCard card = cardView.card;
        if (card.position.column >= 0 && card.position.row >= 0) {
            board[card.position.column, card.position.row].card = null;
        }
        card.position = new Position { column = -1, row = -1 };
    }

    void BlindPhase_AddToBoard(CardView cardView, Position position) {
        board[position.column, position.row].card = cardView.card;
        cardView.card.position = position;

        cardView.SetTarget(new TransformProps(G.boardView.GetTarget(position).transform));

        cardView.ClearCallbacks();
        cardView.IsInteractive = true;

        cardView.OnClickBegin = () => {
            Position returnPosition = cardView.card.position;
            BlindPhase_RemoveFromBoard(cardView);
            Action cancelAction = () => BlindPhase_AddToBoard(cardView, returnPosition);
            BlindPhase_BeginPlayingCard(cardView, true, true, cancelAction);
        };
    }

    void BlindPhase_FinishPlayingCard(CardView cardView, Target target) {
        Card card = cardView.card;
        HandView handView = G.myViews.hand;

        if (target.position.column >= 0 && target.position.row >= 0) {
            BlindPhase_AddToBoard(cardView, target.position);
        }
        else {
            handView.AddCard(cardView);
        }
    }

    protected override void S2CBlindPhaseStartHandler(S2CBlindPhaseStart blindPhaseStart) {
        Animate(async () => {
            HandView handView = G.myViews.hand;

            List<CardView> handCards = G.mulliganView.Deactivate();

            await Task.WhenAll(handCards.Select(async (card, indexInhand) => {
                await Task.Delay(indexInhand * 25);
                card.indexInHand = indexInhand;
                handView.AddCard(card);
                handView.UpdateCardsPositions();
            }).ToArray());

            await Task.Delay(200);

            handView.AllowPlayingFromHand((CardView cardView, bool fromDrag) => {
                Action cancelAction = () => handView.AddCard(cardView);
                BlindPhase_BeginPlayingCard(cardView, fromDrag, false, cancelAction);
            });
        });
    }

    protected override void S2CDoneWithMulliganResultHandler(S2CDoneWithMulliganResult doneWithMulliganResult) {
        base.S2CDoneWithMulliganResultHandler(doneWithMulliganResult);
        // Debug.Log($"[Mulligan] p{doneWithMulliganResult.player} done with mulligan.");
    }

    protected override void S2CGameStartedHandler(S2CGameStarted gameStarted) {
        foreach (CardInfo myHandInfo in gameStarted.myHandInfos) {
            RevealCard(myHandInfo);
        }

        for (int cardIndex = 0; cardIndex < gameStarted.myHand.Count; cardIndex++) {
            UnityCard card = (UnityCard)GetCard(gameStarted.myHand[cardIndex]);
            card.view.indexInHand = cardIndex;
            DrawCard(myPlayer, card);
        }

        for (int cardIndex = 0; cardIndex < gameStarted.opponentHand.Count; cardIndex++) {
            UnityCard card = (UnityCard)GetCard(gameStarted.opponentHand[cardIndex]);
            card.view.indexInHand = cardIndex;
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

