using UnityEngine;
using CCG.Shared;
using CCG.Client;

public class UnityClient : Client<UnityClientGame> {
    public GameView G;

    protected override void OnMatchmakingStateChanged(MatchmakingState matchmakingState) {
        switch (matchmakingState) {
            case MatchmakingState.NotJoined:
                G.matchmakingText.text = "";
                G.matchmakingButtonText.text = "Find Match";
                G.matchmakingButton.interactable = true;
                break;
            case MatchmakingState.Joining:
                G.matchmakingText.text = "Joining Queue...";
                G.matchmakingButtonText.text = "...";
                G.matchmakingButton.interactable = false;
                break;
            case MatchmakingState.Joined:
                G.matchmakingText.text = "In Queue";
                G.matchmakingButtonText.text = "Leave";
                G.matchmakingButton.interactable = true;
                break;
            case MatchmakingState.Leaving:
                G.matchmakingText.text = "Leaving Queue...";
                G.matchmakingButtonText.text = "...";
                G.matchmakingButton.interactable = false;
                break;
        }
    }

    protected override void OnConnecting() {
        G.connectionStateText.text = "Connecting...";
    }

    protected override void OnLostConnection(string reason) {
        Debug.Log("Lost Connection: " + reason);
        G.connectionStateText.text = "Not connected.";
        G.matchmakingPanel.SetActive(false);
    }

    protected override void OnConnected() {
        G.connectionStateText.text = "Connected!";
        G.matchmakingPanel.SetActive(true);
        OnMatchmakingStateChanged(MatchmakingState.NotJoined); // init mm panel
    }

    public void OnMatchmakingButtonPress() {
        if (matchmakingState == MatchmakingState.NotJoined)
            JoinMatchmaking();
        if (matchmakingState == MatchmakingState.Joined)
            LeaveMatchmaking();
    }

    protected override UnityClientGame CreateGame(ClientPlayer myPlayer, ClientPlayer player0, ClientPlayer player1) {
        return new UnityClientGame(this, myPlayer, player0, player1);
    }
};
