using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using CCG.Shared;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HandView : MonoBehaviour {

    private List<CardView> cards = new();

    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private List<float> spacings;
    [SerializeField] private List<float> cardRotation;
    [SerializeField] private float maxUseSpace = 1.0f;
    [SerializeField] private bool isOpponentHand = false;

    public void AddCard(CardView card) {
        cards.Add(card);
        UpdateCardsPositions();
        card.ToggleShadowCast(!isOpponentHand);
    }

    public List<CardView> RemoveAllCards() {
        List<CardView> allCards = cards;
        cards = new List<CardView>();
        return allCards;
    }

    public TransformProps[] GetCardTransformProps() {
        TransformProps[] props = new TransformProps[cards.Count];

        if (cards.Count > 0) {
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

                props[i] = new TransformProps(position, rotation);
            }
        }

        return props;
    }

    public void UpdateCardsPositions() {
        TransformProps[] props = GetCardTransformProps();
        for (int i = 0; i < cards.Count; i++) {
            cards[i].SetTarget(props[i]);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HandView))]
public class HandViewEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        HandView handView = (HandView)target;
        if (GUILayout.Button("Update Card Positions")) {
            handView.UpdateCardsPositions();
        }
    }
}
#endif
