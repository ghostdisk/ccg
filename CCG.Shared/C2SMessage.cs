namespace CCG.Shared;
using MessagePack;

[MessagePackObject]
[Union(1, typeof(C2SJoinMatchmaking))]
[Union(2, typeof(C2SLeaveMatchmaking))]
public abstract class C2SMessage {
}

[MessagePackObject]
public class C2SJoinMatchmaking : C2SMessage {
}

[MessagePackObject]
public class C2SLeaveMatchmaking : C2SMessage {
}
