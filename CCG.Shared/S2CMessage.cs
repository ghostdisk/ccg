namespace CCG.Shared;
using MessagePack;

[MessagePackObject]
[Union(1, typeof(S2CErrorNotify))]
[Union(2, typeof(S2CGameStarted))]
[Union(3, typeof(S2CMatchmakingState))]
[Union(4, typeof(S2CMulliganResult))]
[Union(5, typeof(S2CMulliganDone))]
public abstract class S2CMessage {
}

[MessagePackObject]
public class S2CErrorNotify : S2CMessage {
    [Key(0)]
    public string message = "";
}

[MessagePackObject]
public class S2CGameStarted : S2CMessage {
    [Key(0)]
    public List<Card> initialHand = new List<Card>();
}

[MessagePackObject]
public class S2CMatchmakingState : S2CMessage {
    [Key(0)]
    public MatchmakingState state = MatchmakingState.NotJoined;
}

[MessagePackObject]
public class S2CMulliganResult : S2CMessage {
    [Key(0)]
    public int playerIndex;
    [Key(1)]
    public int indexInHand;
    [Key(2)]
    public int newCardId;
}

[MessagePackObject]
public class S2CMulliganDone : S2CMessage {
}
