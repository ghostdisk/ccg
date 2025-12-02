namespace CCG.Client;

using CCG.Shared;
using System.Collections.Generic;

public class ClientGame : Game<ClientPlayer, ClientGame> {

    public ClientPlayer myPlayer;
    public ClientPlayer opponentPlayer;
    public Dictionary<int, Card> knownCards = new Dictionary<int, Card>();

    public ClientGame(ClientPlayer myPlayer, ClientPlayer player0, ClientPlayer player1) : base(player0, player1) {
        this.myPlayer = myPlayer;
        this.opponentPlayer = myPlayer.opponent;
    }

    public Card GetCard(int cardId) {
        if (!knownCards.TryGetValue(cardId, out Card? card)) {
            card = new Card(CardDatabase.CardPrototypes[0], cardId); // Null Card
            knownCards.Add(cardId, card);
        }
        return card;
    }

    public void RevealCard(CardInfo cardInfo) {
        Card card = GetCard(cardInfo.cardId);
        card.prototype = CardDatabase.CardPrototypes[cardInfo.cardPrototypeId];
        card.strength = card.prototype.initial_strength;
    }

    public void HandleMessage(S2CMessage message) {
        switch (message) {
            case S2CMulliganResult mulliganResult:
                S2CMulliganResultHandler(mulliganResult);
                break;
            case S2CMulliganDone mulliganDone:
                S2CMulliganDoneHandler(mulliganDone);
                break;
            case S2CDoneWithMulliganResult doneWithMulliganResult:
                S2CDoneWithMulliganResultHandler(doneWithMulliganResult);
                break;
            case S2CCardInfo cardInfoMessage:
                foreach (CardInfo cardInfo in cardInfoMessage.cardInfos) {
                    RevealCard(cardInfo);
                }
                break;
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