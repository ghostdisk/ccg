namespace CCG.Shared;

public class Deck {

    public List<Card> cards;

    public Deck() {
        cards = new List<Card>();
    }

    public void Shuffle() {
        Console.WriteLine("Shuffle() not implemented TODO");
    }

    public Card? Draw() {
        if (this.cards.Count > 0) {
            Card card = this.cards[this.cards.Count - 1];
            this.cards.RemoveAt(this.cards.Count - 1);
            return card;
        }
        else {
            return null;
        }
    }

}
