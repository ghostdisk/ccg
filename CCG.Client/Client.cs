namespace CCG.Client;

using CCG.Shared;
using MessagePack;
using System.Collections.Concurrent;
using WebSocketSharp;

public enum ClientState {
    NotConnected,
    Connecting,
    Connected,
}

public abstract class Client<TGame> where TGame : ClientGame {
    public TGame? game;

    protected ClientState state = ClientState.NotConnected;
    protected MatchmakingState matchmakingState = MatchmakingState.NotJoined;

    private WebSocket ws = null!;
    private ConcurrentQueue<Action> gameThreadActions = new();
    private ConcurrentQueue<Action> wsThreadActions = new();

    public Client() {
        ws = new WebSocket("ws://localhost:4444/ws");
    }

    // Executed on game thread
    public void ExecGameThreadCallbacks() {
        Action action;
        while (gameThreadActions.TryDequeue(out action))
            action();
    }

    // Executed on ws thread
    void ExecOnGameThread(Action action) {
        gameThreadActions.Enqueue(action);
    }

    // Executed on game thread
    void ExecOnWSThread(Action action) {
        wsThreadActions.Enqueue(action);
    }

    // Executed on game thread
    public void Send(C2SMessage message) {
        ExecOnWSThread(() => {
            byte[] data = MessagePackSerializer.Serialize(message);
            ws.Send(data);
        });
    }

    // Executed on game thread
    public void JoinMatchmaking() {
        if (matchmakingState == MatchmakingState.NotJoined) {
            matchmakingState = MatchmakingState.Joining;
            OnMatchmakingStateChanged(matchmakingState);
            Send(new C2SJoinMatchmaking());
        }
    }

    // Executed on game thread
    public void LeaveMatchmaking() {
        if (matchmakingState == MatchmakingState.Joined) {
            matchmakingState = MatchmakingState.Leaving;
            OnMatchmakingStateChanged(matchmakingState);
            Send(new C2SLeaveMatchmaking());
        }
    }

    // Executed on game thread
    protected abstract TGame CreateGame(ClientPlayer myPlayer, ClientPlayer player0, ClientPlayer player1);

    // Executed on game thread
    protected void HandleMessage(S2CMessage message) {
        if (game != null && game.HandleMessage(message)) {
            return;
        }
        switch (message) {
            case S2CPing:
                Send(new C2SPong());
                break;
            case S2CMatchmakingState matchmakingStateMessage:
                matchmakingState = matchmakingStateMessage.state;
                OnMatchmakingStateChanged(matchmakingState);
                break;
            case S2CGameStarted gameStarted:
                ClientPlayer myPlayer = new ClientPlayer(gameStarted.myPlayerIndex);
                ClientPlayer opponentPlayer = new ClientPlayer(gameStarted.myPlayerIndex == 0 ? 1 : 0);

                myPlayer.mulligansRemaining = gameStarted.myMulligans;
                opponentPlayer.mulligansRemaining = gameStarted.opponentMulligans;

                ClientPlayer player0 = gameStarted.myPlayerIndex == 0 ? myPlayer : opponentPlayer;
                ClientPlayer player1 = gameStarted.myPlayerIndex == 0 ? opponentPlayer : myPlayer;
                game = CreateGame(myPlayer, player0, player1);

                game.HandleMessage(gameStarted);

                break;
        }
    }

    // Executed on game thread
    protected virtual void OnConnecting() {
    }

    // Executed on game thread
    protected virtual void OnConnected() {
    }

    // Executed on game thread
    protected virtual void OnError(string error) {
        Console.WriteLine("OnError: " + error);
    }

    // Executed on game thread
    protected virtual void OnLostConnection(String reason) {
    }

    // Executed on game thread
    protected virtual void OnMatchmakingStateChanged(MatchmakingState matchmakingState) {
    }
    // Executed on ws thread
    public void Connect() {
        if (state != ClientState.NotConnected)
            throw new Exception("Attempting to Connect in an invalid state");

        state = ClientState.Connecting;
        Task.Run(WSThread);
    }

    // Executed on ws thread
    async Task WSThread() {
        ws.OnOpen += OnWSOpen;
        ws.OnClose += OnWSClose;
        ws.OnMessage += OnWSMessage;
        ws.OnError += OnWSError;

        ExecOnGameThread(OnConnecting);
        ws.Connect();

        while (true) {
            Action action;
            while (wsThreadActions.TryDequeue(out action))
                action();
            await Task.Delay(1); // TODO
        }
    }

    // Executed on ws thread
    private void OnWSOpen(object sender, EventArgs e) {
        state = ClientState.Connected;
        ExecOnGameThread(OnConnected);
    }

    // Executed on ws thread
    private void OnWSClose(object sender, CloseEventArgs e) {
        state = ClientState.NotConnected;
        ExecOnGameThread(() => OnLostConnection(e.Reason));
    }

    // Executed on ws thread
    private void OnWSMessage(object sender, MessageEventArgs args) {
        if (!args.IsBinary) {
            OnError("Unexpected binary message");
            return;
        }

        try {
            S2CMessage? message = MessagePackSerializer.Deserialize<S2CMessage>(args.RawData);
            if (message != null) {
                ExecOnGameThread(() => HandleMessage(message));
            }
        }
        catch (Exception ex) {
            OnError("Got exception " + ex.Message);
            return;
        }
    }

    // Executed on ws thread
    private void OnWSError(object sender, ErrorEventArgs e) {
        OnError("Got exception " + e.Message);
    }

}

