namespace CCG.Shared;

public class Deck {

    public List<Card> cards;

    public Deck() {
        cards = new List<Card>();
    }

    public void Shuffle(Random rng) {
        int n = cards.Count;
        while (n > 1) {
            int k = rng.Next(n);
            n--;
            Card temp = cards[n];
            cards[n] = cards[k];
            cards[k] = temp;
        }
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
