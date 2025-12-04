using UnityEngine;
using System.Linq;

class MultiGameManager : MonoBehaviour {
    [SerializeField] private GameView[] gameViews;
    [SerializeField] private GameObject mainCamera;

    private GameView currentGameController;
    private Vector3 cameraInitialPos;
    private Vector3 cameraTargetPos;
    private Vector3 cameraOffset;

    void Start() {
        gameViews = gameViews.Where(gc => gc.isActiveAndEnabled).ToArray();

        cameraInitialPos = mainCamera.transform.position;
        cameraTargetPos = cameraInitialPos;
        cameraOffset = gameViews[1].transform.position - gameViews[0].transform.position;

        SetCurrentGameView(gameViews[0]);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            cameraTargetPos = cameraInitialPos;
            SetCurrentGameView(gameViews[0]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            cameraTargetPos = cameraInitialPos + cameraOffset;
            SetCurrentGameView(gameViews[1]);
        }

        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, cameraTargetPos, 15 * Time.deltaTime);
    }

    void SetCurrentGameView(GameView newCurrent) {
        foreach (GameView gc in gameViews) {
            foreach (GameObject go in gc.disableWhenInactive) {
                go.SetActive(false);
            }
        }

        currentGameController = newCurrent;

        foreach (GameObject go in newCurrent.disableWhenInactive) {
            go.SetActive(true);
        }
    }
}
