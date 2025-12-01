namespace CCG.Shared;

public class Deck {

    public List<Card> cards;

    public Deck() {
        cards = new List<Card> {
            new Card(CardDatabase.CardPrototypes[1]),
            new Card(CardDatabase.CardPrototypes[1]),
            new Card(CardDatabase.CardPrototypes[1]),
            new Card(CardDatabase.CardPrototypes[1]),
            new Card(CardDatabase.CardPrototypes[2]),
            new Card(CardDatabase.CardPrototypes[2]),
            new Card(CardDatabase.CardPrototypes[2]),
            new Card(CardDatabase.CardPrototypes[2]),
            new Card(CardDatabase.CardPrototypes[3]),
            new Card(CardDatabase.CardPrototypes[3]),
            new Card(CardDatabase.CardPrototypes[3]),
            new Card(CardDatabase.CardPrototypes[3]),
        };
    }

    public void Shuffle() {
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
