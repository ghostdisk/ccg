using UnityEngine;
using System.Collections.Generic;
using System;
using CCG.Shared;

public class PlayCardView : MonoBehaviour {
    [SerializeField] Transform playedCardPosition;

    GameView G;
    CardView card;
    bool fromDrag;
    Action<CardView, CardLocation> callback;

    public void Init(GameView G) {
        this.G = G;
    }

    public void Activate(CardView card, bool fromDrag, IEnumerable<CardLocation> targetLocations, Action<CardView, CardLocation> callback) {
        this.card = card;
        this.fromDrag = fromDrag;
        this.callback = callback;
        card.SetTarget(new TransformProps(playedCardPosition), CardViewTweenMode.ExponentialDecay);

        foreach (CardLocation location in targetLocations) {
            G.Targets[location].Activate();
        }
    }


    void Update() {
        if (card) {
            if (Input.GetKeyDown(KeyCode.Escape))
                Finish(CardLocation.None);
            if (Input.GetMouseButtonDown(1))
                Finish(CardLocation.None);

            bool finish = fromDrag ? Input.GetMouseButtonUp(0) : Input.GetMouseButtonDown(0);
            if (finish)
                Finish(Target.HoverLocation);
        }
    }

    void Finish(CardLocation location) {
        CardView card = this.card;
        this.card = null;
        Target.DeactivateAll();
        callback(card, location);
    }
}
