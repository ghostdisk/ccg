using CCG.Shared;

namespace CCG.Server;

class ServerGame : Game<ServerPlayer,ServerGame> {

    public GamePhase state = GamePhase.Mulligan;
    int nextCardId = 1;
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
            message.myDeckSize = GameRules.DeckSize;
            message.opponentDeckSize = GameRules.DeckSize; // TODO: cmon

            foreach (Card card in player.hand) {
                message.myHand.Add(card.card_id);
                message.myHandInfos.Add(GetCardInfo(card));
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
                HandleMulliganSwap(player, mulliganSwap.cardID);
                return true;
            case C2SDoneWithMulligan doneWithMulligan:
                HandleDoneWithMulligan(player);
                return true;
            case C2S_BlindStage_Done blindStageDone:
                HandleBlindStageDone(player, blindStageDone);
                return true;
            default:
                return false;
        }
    }

    void HandleMulliganSwap(ServerPlayer player, int cardID) {
        if (state != GamePhase.Mulligan) throw new InvalidMessageException("Not in mulligan");
        if (player.mulligansRemaining <= 0) throw new InvalidMessageException("No mulligans left");

        int indexInHand = player.hand.FindIndex(card => card.card_id == cardID);
        if (indexInHand < 0) throw new InvalidMessageException("Invalid card index");

        Card oldCard = player.hand[indexInHand];
        Card newCard = player.deck.Draw()!;
        player.hand[indexInHand] = newCard;
        player.deck.cards.Insert(0, oldCard); // Put old card back at the bottom of the deck
        player.mulligansRemaining--;


        SendToAll(new S2CMulliganResult {
            player = player.index,
            oldCardId = oldCard.card_id,
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

    void CheckForMulliganEnd() {
        if (players.All((player) => player.mulligansRemaining == 0)) {
            state = GamePhase.Blind;
            SendToAll(new S2CBlindPhaseStart());
        }
    }


    void HandleDoneWithMulligan(ServerPlayer player) {
        if (state != GamePhase.Mulligan) throw new InvalidMessageException("Not in mulligan phase");

        if (player.mulligansRemaining > 0) {
            player.mulligansRemaining = 0;
            SendToAll(new S2CDoneWithMulliganResult { player = player.index });
            CheckForMulliganEnd();
        }
    }

    void HandleBlindStageDone(ServerPlayer player, C2S_BlindStage_Done blindStageDone) {
        if (state != GamePhase.Blind) throw new InvalidMessageException("Not in blind phase");
        if (blindStageDone.cardsPlayed.Length > GameRules.BlindStageCards) throw new InvalidMessageException("Too many cards");
        if (player.blindStageDone) throw new InvalidMessageException("Player already done with blind stage");

        List<C2S_BlindStage_PlayCard> validPlays = [];
        foreach (C2S_BlindStage_PlayCard playCard in blindStageDone.cardsPlayed) {
            int indexInHand = player.hand.FindIndex((card) => playCard.cardID == card.card_id);
            if (indexInHand < 0) throw new InvalidMessageException("Invalid card id");

            if (!CheckCanPlayerPlayCardsOnField(player, playCard.position)) throw new InvalidMessageException("Invalid position");
            if (validPlays.Any(other => other.position == playCard.position)) throw new InvalidMessageException("Duplicate position");

            validPlays.Add(playCard);
        }

        foreach (C2S_BlindStage_PlayCard play in validPlays) {
            Card card = player.hand.Find(card => card.card_id == play.cardID)!;
            player.hand.Remove(card);

            board[play.position.column, play.position.row].card = card;
            card.location = CardLocation.Board(play.position);
        }

        player.blindStageDone = true;
        CheckForBlindStageEnd();
    }

    void CheckForBlindStageEnd() {
        if (players[0].blindStageDone && players[1].blindStageDone) {
            phase = GamePhase.Main;

            List<S2CMainPhaseStart.Play> plays = board
                .Cast<Field>()
                .Where(field => field.card != null)
                .Select(field => new S2CMainPhaseStart.Play {
                    position = field.card.location.BoardPosition,
                    cardInfo = GetCardInfo(field.card),
                })
                .ToList();

            SendToAll(new S2CMainPhaseStart { 
                plays = plays,
            });
        }
    }

    CardInfo GetCardInfo(Card card) {
        return new CardInfo {
            cardId = card.card_id,
            cardPrototypeId = card.prototype.id
        };
    }

    public void SendToAll(S2CMessage message) {
        foreach (ServerPlayer player in players) {
            player.SendMessage(message);
        }
    }

    Card CreateCard(CardPrototype prototype) {
        Card card = new Card(prototype, nextCardId++);
        return card;
    }
}
