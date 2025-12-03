using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;

class MulliganView : MonoBehaviour {
    public List<Transform> positions;
    public ParticleSystem fog;
    public GameObject ui;

    [NonSerialized] public List<CardView>? cards;

    public async Task Activate(List<CardView> cards) {
        if (cards.Count > positions.Count) {
            Debug.LogError("There are more cards in hand than mulligan positions set up.\n");
        }
        this.cards = cards;

        fog.Play();

        await Task.WhenAll(cards.Select(async (card, index) => {
            await Task.Delay(index * 75);
            card.SetTarget(new TransformProps(positions[index]));
        }).ToArray());

        ui.SetActive(true);
    }

    public List<CardView> Deactivate() {
        ui.SetActive(false);
        fog.Stop();
        List<CardView> allCards = cards;
        cards = null;
        return cards;
    }
}
