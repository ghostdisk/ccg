namespace CCG.Shared;


public class Game<TPlayer, TGame>
   where TPlayer : Player<TPlayer, TGame>
   where TGame : Game<TPlayer, TGame> {

    public TPlayer[] players;
    public Field[,] board;

    public Game(TPlayer player0, TPlayer player1) {
        Deck deck1 = new Deck();
        Deck deck2 = new Deck();
        players = new TPlayer[] { player0, player1 };

        board = new Field[GameRules.Columns, GameRules.Rows];

        player0.game = (TGame)this;
        player1.game = (TGame)this;

        player0.opponent = player1;
        player1.opponent = player0;
    }

    public TPlayer GetPlayer(int index) {
        return players[index];
    }

    protected bool CheckIsHomeRow(TPlayer player, int row) {
        if (player.index == 0)
            return row < GameRules.HomeRows;
        else
            return row >= GameRules.Rows - GameRules.HomeRows;
    }

    protected bool CheckCanPlayerPlayCardsOnField(TPlayer player, int col, int row) {
        if (!CheckIsHomeRow(player, row))
            return false;

        if (board[col, row].card != null)
            return false;

        return true;
    }
}
