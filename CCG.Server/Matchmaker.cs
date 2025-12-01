using CCG.Shared;

namespace CCG.Server;

class Matchmaker {
    public List<Client> queue = new List<Client>();

    public void AddClient(Client client) {
        if (queue.Contains(client)) {
            return;
        }

        queue.Add(client);

        if (queue.Count >= 2) {
            Client client1 = queue[0];
            Client client2 = queue[1];
            queue.RemoveAt(0);
            queue.RemoveAt(0);
            StartGame(client1, client2);
        }
    }

    public void RemoveClient(Client client) {
        queue.Remove(client);
    }

    void StartGame(Client client1, Client client2) {
        ServerPlayer player1 = new ServerPlayer(client1);
        ServerPlayer player2 = new ServerPlayer(client2);

        ServerGame game = new ServerGame(player1, player2);
        game.Start();
    }
}
