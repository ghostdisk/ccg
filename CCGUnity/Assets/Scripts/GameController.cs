using UnityEngine;
using CCG.Shared;
using CCG.Client;
using TMPro;
using System.Collections.Concurrent;
using System;
using UnityEngine.UI;
using System.Collections.Generic; // Added for Dictionary
#if UNITY_EDITOR
using UnityEditor; // Added for editor functionality
#endif

public class UnityClient : Client<UnityClientGame> {
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

public class UnityClientGame : ClientGame {
    UnityClient client;
    GameController GC;

    public UnityClientGame(UnityClient client, ClientPlayer myPlayer, ClientPlayer player0, ClientPlayer player1) : base(myPlayer, player0, player1) {
        this.client = client;
        this.GC = client.GC;
        GC.menuUiRoot.SetActive(false);
    }

    public void MulliganCard(int index) {
        Debug.Log($"UnityClientGame: Mulligan Card at index {index}");
        client.Send(new C2SMulliganSwap { indexInHand = index });
    }

    public void DoneWithMulligan() {
        Debug.Log("UnityClientGame: Done with mulligan");
        client.Send(new C2SDoneWithMulligan());
    }

    protected override void S2CMulliganResultHandler(S2CMulliganResult mulliganResult) {
        base.S2CMulliganResultHandler(mulliganResult);
        Debug.Log($"UnityClientGame: [Mulligan] p{mulliganResult.player} swapped {mulliganResult.indexInHand} -> {mulliganResult.newCardId}. {mulliganResult.mulligansRemaining} muls left.");
    }

    protected override void S2CMulliganDoneHandler(S2CMulliganDone mulliganDone) {
        Debug.Log("UnityClientGame: [Mulligan] Mulligan done.");
    }

    protected override void S2CDoneWithMulliganResultHandler(S2CDoneWithMulliganResult doneWithMulliganResult) {
        Debug.Log($"UnityClientGame: [Mulligan] p{doneWithMulliganResult.player} done with mulligan.");
    }
}

public class GameController : MonoBehaviour {
    public UnityClient client;
    public Button matchmakingButton;
    public ConcurrentQueue<Action> actionQueue;
    public GameObject fieldPrefab;
    public GameObject fieldGraphicsRoot;
    public Dictionary<Vector2Int, Field> fields; // Added for field storage

    [Header("Menu UI")]
    public GameObject menuUiRoot;
    public TextMeshProUGUI connectionStateText;
    public GameObject matchmakingPanel;
    public TextMeshProUGUI matchmakingText;
    public TextMeshProUGUI matchmakingButtonText;

    [Header("Board")]
    public Material myFieldMaterial;
    public Material opponentFieldMaterial;

    void Start() {
        menuUiRoot.SetActive(true);
        actionQueue = new ConcurrentQueue<Action>();

        client = new UnityClient();
        client.GC = this;

        connectionStateText.text = "Not connected.";
        matchmakingPanel.SetActive(false);

        client.Connect();

        fields = new Dictionary<Vector2Int, Field>();
        foreach (Field field in fieldGraphicsRoot.GetComponentsInChildren<Field>()) {
            fields.Add(new Vector2Int(field.x, field.y), field);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Init Board")]
    public void InitBoard() {
        // Clear existing fields
        while (fieldGraphicsRoot.transform.childCount > 0) {
            DestroyImmediate(fieldGraphicsRoot.transform.GetChild(0).gameObject);
        }

        // Create the field
        for (int x = 0; x < 7; x++) {
            for (int y = 0; y < 6; y++) {
                GameObject fieldGO = Instantiate(fieldPrefab);
                fieldGO.transform.position = new Vector3(x, 0, y);
                fieldGO.transform.parent = fieldGraphicsRoot.transform;
                fieldGO.name = $"Field {x}:{y}";

                Material material = y < 3 ? myFieldMaterial : opponentFieldMaterial;
                fieldGO.GetComponentInChildren<MeshRenderer>().material = material;

                Field fieldComponent = fieldGO.GetComponent<Field>();
                if (fieldComponent == null) {
                    Debug.LogError($"Field component not found on prefab for field {x}:{y}. Please add a Field component to the fieldPrefab.");
                } else {
                    fieldComponent.x = x;
                    fieldComponent.y = y;
                }
            }
        }
    }
#endif

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
