using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

class HandView : MonoBehaviour 
{
    public SplineContainer splineContainer;

    public List<CardView> cards;
    public List<float> spacings;
    public List<float> cardRotation;

    public float maxUseSpace = 1.0f;
    public DeckView deckView;



    void Start() {
        UpdatePositions();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            CardView card = deckView.CreateCardView();
            cards.Add(card);
            Debug.Log($"Created card. Now have {cards.Count}");
            UpdatePositions();
        }
        if (Input.GetMouseButtonDown(1) && cards.Count > 0) {
            CardView last = cards[cards.Count - 1];
            cards.Remove(last);
            Destroy(last.gameObject);
            UpdatePositions();
        }
    }

    void UpdatePositions() {
        if (cards.Count == 0)
            return;

        float cardSpacing = 0;
        if (cards.Count >= spacings.Count) 
            cardSpacing = maxUseSpace / cards.Count;
        else 
            cardSpacing = spacings[cards.Count];

        float firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2.0f;

        Spline spline = splineContainer.Spline;

        for (int i = 0; i < cards.Count; i++) {
            float t = firstCardPosition + i * cardSpacing;

            Vector3 forward = spline.EvaluateTangent(t);
            Vector3 up = spline.EvaluateUpVector(t);
            Vector3 position = splineContainer.transform.TransformPoint(spline.EvaluatePosition(t));

            Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized) * splineContainer.transform.rotation;
            rotation = Quaternion.Euler(0, 0, cardRotation[cards.Count <= cardRotation.Count ? cards.Count - 1 : cardRotation.Count - 1]) * rotation;

            cards[i].SetTarget(position, rotation, new Vector3(1,1,1));
            cards[i].SetSortingOrder("Hand", 10000 - i * 5);
        }

    }
}
