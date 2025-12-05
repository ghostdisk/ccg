using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HandView : MonoBehaviour {

    public Transform playedCardPosition;

    private List<CardView> cards = new();

    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private List<float> spacings;
    [SerializeField] private List<float> cardRotation;
    [SerializeField] private float maxUseSpace = 1.0f;
    [SerializeField] private bool isOpponentHand = false;

    private CardView cardPendingPlay;
    private bool cardPendingPlayFromDrag;
    private Func<CardView, IEnumerable<Target>> allowedTargetsFunc;
    private Action<CardView, Target> cardPlayFunc;

    public void AddCard(CardView card) {
        cards.Add(card);
        UpdateCardsPositions();
        card.ToggleShadowCast(!isOpponentHand);
    }

    public void RemoveCard(CardView card) {
        cards.Remove(card);
        UpdateCardsPositions();
    }

    public List<CardView> RemoveAllCards() {
        List<CardView> allCards = cards;
        cards = new List<CardView>();
        return allCards;
    }

    void Update() {
        if (cardPendingPlay) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                CancelPlayingCard();
            }
            if (Input.GetMouseButtonDown(1)) {
                CancelPlayingCard();
            }
            if (Input.GetMouseButtonUp(0) && cardPendingPlayFromDrag) {
                FinishPlayingCard();
            }
            if (Input.GetMouseButtonDown(0) && !cardPendingPlayFromDrag) {
                FinishPlayingCard();
            }
        }
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

    void BeginPlayingCard(CardView cardToPlay, bool fromDrag) {
        foreach (CardView card in cards) {
            card.InteractionMode = 0;
        }

        cardPendingPlayFromDrag = fromDrag;
        cardToPlay.SetTarget(new TransformProps(playedCardPosition));
        cardToPlay.InteractionMode = 0;
        cardPendingPlay = cardToPlay;

        foreach (Target target in allowedTargetsFunc(cardToPlay))
            target.Activate();
    }

    void FinishPlayingCard() {
        if (Target.hoveredTarget != null) {
            CardView card = cardPendingPlay;
            cardPendingPlay = null;

            Target target = Target.hoveredTarget;
            Target.DeactivateAll();

            UpdateCardsPositions();

            cardPlayFunc(card, target);
        }
        else {
            CancelPlayingCard();
        }
    }

    void CancelPlayingCard() {
        cardPendingPlay = null;
        foreach (CardView card in cards) {
            card.InteractionMode = CardViewInteractionMode.Click | CardViewInteractionMode.DragFromHand;
            card.onClick = () => { BeginPlayingCard(card, false); };
            card.onDragOutOfHand = () => { BeginPlayingCard(card, true); };
        }
        Target.DeactivateAll();
        UpdateCardsPositions();
    }

    public void AllowPlayingFromHand(Func<CardView, IEnumerable<Target>> allowedTargetsFunc, Action<CardView, Target> cardPlayFunc) {
        this.allowedTargetsFunc = allowedTargetsFunc;
        this.cardPlayFunc = cardPlayFunc;

        foreach (CardView card in cards) {
            card.InteractionMode = CardViewInteractionMode.Click | CardViewInteractionMode.DragFromHand;
            card.onClick = () => { BeginPlayingCard(card, false); };
            card.onDragOutOfHand = () => { BeginPlayingCard(card, true); };
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
