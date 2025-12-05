namespace CCG.Server;

using WebSocketSharp;
using WebSocketSharp.Server;
using MessagePack;
using CCG.Shared;

class Connection : WebSocketBehavior {

    public ServerGame? game;
    public ServerPlayer? player;
    public bool inMatchmaking;
    public int PingDelta { get; private set; }

    private Timer? heartbeatTimer = null!;

    protected override void OnOpen() {
        heartbeatTimer = new Timer((_) => {
            SendMessage(new S2CPing());
            PingDelta++;
            if (PingDelta > 5) {
                Close();
            }
        }, null, 1000, 1000);
    }

    public void SendMessage(S2CMessage message) {
        byte[] data = MessagePackSerializer.Serialize(message);
        Send(data);
    }

    protected override void OnMessage(MessageEventArgs args) {
        if (!args.IsBinary) {
            Context.WebSocket.Close(CloseStatusCode.InvalidData);
            return;
        }

        C2SMessage? message;
        try {
            message = MessagePackSerializer.Deserialize<C2SMessage>(args.RawData);
        }
        catch (Exception ex) {
            Console.WriteLine("OnMessage: " + ex.Message);
            Context.WebSocket.Close(CloseStatusCode.InvalidData);
            return;
        }
        if (message != null) {
            if ((message as C2SPong) != null) {
                PingDelta--;
            }
            Server.HandleMessage(this, message);
        }
    }

    protected override void OnClose(CloseEventArgs args) {
        heartbeatTimer?.Dispose();
        heartbeatTimer = null;
        Server.HandleConnectionClose(this);
    }

    protected override void OnError(ErrorEventArgs args) {
    }

    void Close() {
        if (Context.WebSocket.ReadyState == WebSocketState.Open) {
            Context.WebSocket.Close();
        }
    }
}
