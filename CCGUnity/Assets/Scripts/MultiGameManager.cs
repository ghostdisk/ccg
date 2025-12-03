using UnityEngine;
using System.Linq;

class MultiGameManager : MonoBehaviour {
    [SerializeField] private GameController[] gameControllers;
    [SerializeField] private GameObject mainCamera;

    private GameController currentGameController;
    private Vector3 cameraInitialPos;
    private Vector3 cameraTargetPos;
    private Vector3 cameraOffset;

    void Start() {
        gameControllers = gameControllers.Where(gc => gc.isActiveAndEnabled).ToArray();

        cameraInitialPos = mainCamera.transform.position;
        cameraTargetPos = cameraInitialPos;
        cameraOffset = gameControllers[1].transform.position - gameControllers[0].transform.position;

        SetCurrentGameController(gameControllers[0]);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            cameraTargetPos = cameraInitialPos;
            SetCurrentGameController(gameControllers[0]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            cameraTargetPos = cameraInitialPos + cameraOffset;
            SetCurrentGameController(gameControllers[1]);
        }

        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, cameraTargetPos, 15 * Time.deltaTime);
    }

    void SetCurrentGameController(GameController newCurrent) {
        foreach (GameController gc in gameControllers) {
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
