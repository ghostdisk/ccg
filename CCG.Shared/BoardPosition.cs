using MessagePack;

namespace CCG.Shared;

[MessagePackObject]
public struct BoardPosition {
    [Key(0)] public int column;
    [Key(1)] public int row;
}
