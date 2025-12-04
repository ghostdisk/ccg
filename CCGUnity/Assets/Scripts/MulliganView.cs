using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;

public class MulliganView : MonoBehaviour {

    public List<Transform> positions;
    public CardView cardViewPrefab;

    [NonSerialized] public List<CardView> cards;
    [SerializeField] private ParticleSystem fog;
    [SerializeField] private GameObject ui;
    [SerializeField] private Transform firstDiscardPosition;
    [SerializeField] private float discardClimbPerCard = 0.2f;
    [SerializeField] private float discardRandomRotation = 45.0f;

    private float discardY = 0;

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

    public TransformProps GetNextDiscardTransformProps() {
        TransformProps props = new TransformProps(firstDiscardPosition);
        props.position.y += discardY;
        props.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(-discardRandomRotation / 2.0f, discardRandomRotation / 2.0f), 0);
        discardY += discardClimbPerCard;
        return props;
    }
}
