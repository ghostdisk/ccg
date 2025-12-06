namespace CCG.Shared;

public class Card {
    public int card_id;
    public int strength;
    public CardPrototype prototype;
    public CardLocation location = CardLocation.None;

    public Card(CardPrototype prototype, int card_id) {
        this.prototype = prototype;
        this.strength = prototype.initial_strength;
        this.card_id = card_id;
    }
}
