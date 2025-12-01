namespace CCG.Shared;

using MessagePack;

[MessagePackObject]
public struct CardInfo {
    [Key(0)]
    public int cardId;
    [Key(1)]
    public int cardPrototypeId;
}