namespace CCG.Shared;
using MessagePack;

[MessagePackObject]
[Union(1, typeof(S2CErrorNotify))]
[Union(2, typeof(S2CGameStarted))]
[Union(3, typeof(S2CMatchmakingState))]
public abstract class S2CMessage {
}

[MessagePackObject]
public class S2CErrorNotify : S2CMessage {
    [Key(0)]
    public string message = "";
}

[MessagePackObject]
public class S2CGameStarted : S2CMessage {
}

[MessagePackObject]
public class S2CMatchmakingState : S2CMessage {
    [Key(0)]
    public MatchmakingState state = MatchmakingState.NotJoined;
}
