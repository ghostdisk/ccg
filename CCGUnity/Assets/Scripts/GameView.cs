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
    public Target[,] fieldTargets;

    [Header("Prefabs")]
    public CardView cardViewPrefab;
    public Target fieldTargetPrefab;

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

        fieldTargets = new Target[GameRules.Columns, GameRules.Rows];
        for (int column = 0; column < GameRules.Columns; column++) {
            for (int row = 0; row < GameRules.Rows; row++) {
                Target fieldTarget = Instantiate(fieldTargetPrefab);

                fieldTarget.position = new Position {
                    column = column,
                    row = row,
                };

                fieldTarget.transform.SetParent(transform);
                fieldTarget.transform.localPosition = new Vector3(column, 0, row);
                fieldTarget.Deactivate();

                fieldTargets[column, row] = fieldTarget;
            }
        }

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
