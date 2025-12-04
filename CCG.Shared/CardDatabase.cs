namespace CCG.Shared;

public static class CardDatabase {
    public static readonly CardPrototype[] CardPrototypes = {
        new CardPrototype {
            id = 0,
            name = "Null Card",
            description = "Nothing to see here."
        },
        new CardPrototype {
            id = 1,
            initial_strength = 4,
            name = "Goblin Bomber",
            description = "BOMB BOMB",
        },
        new CardPrototype {
            id = 2,
            initial_strength = 4,
            name = "Healer",
            description = "Heal Heal",
        },
        new CardPrototype {
            id = 3,
            initial_strength = 4,
            name = "Card3",
            description = "Banger card 3",
        },
        new CardPrototype {
            id = 4,
            initial_strength = 10,
            name = "Card4",
            description = "Banger card 4",
        },
    };

    public static CardPrototype NullPrototype => CardPrototypes[0];
}