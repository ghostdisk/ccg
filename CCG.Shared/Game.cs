namespace CCG.Shared;


public class Game<TPlayer, TGame>
   where TPlayer : Player<TPlayer, TGame>
   where TGame : Game<TPlayer, TGame> {

    public TPlayer[] players;

    public Game(TPlayer player0, TPlayer player1) {
        Deck deck1 = new Deck();
        Deck deck2 = new Deck();
        players = new TPlayer[] { player0, player1 };

        player0.game = (TGame)this;
        player1.game = (TGame)this;

        player0.opponent = player1;
        player1.opponent = player0;
    }

    public TPlayer GetPlayer(int index) {
        return players[index];
    }
}
