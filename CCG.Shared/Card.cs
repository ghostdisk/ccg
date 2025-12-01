namespace CCG.Shared;

public class Card {
    public int card_id;
    public int strength;
    public CardPrototype prototype;

    public Card() {
        card_id = 0; // Default for deserialization
        strength = 0;
        prototype = null!;
    }

    public Card(CardPrototype prototype, int card_id) {
        this.prototype = prototype;
        this.strength = prototype.initial_strength;
        this.card_id = card_id;
    }
}
