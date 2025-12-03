using CCG.Shared;
using System.Collections.Generic;
using System.Linq;

namespace CCG.Server;

internal class ServerGame : Game<ServerPlayer,ServerGame> {

    public GameState state = GameState.Mulligan;
    private int nextCardId = 1;
    Random rng;

    public ServerGame(ServerPlayer player0, ServerPlayer player1) : base(player0, player1) {
        rng = new Random(Guid.NewGuid().GetHashCode());
    }

    public void Start() {
        foreach (ServerPlayer player in players) {
            player.mulligansRemaining = GameRules.MaxMulliganCount;
            player.deck = new Deck();

            for (int i = 0; i < 7; i++) {
                player.deck.cards.Add(CreateCard(CardDatabase.CardPrototypes[1]));
                player.deck.cards.Add(CreateCard(CardDatabase.CardPrototypes[2]));
                player.deck.cards.Add(CreateCard(CardDatabase.CardPrototypes[3]));
                player.deck.cards.Add(CreateCard(CardDatabase.CardPrototypes[4]));
            }
            player.deck.Shuffle(rng);

            for (int i = 0; i < GameRules.InitialHandSize; i++) {
                Card card = player.deck.Draw()!;
                player.hand.Add(card);
            }
        }

        foreach (ServerPlayer player in players) {
            S2CGameStarted message = new S2CGameStarted();

            message.myMulligans = player.mulligansRemaining;
            message.opponentMulligans = player.opponent.mulligansRemaining;
            message.myPlayerIndex = player.index;

            foreach (Card card in player.hand) {
                message.myHand.Add(card.card_id);
                message.myHandInfos.Add(new CardInfo { cardId = card.card_id, cardPrototypeId = card.prototype.id });
            }
            foreach (Card card in player.opponent.hand) {
                message.opponentHand.Add(card.card_id);
            }
            player.SendMessage(message);
        }
    }

    public bool HandleMessage(ServerPlayer player, C2SMessage message) {
        switch (message) {
            case C2SMulliganSwap mulliganSwap:
                HandleMulliganSwap(player, mulliganSwap.indexInHand);
                return true;
            case C2SDoneWithMulligan doneWithMulligan:
                HandleDoneWithMulligan(player);
                return true;
            default:
                return false;
        }
    }

    private void HandleMulliganSwap(ServerPlayer player, int indexInHand) {
        if (state != GameState.Mulligan) {
            player.SendMessage(new S2CErrorNotify { message = "Not in mulligan phase." });
            return;
        }
        if (player.mulligansRemaining <= 0) {
            player.SendMessage(new S2CErrorNotify { message = "No mulligans remaining." });
            return;
        }
        if (indexInHand < 0 || indexInHand >= player.hand.Count) {
            player.SendMessage(new S2CErrorNotify { message = "Invalid card index." });
            return;
        }

        Card oldCard = player.hand[indexInHand];
        Card newCard = player.deck.Draw()!;
        player.hand[indexInHand] = newCard;
        player.deck.cards.Insert(0, oldCard); // Put old card back at the bottom of the deck
        player.mulligansRemaining--;


        SendToAll(new S2CMulliganResult {
            player = player.index,
            indexInHand = indexInHand,
            newCardId = newCard.card_id,
            mulligansRemaining = player.mulligansRemaining,
        });

        player.SendMessage(new S2CCardInfo {
            cardInfos = new List<CardInfo> {
                new CardInfo { cardId = newCard.card_id, cardPrototypeId = newCard.prototype.id }
            }
        });

        if (player.mulligansRemaining == 0 && player.opponent.mulligansRemaining == 0) {
            CheckForMulliganEnd();
        }
    }

    private void CheckForMulliganEnd() {
        if (players.All((player) => player.mulligansRemaining == 0)) {
            state = GameState.Playing;
            SendToAll(new S2CMulliganDone());
        }
    }

    private void HandleDoneWithMulligan(ServerPlayer player) {
        if (state != GameState.Mulligan) {
            player.SendMessage(new S2CErrorNotify { message = "Not in mulligan phase." });
            return;
        }
        if (player.mulligansRemaining > 0) {
            player.mulligansRemaining = 0;
            SendToAll(new S2CDoneWithMulliganResult { player = player.index });
            CheckForMulliganEnd();
        }
    }

    public void SendToAll(S2CMessage message) {
        foreach (ServerPlayer player in players) {
            player.SendMessage(message);
        }
    }

    private Card CreateCard(CardPrototype prototype) {
        Card card = new Card(prototype, nextCardId++);
        return card;
    }
}
