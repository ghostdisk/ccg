namespace CCG.Shared;
using MessagePack;

[MessagePackObject]
[Union(1, typeof(C2SJoinMatchmaking))]
[Union(2, typeof(C2SLeaveMatchmaking))]
[Union(3, typeof(C2SMulliganSwap))]
[Union(4, typeof(C2SDoneWithMulligan))]
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
