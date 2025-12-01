namespace CCG.Shared;

public class Player<TPlayer, TGame>
    where TPlayer : Player<TPlayer, TGame>
    where TGame : Game<TPlayer, TGame> {

    public TGame game = null!;
    public Deck deck = null!;
    public TPlayer opponent = null!;
    public int mulligansRemaining = 0;
    public List<Card> hand = new List<Card>();

    public Player() {
    }
}
