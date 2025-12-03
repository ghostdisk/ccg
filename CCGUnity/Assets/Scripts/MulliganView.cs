using UnityEngine;
using System.Collections.Generic;
using System;

class MulliganView : MonoBehaviour {
    public List<Transform> positions;
    public ParticleSystem fog;
    public GameObject ui;

    [NonSerialized] public List<CardView> cards;
    HandView hand;


    public void Activate(List<CardView> cards) {
        if (cards.Count > positions.Count) {
            Debug.LogError("There are more cards in hand than mulligan positions set up.\n");
        }
        this.cards = cards;

        for (var i = 0; i < Math.Min(cards.Count, positions.Count); i++) {
            cards[i].SetTarget(new TransformProps(positions[i]));
        }
        fog.Play();

        ui.SetActive(true);
    }

    public void Deactivate() {
        ui.SetActive(false);
        fog.Stop();
    }
}
