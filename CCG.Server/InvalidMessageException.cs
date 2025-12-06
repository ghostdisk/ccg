namespace CCG.Server;

public class InvalidMessageException : Exception {
    public InvalidMessageException(string message) : base(message) {
    }
}
