namespace CCG.Shared;

using MessagePack;

[MessagePackObject]
public class Card {
    [Key(0)]
    public int card_id;
    [IgnoreMember]
    public int strength;
    [IgnoreMember]
    public CardPrototype prototype;

    public Card() {
        this.card_id = 0; // Default for deserialization
        this.strength = 0;
        this.prototype = null!;
    }

    public Card(CardPrototype prototype, int card_id) {
        this.prototype = prototype;
        this.strength = prototype.initial_strength;
        this.card_id = card_id;
    }
}
