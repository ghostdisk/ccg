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
            description = "Active: Deal 5 damage in a 3x3 area around this card",
        },
        new CardPrototype {
            id = 2,
            initial_strength = 3,
            name = "Healer",
            description = "End of Turn: Give 1 health to all the nearest units",
        },
        new CardPrototype {
            id = 3,
            initial_strength = 12,
            name = "Statue",
            description = "",
        },
        new CardPrototype {
            id = 4,
            initial_strength = 4,
            name = "Archer",
            description = "Active: Target a field. Start of Turn: Fire an arrow dealing 2 damage to that field.",
        },
    };

    public static CardPrototype NullPrototype => CardPrototypes[0];
}