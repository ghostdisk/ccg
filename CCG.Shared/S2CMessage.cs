namespace CCG.Shared;
using MessagePack;

[MessagePackObject]
[Union(1, typeof(S2CErrorNotify))]
[Union(2, typeof(S2CGameStarted))]
[Union(3, typeof(S2CMatchmakingState))]
[Union(4, typeof(S2CMulliganResult))]
[Union(5, typeof(S2CMulliganDone))]
[Union(6, typeof(S2CCardInfo))]
[Union(7, typeof(S2CDoneWithMulliganResult))]
public abstract class S2CMessage {
}

[MessagePackObject]
public class S2CErrorNotify : S2CMessage {
    [Key(0)]
    public string message = "";
}

[MessagePackObject]
public class S2CGameStarted : S2CMessage {
    [Key(0)] public List<int> myHand = new();
    [Key(1)] public List<int> opponentHand = new();
    [Key(2)] public int myMulligans = 0;
    [Key(3)] public int opponentMulligans = 0;
    [Key(4)] public List<CardInfo> myHandInfos = new();
    [Key(5)] public int myPlayerIndex = 0;
}

[MessagePackObject]
public class S2CMatchmakingState : S2CMessage {
    [Key(0)]
    public MatchmakingState state = MatchmakingState.NotJoined;
}

[MessagePackObject]
public class S2CMulliganResult : S2CMessage {
    [Key(0)]
    public int player;
    [Key(1)]
    public int indexInHand;
    [Key(2)]
    public int newCardId;
    [Key(3)]
    public int mulligansRemaining;
}

[MessagePackObject]
public class S2CDoneWithMulliganResult : S2CMessage {
    [Key(0)]
    public int player;
}

[MessagePackObject]
public class S2CMulliganDone : S2CMessage {
}

[MessagePackObject]
public class S2CCardInfo : S2CMessage {
    [Key(0)]
    public List<CardInfo> cardInfos = new List<CardInfo>();
}
