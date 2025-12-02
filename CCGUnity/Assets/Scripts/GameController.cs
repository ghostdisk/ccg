using UnityEngine;
using TMPro;
using System.Collections.Concurrent;
using System;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    public UnityClient client;
    public Button matchmakingButton;
    public ConcurrentQueue<Action> actionQueue;
    public GameObject fieldPrefab;
    public GameObject fieldGraphicsRoot;

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
}
