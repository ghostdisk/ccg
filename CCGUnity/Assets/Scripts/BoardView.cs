using CCG.Shared;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BoardView : MonoBehaviour {

    [SerializeField] private Target fieldTargetPrefab;
    GameView G;

    public IEnumerable<Target> BoardTargets
        => G.Targets.Values.Where(target => target.location.type == CardLocationType.Board);

    public void Init(GameView G) {
        this.G = G;
        for (int column = 0; column < GameRules.Columns; column++) {
            for (int row = 0; row < GameRules.Rows; row++) {
                Target fieldTarget = Instantiate(fieldTargetPrefab);

                fieldTarget.location = CardLocation.Board(new BoardPosition { column = column, row = row });

                fieldTarget.transform.SetParent(transform);
                fieldTarget.transform.localPosition = new Vector3(column, 0, row);
                fieldTarget.Deactivate();

                G.Targets[fieldTarget.location] = fieldTarget;
            }
        }
    }

}
