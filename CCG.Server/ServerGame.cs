using CCG.Shared;

namespace CCG.Server;

internal class ServerGame : Game<ServerPlayer,ServerGame> {

    public GameState state = GameState.Mulligan;

    public ServerGame(ServerPlayer player1, ServerPlayer player2) : base(player1, player2) {
    }

    public void Start() {
        player1.mulligansRemaining = GameRules.MaxMulliganCount;
        player2.mulligansRemaining = GameRules.MaxMulliganCount;

        for (int i = 0; i < GameRules.InitialHandSize; i++) {
            player1.hand.Add(player1.deck.Draw()!);
            player2.hand.Add(player2.deck.Draw()!);
        }

        SendToAll(new S2CGameStarted { initialHand = player1.hand });
        player2.client.SendMessage(new S2CGameStarted { initialHand = player2.hand });
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

        SendToAll(new S2CMulliganResult {
            playerIndex = player == player1 ? 0 : 1,
            indexInHand = indexInHand,
            newCardId = newCard.card_id
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
}
