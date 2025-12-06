using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using System;
using CCG.Shared;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
#endif


public class HandView : MonoBehaviour {

    [SerializeField] public Target target;
    [SerializeField] SplineContainer splineContainer;
    [SerializeField] List<float> spacings;
    [SerializeField] List<float> cardRotation;
    [SerializeField] float maxUseSpace = 1.0f;
    [SerializeField] bool isOpponentHand = false;

    List<CardView> cards = new();
    CardView pressedCard;
    Vector3 mouseDownPos;
    Action<CardView, bool> cardPlayCallback; // args: card, fromDrag
    bool _isInteractive;


    enum PlayCardState {
        None,
        CardHeldDown,
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


    public void AddCard(CardView card) {
        if (card.indexInHand < 0) {
            throw new Exception("CardView.indexInHand must be set before calling HandView.AddCard.");
        }

        cards.Add(card);
        AttachCardCallbacks(card);

        UpdateCardsPositions();
        card.ToggleShadowCast(!isOpponentHand);
    }

    public void RemoveCard(CardView card, bool keepSpot) {
        int index = cards.IndexOf(card);
        if (index >= 0) {
            card.ClearCallbacks();

            if (keepSpot) cards[index] = null;
            else cards.Remove(card);

            UpdateCardsPositions();
        }
    }

    public List<CardView> RemoveAllCards() {
        foreach (CardView card in cards)
            card.ClearCallbacks();

        List<CardView> allCards = cards;
        cards = new List<CardView>();
        return allCards;
    }

    void Update() {

        if (pressedCard) {
            float zOffset = 0.35f;
            Vector3 mouse = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5f));
            float add = (mouse.z - mouseDownPos.z) * 0.8f;
            zOffset += add;
            pressedCard.childTarget.position = new Vector3(0, 0.25f, zOffset);
            if (add > 0.3) {
                BeginPlayingCard(true);
            }
        }
    }

    TransformProps[] GetCardTransformProps() {
        int cardsCount = cards.Max(card => card ? card.indexInHand : -1) + 1;
        TransformProps[] props = new TransformProps[cards.Count];

        if (cardsCount > 0) {
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
        foreach (CardView card in cards)
            card?.SetTarget(props[card.indexInHand]);
    }

    public void AllowPlayingFromHand(Action<CardView, bool> cardPlayCallback) {
        IsInteractive = true;
        this.cardPlayCallback = cardPlayCallback;
    }

    void AttachCardCallbacks(CardView card) {
        card.IsInteractive = true;

        card.OnClickBegin = () => {
            IsInteractive = false;
            pressedCard = card;
            mouseDownPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5f));
        };

        card.OnClickEnd = (bool wasDown, bool isHover) => {
            if (pressedCard && isHover)
                BeginPlayingCard(false);
        };

        card.OnHoverChanged = (bool hover) => {
            card.childTarget.position = hover && IsInteractive ? new Vector3(0, 0.25f, 0.35f) : Vector3.zero;
        };
    }

    void BeginPlayingCard(bool fromDrag) {
        pressedCard.childTarget.position = Vector3.zero;

        CardView playedCard = pressedCard;
        pressedCard = null;

        cardPlayCallback(playedCard, fromDrag);
    }

    void OnIsInteractiveChanged() {
        foreach (CardView card in cards)
            if (card)
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
