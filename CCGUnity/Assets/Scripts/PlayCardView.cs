using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayCardView : MonoBehaviour {
    [SerializeField] Transform playedCardPosition;

    CardView card;
    bool fromDrag;
    Action<CardView, Target> callback;

    public void Activate(CardView card, bool fromDrag, IEnumerable<Target> targets, Action<CardView, Target> callback) {
        this.card = card;
        this.fromDrag = fromDrag;
        this.callback = callback;
        card.SetTarget(new TransformProps(playedCardPosition));

        foreach (Target target in targets) {
            target.Activate();
        }
    }

    void Update() {
        if (card) {
            if (Input.GetKeyDown(KeyCode.Escape))
                Finish(null);
            if (Input.GetMouseButtonDown(1))
                Finish(null);

            bool finish = fromDrag ? Input.GetMouseButtonUp(0) : Input.GetMouseButtonDown(0);
            if (finish)
                Finish(Target.hoveredTarget);
        }
    }

    void Finish(Target target) {
        CardView card = this.card;
        this.card = null;
        callback(card, target);
    }
}
