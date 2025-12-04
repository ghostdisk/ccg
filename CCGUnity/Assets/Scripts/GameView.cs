using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using CCG.Client;

[Serializable]
class PlayerViews {
    public HandView hand;
    public DeckView deck;
};

class GameView : MonoBehaviour {
    public UnityClient client;

    [Header("Prefabs")]
    public CardView cardViewPrefab;

    [Header("Global Views")]
    public List<GameObject> disableWhenInactive = new();
    public PlayerViews myViews;
    public PlayerViews opponentViews;
    public MulliganView mulliganView;

    [Header("Menu UI")]
    public GameObject menuUiRoot;
    public TextMeshProUGUI connectionStateText;
    public GameObject matchmakingPanel;
    public TextMeshProUGUI matchmakingText;
    public TextMeshProUGUI matchmakingButtonText;
    public Button matchmakingButton;


    void Start() {
        menuUiRoot.SetActive(true);
        matchmakingPanel.SetActive(false);
        matchmakingButton.onClick.AddListener(OnMatchmakingButtonPress);

        client = new UnityClient();
        client.G = this;
        client.Connect();
    }

    void Update() {
        client.ExecGameThreadCallbacks();
    }

    public PlayerViews GetPlayerViews(ClientPlayer player) {
        return player == client.game.myPlayer ? myViews : opponentViews;
    }

    void OnMatchmakingButtonPress() {
        client.OnMatchmakingButtonPress();
    }

}
