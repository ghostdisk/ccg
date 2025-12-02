using CCG.CLIClient;
using CCG.Shared;
using MessagePack.Resolvers;
using System.Collections.Concurrent;
using System.Dynamic;

public class Program {
    private static ConcurrentDictionary<int, CLIClient> clients = new ConcurrentDictionary<int, CLIClient>();
    private static int currentClientIndex = 0;

    public static void Main(string[] args) {
        Console.WriteLine("CCG CLI Client. Type 'help' for commands.");
        clients.TryAdd(0, new CLIClient(0));

        while (true) {
            Console.Write($"{currentClientIndex}> ");
            string? input = Console.ReadLine();
            try {
                RunCommand(input == null ? "" : input);
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }
    }

    static void RunCommand(string fullCommand) {
        if (string.IsNullOrWhiteSpace(fullCommand)) {
            return;
        }
        string[] parts = fullCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string command = parts[0].ToLower();

        CLIClient client = GetCurrentClient();

        switch (command) {
            case "client":
            case "c":
                if (parts.Length > 1 && int.TryParse(parts[1], out int clientNum)) {
                    SwitchClient(clientNum);
                } else {
                    Console.WriteLine("Usage: client <number>");
                }
                break;
            case "connect":
            case "conn":
            case "co":
                ConnectClient();
                break;
            case "mm":
                if (parts.Length > 1) {
                    HandleMatchmakingCommand(parts[1].ToLower());
                } else {
                    Console.WriteLine("Usage: mm join/leave");
                }
                break;
            case "hand":
            case "h":
                client.game!.DisplayHand();
                break;
            case "mulligan":
            case "mul":
                if (parts.Length == 2 && int.TryParse(parts[1], out int index)) {
                    client.game!.MulliganCard(index);
                } else if (parts.Length == 2 && parts[1].ToLower() == "done") {
                    client.game!.DoneWithMulligan();
                } else {
                    client.Log("Usage: mulligan <index> or mulligan done");
                }
                break;
            case "quit":
            case "exit":
            case "q":
                Console.WriteLine("Exiting...");
                return;
            case "quiet":
                if (parts.Length == 2 && parts[1] == "on") {
                    client.quiet = false;
                } if (parts.Length == 2 && parts[1] == "off") {
                    client.quiet = false;
                } if (parts.Length == 1) {
                    client.quiet = !client.quiet;
                }
                client.Log($"Quiet mode: {(client.quiet ? "on" : "off")}", true);
                break;
            case "help":
                PrintHelp();
                break;
            default:
                Console.WriteLine("Unknown command. Type 'help' for commands.");
                break;
        }
    }

    static CLIClient GetCurrentClient() {
        if (clients.TryGetValue(currentClientIndex, out CLIClient? client)) {
            return client;
        }
        return null!;
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
        Console.WriteLine("  client <number> | c <number>    - Switch to or create client <number>");
        Console.WriteLine("  connect | co                    - Connect current client to the server");
        Console.WriteLine("  mm join                         - Join matchmaking");
        Console.WriteLine("  mm leave                        - Leave matchmaking");
        Console.WriteLine("  hand | h                        - Display current hand");
        Console.WriteLine("  mulligan <index> | mul <index>  - Mulligan card at index");
        Console.WriteLine("  mulligan done | mul done        - End mulligan phase");
        Console.WriteLine("  exit | quit | q                 - Exit the application");
        Console.WriteLine("  help                            - Display this help message");
        Console.WriteLine("  quiet on/off                    - Disable/enable this client's logging");
    }
}