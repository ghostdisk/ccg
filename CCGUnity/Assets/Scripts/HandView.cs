using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using System;
using CCG.Shared;


#if UNITY_EDITOR
using UnityEditor;
#endif


public class HandView : MonoBehaviour {

    public Transform playedCardPosition;

    List<CardView> cards = new();

    [SerializeField] SplineContainer splineContainer;
    [SerializeField] Target handTarget;
    [SerializeField] List<float> spacings;
    [SerializeField] List<float> cardRotation;
    [SerializeField] float maxUseSpace = 1.0f;
    [SerializeField] bool isOpponentHand = false;


    enum PlayCardState {
        None,
        CardHeldDown,
        CardBeingPlayed,
    };

    public bool IsInteractive {
        get {
            return _isInteractive;
        }
        set {
            _isInteractive = value;
            OnIsInteractiveChanged();
        }
    }
    bool _isInteractive;


    PlayCardState playCardState = PlayCardState.None;
    CardView cardPendingPlay;
    Vector3 mouseDownPos;
    bool cardPendingPlayFromDrag;
    Func<CardView, IEnumerable<Target>> getAllowedTargetsCallback;
    Action<CardView, Target> cardPlayCallback;


    public void AddCard(CardView card) {
        cards.Add(card);
        UpdateCardsPositions();
        card.ToggleShadowCast(!isOpponentHand);
    }

    public void RemoveCard(CardView card) {
        card.ClearCallbacks();
        cards.Remove(card);
        UpdateCardsPositions();
    }

    public List<CardView> RemoveAllCards() {
        foreach (CardView card in cards)
            card.ClearCallbacks();

        List<CardView> allCards = cards;
        cards = new List<CardView>();
        return allCards;
    }

    void Update() {
        switch (playCardState) {
            case PlayCardState.CardHeldDown:
                float zOffset = 0.35f;
                Vector3 mouse = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5f));
                float add = (mouse.z - mouseDownPos.z) * 0.8f;
                zOffset += add;
                cardPendingPlay.childTarget.position = new Vector3(0, 0.25f, zOffset);
                if (add > 0.3) {
                    BeginPlayingCard(true);
                }
                break;
            case PlayCardState.CardBeingPlayed:
                if (Input.GetKeyDown(KeyCode.Escape)) {
                    CancelPlayingCard();
                }
                if (Input.GetMouseButtonDown(1)) {
                    CancelPlayingCard();
                }

                bool finish = cardPendingPlayFromDrag ? Input.GetMouseButtonUp(0) : Input.GetMouseButtonDown(0);
                if (finish) {
                    if (Target.hoveredTarget != null) {
                        FinishPlayingCard(Target.hoveredTarget);
                    }
                    else {
                        CancelPlayingCard();
                    }
                }
                break;
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

    public void AllowPlayingFromHand(Func<CardView, IEnumerable<Target>> getAllowedTargetsCallback, Action<CardView, Target> cardPlayCallback) {
        IsInteractive = true;
        this.getAllowedTargetsCallback = getAllowedTargetsCallback;
        this.cardPlayCallback = cardPlayCallback;

        foreach (CardView card in cards) {
            card.IsInteractive = true;

            card.OnClickBegin = () => {
                IsInteractive = false;
                cardPendingPlay = card;
                mouseDownPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5f));
                playCardState = PlayCardState.CardHeldDown;
            };

            card.OnClickEnd = (bool wasDown, bool isHover) => {
                if (playCardState == PlayCardState.CardHeldDown && isHover)
                    BeginPlayingCard(false);
            };

            card.OnHoverChanged = (bool hover) => {
                card.childTarget.position = hover && IsInteractive ? new Vector3(0, 0.25f, 0.35f) : Vector3.zero;
            };
        }
    }

    void BeginPlayingCard(bool fromDrag) {
        cardPendingPlayFromDrag = fromDrag;
        cardPendingPlay.SetTarget(new TransformProps(playedCardPosition));
        cardPendingPlay.childTarget.position = Vector3.zero;
        playCardState = PlayCardState.CardBeingPlayed;

        foreach (Target target in getAllowedTargetsCallback(cardPendingPlay))
            target.Activate();
    }

    void FinishPlayingCard(Target target) {
        CardView card = cardPendingPlay;
        cardPendingPlay = null;

        Target.DeactivateAll();
        UpdateCardsPositions();

        cardPlayCallback(card, target);
    }

    void CancelPlayingCard() {
        IsInteractive = true;
        playCardState = PlayCardState.None;
        if (cardPendingPlay) {
            cardPendingPlay.childTarget.position = Vector3.zero;
            cardPendingPlay = null;
        }
        Target.DeactivateAll();
        UpdateCardsPositions();
    }

    void OnIsInteractiveChanged() {
        foreach (CardView card in cards)
            card.IsInteractive = IsInteractive;
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
