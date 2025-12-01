namespace CCG.Shared;

public class Card {
    int strength;
    public int card_id;
    CardPrototype prototype;

    public Card(CardPrototype prototype) {
        this.prototype = prototype;
        this.strength = prototype.initial_strength;
        this.card_id = prototype.id;
    }
}
