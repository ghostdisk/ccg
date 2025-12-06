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

    public UnityClientGame(UnityClient client, ClientPlayer myPlayer, ClientPlayer player0, ClientPlayer player1) : base(myPlayer, player0, player1) {
        this.client = client;
        G = client.G;
        G.menuUiRoot.SetActive(false);
    }

    protected override void S2CMulliganResultHandler(S2CMulliganResult mulliganResult) {
        ClientPlayer player = GetPlayer(mulliganResult.player);
        int indexInHand = player.hand.FindIndex(card => card.card_id == mulliganResult.oldCardId);
        player.hand[indexInHand] = GetCard(mulliganResult.newCardId);
        player.mulligansRemaining = mulliganResult.mulligansRemaining;

        if (player == myPlayer) {
            G.mulliganView.SetRemaining(G.mulliganView.mulligansRemaining, mulliganResult.mulligansRemaining);

            G.Animate(async () => {
                DeckView deckView = G.GetPlayerViews(player).deck;
                UnityCard card = (UnityCard)GetCard(mulliganResult.newCardId);

                card.location = CardLocation.Hand(mulliganResult.player, indexInHand);
                card.view.SetTarget(deckView.GetTransformProps(), CardViewTweenMode.ExponentialDecay);
                card.view.JumpToTarget();

                G.mulliganView.AddReplacedCard(card.view);

                await Task.Delay(250);
            });
        }
        else {
            PlayerViews views = G.GetPlayerViews(player);

            UnityCard oldCard = (UnityCard)GetCard(mulliganResult.oldCardId);
            UnityCard newCard = (UnityCard)GetCard(mulliganResult.newCardId);
            oldCard.view.gameObject.SetActive(false);

            newCard.location = CardLocation.Hand(mulliganResult.player, indexInHand);
            views.hand.RemoveCard(oldCard.view);
            views.hand.AddCard(newCard.view);
            newCard.view.JumpToTarget();
        }
    }

    void PlaceCard(CardView cardView, CardLocation location) {
        CardLocation oldLocation = cardView.card.location;
        CardLocation newLocation = location;

        switch (oldLocation.type) {
            case CardLocationType.Board:
                board[oldLocation.val1, oldLocation.val2].card = null;
                break;
            case CardLocationType.Hand:
                G.GetPlayerViews(GetPlayer(oldLocation.val1)).hand.RemoveCard(cardView);
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
                cardView.SetTarget(new TransformProps(G.Targets[newLocation].transform), CardViewTweenMode.ExponentialDecay);

                if (!cardView.gameObject.activeSelf) {
                    Debug.LogWarning("Card placed on board was inactive. This is likely a desync.");
                    cardView.gameObject.SetActive(true);
                }
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

    void OnBlindStageDoneButtonPressed() {
        G.blindStageView.MakeFinal();
        G.myViews.hand.IsInteractive = false;
        G.myViews.hand.RemoveGaps();

        List<Card> cards = board.Cast<Field>()
            .Where(field => field.card != null)
            .Select(field => field.card)
            .ToList();

        foreach (Card card in cards) {
            ((UnityCard)card).view.ClearCallbacks();
        }

        client.Send(new C2S_BlindStage_Done {
            cardsPlayed = cards.Select(card => new C2S_BlindStage_PlayCard {
                cardID = card.card_id,
                position = card.location.BoardPosition,
            }).ToArray(),
        });
    }

    protected override void S2CBlindPhaseStartHandler(S2CBlindPhaseStart blindPhaseStart) {
        G.Animate(async () => {
            HandView handView = G.myViews.hand;

            List<CardView> handCards = G.mulliganView.Deactivate();

            await Task.WhenAll(handCards.Select(async (cardView, indexInhand) => {
                await Task.Delay(indexInhand * 25);
                cardView.card.location = CardLocation.Hand(myPlayer.index, indexInhand);
                handView.AddCard(cardView);
                handView.UpdateCardsPositions();
            }).ToArray());

            await Task.Delay(200);

            Action<CardView, CardLocation> placeCard = (CardView cardView, CardLocation location) => {
                PlaceCard(cardView, location);

                int unitsCount = board.Cast<Field>().Where(field => field.card != null).Count();
                int remaining = GameRules.BlindStageCards - unitsCount;
                G.blindStageView.UpdateUI(remaining);

                G.myViews.hand.IsInteractive = remaining > 0;
            };

            G.blindStageView.Activate(placeCard, GetAllowedBlindStageFields, OnBlindStageDoneButtonPressed, GameRules.BlindStageCards);

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
            UnityCard myCard = (UnityCard)GetCard(gameStarted.myHand[cardIndex]);
            myCard.location = CardLocation.Hand(myPlayer.index, cardIndex);

            UnityCard opponentCard = (UnityCard)GetCard(gameStarted.opponentHand[cardIndex]);
            opponentCard.location = CardLocation.Hand(opponentPlayer.index, cardIndex);

            myPlayer.hand.Add(myCard);
            opponentPlayer.hand.Add(opponentCard);

            G.Animate(async () => {
                AnimateDrawCard(myPlayer, myCard);
                myCard.view.Flip(false, 0.30f);
                AnimateDrawCard(opponentPlayer, opponentCard);
                await Task.Delay(150);
            });
        }

        G.Animate(async () => {
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

    protected override void S2CMainPhaseStartHandler(S2CMainPhaseStart mainPhaseStart) {
        G.blindStageView.Deactivate();

        foreach (var play in mainPhaseStart.plays) {
            UnityCard card = (UnityCard)RevealCard(play.cardInfo);
            card.view.Flip(false, 0.30f);
            PlaceCard(card.view, CardLocation.Board(play.position));
        }

        G.myViews.hand.RemoveGaps();
        G.opponentViews.hand.RemoveGaps();
    }

    protected override Card CreateCard(CardPrototype proto, int cardId) {
        UnityCard card = new UnityCard(proto, cardId);
        card.view = UnityEngine.Object.Instantiate(G.cardViewPrefab);
        card.view.gameObject.SetActive(false);
        card.view.card = card;
        return card;
    }

    void AnimateDrawCard(ClientPlayer player, Card _card) {
        PlayerViews views = G.GetPlayerViews(player);
        UnityCard card = (UnityCard)_card;

        card.view.SetTarget(views.deck.GetTransformProps(), CardViewTweenMode.ExponentialDecay);
        card.view.JumpToTarget();
        card.view.gameObject.SetActive(true);

        views.hand.AddCard(card.view);
        views.hand.UpdateCardsPositions();
    }

    public override Card RevealCard(CardInfo cardInfo) {
        UnityCard card = (UnityCard)base.RevealCard(cardInfo);
        card.view.UpdateInfo();
        return card;
}
}

