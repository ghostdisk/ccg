using CCG.Shared;
using System.Collections.Generic;
using System.Linq;

namespace CCG.Server;

internal class ServerGame : Game<ServerPlayer,ServerGame> {

    public GameState state = GameState.Mulligan;
    private int nextCardId = 1;

    public ServerGame(ServerPlayer player1, ServerPlayer player2) : base(player1, player2) {
    }

    public void Start() {
        ServerPlayer[] players = { player1, player2 };

        foreach (ServerPlayer player in players) {
            List<CardInfo> handCardInfos = new List<CardInfo>();

            player.mulligansRemaining = GameRules.MaxMulliganCount;
            player.deck = new Deck();

            for (int i = 0; i < 10; i++) {
                player.deck.cards.Add(CreateCard(CardDatabase.CardPrototypes[1]));
                player.deck.cards.Add(CreateCard(CardDatabase.CardPrototypes[2]));
                player.deck.cards.Add(CreateCard(CardDatabase.CardPrototypes[3]));
            }
            player.deck.Shuffle();

            for (int i = 0; i < GameRules.InitialHandSize; i++) {
                Card card = player.deck.Draw()!;
                player.hand.Add(card);
                handCardInfos.Add(new CardInfo { cardId = card.card_id, cardPrototypeId = card.prototype.id });
            }

        }

        foreach (ServerPlayer player in players) {
            S2CGameStarted message = new S2CGameStarted();

            message.myMulligans = player.mulligansRemaining;
            message.opponentMulligans = player.opponent.mulligansRemaining;

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

    public void HandleMessage(ServerPlayer player, C2SMessage message) {
        switch (message) {
            case C2SMulliganSwap mulliganSwap:
                HandleMulliganSwap(player, mulliganSwap.indexInHand);
                break;
            case C2SDoneWithMulligan doneWithMulligan:
                HandleDoneWithMulligan(player);
                break;
            default:
                break;
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

        player.SendMessage(new S2CMulliganResult {
            playerIndex = 0,
            indexInHand = indexInHand,
            newCardId = newCard.card_id
        });
        player.opponent.SendMessage(new S2CMulliganResult {
            playerIndex = 1,
            indexInHand = indexInHand,
            newCardId = newCard.card_id,
        });
        player.SendMessage(new S2CCardInfo {
            cardInfos = new List<CardInfo> {
                new CardInfo { cardId = newCard.card_id, cardPrototypeId = newCard.prototype.id }
            }
        });
    }

    private void HandleDoneWithMulligan(ServerPlayer player) {
        if (state != GameState.Mulligan) {
            player.SendMessage(new S2CErrorNotify { message = "Not in mulligan phase." });
            return;
        }
        player.mulligansRemaining = 0; // Player is done with mulligan

        if (player1.mulligansRemaining == 0 && player2.mulligansRemaining == 0) {
            state = GameState.Playing; // Transition to playing phase
            SendToAll(new S2CMulliganDone());
        }
    }

    public void SendToAll(S2CMessage message) {
        player1.SendMessage(message);
        player2.SendMessage(message);
    }

    private Card CreateCard(CardPrototype prototype) {
        Card card = new Card(prototype, nextCardId++);
        return card;
    }
}
