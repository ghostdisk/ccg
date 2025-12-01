using CCG.CLIClient;
using System.Collections.Concurrent;
using CCG.Shared;
using System.Threading.Tasks;

public class Program {
    private static ConcurrentDictionary<int, CLIClient> clients = new ConcurrentDictionary<int, CLIClient>();
    private static int currentClientIndex = 0;

    public static void Main(string[] args) {
        Console.WriteLine("CCG CLI Client. Type 'help' for commands.");
        clients.TryAdd(0, new CLIClient(0));

        while (true) {
            Console.Write($"{currentClientIndex}> ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) {
                continue;
            }

            string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string command = parts[0].ToLower();

            switch (command) {
                case "client":
                    if (parts.Length > 1 && int.TryParse(parts[1], out int clientNum)) {
                        SwitchClient(clientNum);
                    } else {
                        Console.WriteLine("Usage: client <number>");
                    }
                    break;
                case "connect":
                    ConnectClient();
                    break;
                case "mm":
                    if (parts.Length > 1) {
                        HandleMatchmakingCommand(parts[1].ToLower());
                    } else {
                        Console.WriteLine("Usage: mm join/leave");
                    }
                    break;
                case "exit":
                    Console.WriteLine("Exiting...");
                    return;
                case "help":
                    PrintHelp();
                    break;
                default:
                    Console.WriteLine("Unknown command. Type 'help' for commands.");
                    break;
            }
        }
    }

    private static void SwitchClient(int clientNum) {
        if (!clients.ContainsKey(clientNum)) {
            clients.TryAdd(clientNum, new CLIClient(clientNum));
            Console.WriteLine($"Created new client {clientNum}");
        }
        currentClientIndex = clientNum;
        Console.WriteLine($"Switched to client {currentClientIndex}");
    }

    private static void ConnectClient() {
        if (clients.TryGetValue(currentClientIndex, out CLIClient? client)) {
            client.Connect();
        } else {
            Console.WriteLine($"Client {currentClientIndex} not found. Switch to or create it first.");
        }
    }

    private static void HandleMatchmakingCommand(string subCommand) {
        if (clients.TryGetValue(currentClientIndex, out CLIClient? client)) {
            switch (subCommand) {
                case "join":
                    client.JoinMatchmaking();
                    break;
                case "leave":
                    client.LeaveMatchmaking();
                    break;
                default:
                    Console.WriteLine("Usage: mm join/leave");
                    break;
            }
        } else {
            Console.WriteLine($"Client {currentClientIndex} not found. Switch to or create it first.");
        }
    }

    private static void PrintHelp() {
        Console.WriteLine("Available commands:");
        Console.WriteLine("  client <number>   - Switch to or create client <number>");
        Console.WriteLine("  connect           - Connect current client to the server");
        Console.WriteLine("  hand              - Display current hand");
        Console.WriteLine("  mulligan <index>  - Mulligan card at index");
        Console.WriteLine("  mulligan done     - End mulligan phase");
        Console.WriteLine("  mm join           - Join matchmaking");
        Console.WriteLine("  mm leave          - Leave matchmaking");
        Console.WriteLine("  exit              - Exit the application");
        Console.WriteLine("  help              - Display this help message");
    }
}