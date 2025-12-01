namespace CCG.Client;

using CCG.Shared;
using System.Collections.Generic;

public class ClientGame : Game<ClientPlayer, ClientGame> {
    public Dictionary<int, Card> knownCards = new Dictionary<int, Card>();
    public List<Card> hand = new List<Card>();
    public int mulligansRemaining = 0;

    public ClientGame(ClientPlayer player1, ClientPlayer player2) : base(player1, player2) {
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
}

public class ClientPlayer : Player<ClientPlayer, ClientGame> {
    public ClientPlayer() {
    }
}