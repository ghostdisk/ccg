using CCG.Shared;

namespace CCG.Server;

class Matchmaker {
    public List<Connection> queue = new List<Connection>();
    private object syncLock = new object(); 

    public void AddClient(Connection connection) {
        lock (syncLock) {
            if (!connection.inMatchmaking) {
                connection.inMatchmaking = true;

                queue.Add(connection);

                if (queue.Count >= 2) {
                    Connection conn1 = queue[0];
                    Connection conn2 = queue[1];
                    queue.RemoveAt(0);
                    queue.RemoveAt(0);
                    StartGame(conn1, conn2);
                }
            }
        }
    }

    public void RemoveClient(Connection connection) {
        lock (syncLock) {
            if (connection.inMatchmaking) {
                queue.Remove(connection);
                connection.inMatchmaking = false;
            }
        }
    }

    void StartGame(Connection conn0, Connection conn1) {
        ServerPlayer player0 = new ServerPlayer(conn0, 0);
        ServerPlayer player1 = new ServerPlayer(conn1, 1);

        ServerGame game = new ServerGame(player0, player1);
        game.Start();
    }
}
