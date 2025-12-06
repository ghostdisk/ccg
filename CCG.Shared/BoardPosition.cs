using MessagePack;

namespace CCG.Shared;

[MessagePackObject]
public struct BoardPosition {
    [Key(0)] public int column;
    [Key(1)] public int row;



    public bool Equals(BoardPosition other)
        => column == other.column && row == other.row;

    public override bool Equals(object obj)
        => obj is BoardPosition other && Equals(other);

    public override int GetHashCode()
    => HashCode.Combine(column, row);

    public static bool operator ==(BoardPosition left, BoardPosition right)
        => left.Equals(right);

    public static bool operator !=(BoardPosition left, BoardPosition right)
        => !left.Equals(right);
}
