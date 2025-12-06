using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using CCG.Client;
using CCG.Shared;

[Serializable]
public class PlayerViews {
    public HandView hand;
    public DeckView deck;
};

public class GameView : MonoBehaviour {
    public UnityClient client;

    [Header("Prefabs")]
    public CardView cardViewPrefab;

    [Header("Global Views")]
    public List<GameObject> disableWhenInactive = new();
    public PlayerViews myViews;
    public PlayerViews opponentViews;
    public MulliganView mulliganView;
    public BoardView boardView;
    public PlayCardView playCardView;
    public BlindStageView blindStageView;

    [Header("Menu UI")]
    public GameObject menuUiRoot;
    public TextMeshProUGUI connectionStateText;
    public GameObject matchmakingPanel;
    public TextMeshProUGUI matchmakingText;
    public TextMeshProUGUI matchmakingButtonText;
    public Button matchmakingButton;

    public Dictionary<CardLocation, Target> Targets = new();

    void Start() {
        menuUiRoot.SetActive(true);
        matchmakingPanel.SetActive(false);
        matchmakingButton.onClick.AddListener(OnMatchmakingButtonPress);


        client = new UnityClient();
        client.G = this;
        client.Connect();
    }

    public void Init(ClientGame game) {
        boardView.Init(this,game.myPlayer.index == 1);
        myViews.hand.Init(this, game.myPlayer.index);
        opponentViews.hand.Init(this, game.myPlayer.opponent.index);
        blindStageView.Init(this);
        playCardView.Init(this);
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
