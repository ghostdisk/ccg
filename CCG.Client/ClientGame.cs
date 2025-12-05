namespace CCG.Client;

using CCG.Shared;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class ClientGame : Game<ClientPlayer, ClientGame> {

    public ClientPlayer myPlayer;
    public ClientPlayer opponentPlayer;
    public Dictionary<int, Card> knownCards = new();
    private ConcurrentQueue<Action> gameActions = new();

    public void EnqueueGameAction(Action action) {
        gameActions.Enqueue(action);
    }

    public void ProcessGameActions() {
        Action action;
        while (gameActions.TryDequeue(out action))
            action();
    }

    public ClientGame(ClientPlayer myPlayer, ClientPlayer player0, ClientPlayer player1) : base(player0, player1) {
        this.myPlayer = myPlayer;
        this.opponentPlayer = myPlayer.opponent;
    }

    protected virtual Card CreateCard(CardPrototype proto, int cardId) {
        return new Card(proto, cardId);
    }

    public Card GetCard(int cardId) {
        if (!knownCards.TryGetValue(cardId, out Card? card)) {
            card = CreateCard(CardDatabase.NullPrototype, cardId);
            knownCards.Add(cardId, card);
        }
        return card;
    }

    public virtual void RevealCard(CardInfo cardInfo) {
        Card card = GetCard(cardInfo.cardId);
        card.prototype = CardDatabase.CardPrototypes[cardInfo.cardPrototypeId];
        card.strength = card.prototype.initial_strength;
    }

    public bool HandleMessage(S2CMessage message) {
        switch (message) {
            case S2CMulliganResult mulliganResult:
                S2CMulliganResultHandler(mulliganResult);
                return true;
            case S2CMulliganDone mulliganDone:
                S2CMulliganDoneHandler(mulliganDone);
                return true;
            case S2CDoneWithMulliganResult doneWithMulliganResult:
                S2CDoneWithMulliganResultHandler(doneWithMulliganResult);
                return true;
            case S2CGameStarted gameStarted:
                S2CGameStartedHandler(gameStarted);
                return true;
            case S2CCardInfo cardInfoMessage:
                foreach (CardInfo cardInfo in cardInfoMessage.cardInfos) {
                    RevealCard(cardInfo);
                }
                return true;
            default:
                return false;
        }
    }

    protected virtual void DrawCard(ClientPlayer player, Card card) {
        player.hand.Add(card);
    }

    protected virtual void S2CGameStartedHandler(S2CGameStarted gameStarted) {
        foreach (CardInfo cardInfo in gameStarted.myHandInfos) {
            RevealCard(cardInfo);
        }
        foreach (int cardId in gameStarted.myHand) {
            DrawCard(myPlayer, GetCard(cardId));
        }
        foreach (int cardId in gameStarted.opponentHand) {
            DrawCard(opponentPlayer, GetCard(cardId));
        }
    }

    protected virtual void S2CMulliganResultHandler(S2CMulliganResult mulliganResult) {
        var player = GetPlayer(mulliganResult.player);
        player.hand[mulliganResult.indexInHand] = GetCard(mulliganResult.newCardId);
        player.mulligansRemaining = mulliganResult.mulligansRemaining;
    }

    protected virtual void S2CMulliganDoneHandler(S2CMulliganDone mulliganDone) {
    }

    protected virtual void S2CDoneWithMulliganResultHandler(S2CDoneWithMulliganResult doneWithMulliganResult) {
    }

}

public class ClientPlayer : Player<ClientPlayer, ClientGame> {
    public ClientPlayer(int index) : base(index) {
    }
}