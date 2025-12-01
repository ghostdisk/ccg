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
            initial_strength = 5,
            name = "Card1",
            description = "Banger card 1",
        },
        new CardPrototype {
            id = 2,
            initial_strength = 7,
            name = "Card2",
            description = "Banger card 2",
        },
        new CardPrototype {
            id = 3,
            initial_strength = 4,
            name = "Card3",
            description = "Banger card 3",
        },
    };
}
