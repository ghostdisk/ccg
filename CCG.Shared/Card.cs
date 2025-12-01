namespace CCG.Shared;

public class Card {
    int strength;
    int card_id;
    CardPrototype prototype;

    public Card(CardPrototype prototype) {
        this.prototype = prototype;
        this.strength = prototype.initial_strength;
    }
}
