using UnityEngine;
using TMPro;
using System.Collections.Concurrent;
using System;
using UnityEngine.UI;
using CCG.Client;

[Serializable]
class PlayerViews {
    public HandView hand;
    public DeckView deck;
};

class GameController : MonoBehaviour {
    public UnityClient client;
    public Button matchmakingButton;
    public ConcurrentQueue<Action> actionQueue;

    [Header("Prefabs")]
    public CardView cardViewPrefab;

    [Header("Global Views")]
    public PlayerViews myViews;
    public PlayerViews opponentViews;
    public MulliganView mulliganView;

    [Header("Menu UI")]
    public GameObject menuUiRoot;
    public TextMeshProUGUI connectionStateText;
    public GameObject matchmakingPanel;
    public TextMeshProUGUI matchmakingText;
    public TextMeshProUGUI matchmakingButtonText;

    void Start() {
        menuUiRoot.SetActive(true);
        actionQueue = new ConcurrentQueue<Action>();

        client = new UnityClient();
        client.GC = this;

        connectionStateText.text = "Not connected.";
        matchmakingPanel.SetActive(false);

        client.Connect();
    }

    void Update() {
        Action action;
        while (actionQueue.TryDequeue(out action)) {
            action();
        }
    }

    public void OnMatchmakingButtonPress() {
        client.OnMatchmakingButtonPress();
    }

    public PlayerViews GetPlayerViews(ClientPlayer player) {
        return player == client.game.myPlayer ? myViews : opponentViews;
    }
}
