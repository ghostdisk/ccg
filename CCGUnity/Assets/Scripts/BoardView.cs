using CCG.Shared;
using UnityEngine;

public class BoardView : MonoBehaviour {

    public Target[,] fieldTargets;

    [SerializeField] private Target fieldTargetPrefab;

    public Target GetTarget(Position position) {
        return fieldTargets[position.column, position.row];
    }

    void Awake() {
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
        
    }
}
