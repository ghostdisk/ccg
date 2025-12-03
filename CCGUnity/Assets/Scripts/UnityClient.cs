using System;
using UnityEngine;
using CCG.Shared;
using CCG.Client;

class UnityClient : Client<UnityClientGame> {
    public GameController GC;

    protected override void ExecOnMainThread(Action action) {
        GC.actionQueue.Enqueue(action);
    }

    protected override void OnConnecting() {
        GC.connectionStateText.text = "Connecting...";
    }

    protected override void OnError(string error) {
        ExecOnMainThread(() => Debug.Log("OnError: " + error));
    }

    protected override void OnMatchmakingStateChanged(MatchmakingState matchmakingState) {
        switch (matchmakingState) {
            case MatchmakingState.NotJoined:
                GC.matchmakingText.text = "";
                GC.matchmakingButtonText.text = "Find Match";
                GC.matchmakingButton.interactable = true;
                break;
            case MatchmakingState.Joining:
                GC.matchmakingText.text = "Joining Queue...";
                GC.matchmakingButtonText.text = "...";
                GC.matchmakingButton.interactable = false;
                break;
            case MatchmakingState.Joined:
                GC.matchmakingText.text = "In Queue";
                GC.matchmakingButtonText.text = "Leave";
                GC.matchmakingButton.interactable = true;
                break;
            case MatchmakingState.Leaving:
                GC.matchmakingText.text = "Leaving Queue...";
                GC.matchmakingButtonText.text = "...";
                GC.matchmakingButton.interactable = false;
                break;
        }
    }

    protected override void HandleMessage(S2CMessage message) {
        base.HandleMessage(message);
    }

    protected override void OnLostConnection(string reason) {
        Debug.Log("Lost Connection: " + reason);
        GC.connectionStateText.text = "Not connected.";
        GC.matchmakingPanel.SetActive(false);
    }

    protected override void OnConnected() {
        GC.connectionStateText.text = "Connected!";
        GC.matchmakingPanel.SetActive(true);
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
