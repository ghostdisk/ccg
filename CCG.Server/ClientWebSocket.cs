namespace CCG.Server;

using WebSocketSharp;
using WebSocketSharp.Server;
using MessagePack;
using CCG.Shared;

class ClientWebSocket : WebSocketBehavior {
    public Client client = null!;

    protected override void OnOpen() {
        client = new Client(this);
    }

    public void Send2(byte[] data) {
        Send(data);
    }

    protected override void OnMessage(MessageEventArgs args) {
        if (!args.IsBinary) {
            Context.WebSocket.Close(CloseStatusCode.InvalidData);
            return;
        }

        try {
            C2SMessage? msg = MessagePackSerializer.Deserialize<C2SMessage>(args.RawData);
            if (msg != null) {
                this.client.HandleMessage(msg);
            }
        }
        catch (Exception ex) {
            Console.WriteLine("OnMessage: " + ex.Message);
            Context.WebSocket.Close(CloseStatusCode.InvalidData);
            return;
        }
    }

    protected override void OnClose(CloseEventArgs args) {
    }

    protected override void OnError(WebSocketSharp.ErrorEventArgs args) {
    }
}
