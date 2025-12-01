namespace CCG.Shared;


public class Game<TPlayer, TGame>
   where TPlayer : Player<TPlayer, TGame>
   where TGame : Game<TPlayer, TGame> {

    public TPlayer player1;
    public TPlayer player2;

    public Game(TPlayer player1, TPlayer player2) {
        Deck deck1 = new Deck();
        Deck deck2 = new Deck();

        this.player1 = player1;
        this.player2 = player2;

        player1.game = (TGame)this;
        player2.game = (TGame)this;

        player1.opponent = player2;
        player2.opponent = player1;
    }

    public TPlayer GetPlayer(int index) {
        return index > 0 ? player2 : player1;
    }
}
