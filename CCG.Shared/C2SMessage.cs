namespace CCG.Shared;
using MessagePack;

[MessagePackObject]
[Union(1, typeof(C2SJoinMatchmaking))]
[Union(2, typeof(C2SLeaveMatchmaking))]
[Union(3, typeof(C2SMulliganSwap))]
[Union(4, typeof(C2SDoneWithMulligan))]
[Union(5, typeof(C2SPong))]
[Union(6, typeof(C2S_BlindStage_PlaceCard))]
[Union(7, typeof(C2S_BlindStage_ReturnCard))]
[Union(8, typeof(C2S_BlindStage_Done))]
public abstract class C2SMessage {
}

[MessagePackObject]
public class C2SJoinMatchmaking : C2SMessage {
}

[MessagePackObject]
public class C2SLeaveMatchmaking : C2SMessage {
}

[MessagePackObject]
public class C2SMulliganSwap : C2SMessage {
    [Key(0)]
    public int indexInHand;
}

[MessagePackObject]
public class C2SDoneWithMulligan : C2SMessage {
}

[MessagePackObject]
public class C2SPong : C2SMessage {
}

[MessagePackObject]
public class C2S_BlindStage_PlaceCard : C2SMessage {
    [Key(0)]
    public int cardID;

    [Key(1)]
    public Position position;
}

[MessagePackObject]
public class C2S_BlindStage_ReturnCard : C2SMessage {
    [Key(0)]
    public int cardID;
}

[MessagePackObject]
public class C2S_BlindStage_Done : C2SMessage {
}
