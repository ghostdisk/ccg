namespace CCG.Client;

using CCG.Shared;
using MessagePack;
using WebSocketSharp;

public enum ClientState {
    NotConnected,
    Connecting,
    Connected,
}

public class Client {
    protected ClientState state = ClientState.NotConnected;
    protected MatchmakingState matchmakingState = MatchmakingState.NotJoined;

    public List<Card> hand = new List<Card>();
    public int mulligansRemaining = 0;

    private WebSocket ws = null!;

    public Client() {
        ws = new WebSocket("ws://localhost:4444/ws");
        ws.OnOpen += OnWSOpen;
        ws.OnClose += OnWSClose;
        ws.OnMessage += OnWSMessage;
        ws.OnError += OnWSError;
    }

    public void Connect() {
        if (state != ClientState.NotConnected)
            throw new Exception("Attempting to Connect in an invalid state");

        state = ClientState.Connecting;
        OnConnecting();
        ws.Connect();
    }

    public void Send(C2SMessage message) {
        byte[] data = MessagePackSerializer.Serialize<C2SMessage>(message);
        ws.Send(data);
    }

    public void JoinMatchmaking() {
        if (matchmakingState == MatchmakingState.NotJoined) {
            matchmakingState = MatchmakingState.Joining;
            OnMatchmakingStateChanged(matchmakingState);
            Send(new C2SJoinMatchmaking());
        }
    }

    public virtual void OnGameStarted() {
    }

    public void LeaveMatchmaking() {
        if (matchmakingState == MatchmakingState.Joined) {
            matchmakingState = MatchmakingState.Leaving;
            OnMatchmakingStateChanged(matchmakingState);
            Send(new C2SLeaveMatchmaking());
        }
    }

    protected virtual void ExecOnMainThread(Action action) {
        action();
    }

    protected virtual void OnMatchmakingStateChanged(MatchmakingState matchmakingState) {
    }

    protected virtual void HandleMessage(S2CMessage message) {
        switch (message) {
            case S2CMatchmakingState matchmakingStateMessage:
                matchmakingState = matchmakingStateMessage.state;
                OnMatchmakingStateChanged(matchmakingState);
                break;
            case S2CGameStarted gameStarted:
                hand = gameStarted.initialHand;
                mulligansRemaining = GameRules.MaxMulliganCount;
                OnGameStarted();
                break;
            case S2CMulliganResult mulliganResult:
                HandleMulliganResult(mulliganResult);
                break;
            case S2CMulliganDone mulliganDone:
                OnMulliganDone();
                break;
        }
    }

    protected virtual void OnConnecting() {
    }

    protected virtual void OnConnected() {
    }

    protected virtual void OnError(string error) {
        Console.WriteLine("OnError: " + error);
    }

    protected virtual void OnLostConnection(String reason) {
    }

    private void OnWSOpen(object sender, EventArgs e) {
        state = ClientState.Connected;
        ExecOnMainThread(OnConnected);
    }

    private void OnWSClose(object sender, CloseEventArgs e) {
        state = ClientState.NotConnected;
        ExecOnMainThread(() => OnLostConnection(e.Reason));
    }

    private void OnWSMessage(object sender, MessageEventArgs args) {
        if (!args.IsBinary) {
            OnError("Unexpected binary message");
            return;
        }

        try {
            S2CMessage? message = MessagePackSerializer.Deserialize<S2CMessage>(args.RawData);
            if (message != null) {
                ExecOnMainThread(() => HandleMessage(message));
            }
        }
        catch (Exception ex) {
            OnError("Got exception " + ex.Message);
            return;
        }
    }

    private void OnWSError(object sender, ErrorEventArgs e) {
        OnError("Got exception " + e.Message);
    }

    protected virtual void HandleMulliganResult(S2CMulliganResult mulliganResult) {
        if (mulliganResult.playerIndex == 0) { // This client's card
            hand[mulliganResult.indexInHand] = new Card(CardDatabase.CardPrototypes[mulliganResult.newCardId]);
            mulligansRemaining--;
        }
    }

    protected virtual void OnMulliganDone() {
        // This can be overridden by CLIClient to log the event
    }
}

