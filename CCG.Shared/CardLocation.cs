namespace CCG.Shared;

public enum CardLocationType {
    None,
    Hand,
    Board,
};

public struct CardLocation {
    public CardLocationType type;
    public int val1;
    public int val2;

    public CardLocation(CardLocationType type, int val1, int val2) {
        this.type = type;
        this.val1 = val1;
        this.val2 = val2;
    }

    public static CardLocation None => new CardLocation(CardLocationType.None, 0, 0);

    public static CardLocation Board(BoardPosition position) => new CardLocation(CardLocationType.Board, position.column, position.row);
    public static CardLocation Hand(int player, int indexInHand) => new CardLocation(CardLocationType.Hand, player, indexInHand);

    public override string ToString() {
        switch (type) {
            case CardLocationType.Hand:
                return $"CardLocation.Hand(Player = {val1}, IndexInHand = {val2})";
            case CardLocationType.Board:
                return $"CardLocation.Board(Column = {val1}, Row = {val2})";
            default:
                return $"CardLocation(type = {type}, val1 = {val1}, val2 = {val2})";
        }
    }

    public int IndexInHand {
        get {
            if (type != CardLocationType.Hand)
                throw new Exception("Invalid location type");
            return val2;
        }
        set {
            if (type != CardLocationType.Hand)
                throw new Exception("Invalid location type");
            val2 = value;
        }
    }

    public BoardPosition BoardPosition {
        get {
            if (type != CardLocationType.Board)
                throw new Exception("Invalid location type");
            return new BoardPosition { column = val1, row = val2 };
        }
        set {
            if (type != CardLocationType.Board)
                throw new Exception("Invalid location type");
            val1 = value.column;
            val2 = value.row;
        }
    }

}
