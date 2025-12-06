using UnityEngine;
using System;
using System.Collections.Generic;
using CCG.Shared;
using CCG.Client;

public class BlindStageView : MonoBehaviour {
    public GameView G;

    public Action<CardView, CardLocation> PlaceCardCallback;
    public Func<CardView, bool, IEnumerable<CardLocation>> GetAllowedFieldsCallback; // args: card, allowHand

    public void Init(GameView G) {
        this.G = G;
    }

    public void Activate(Action<CardView, CardLocation> placeCardCallback, Func<CardView, bool, IEnumerable<CardLocation>> getAllowedFieldsCallback) {
        PlaceCardCallback = placeCardCallback;
        GetAllowedFieldsCallback = getAllowedFieldsCallback;
    }

    public void BeginPlayingCard(CardView card, bool fromDrag, bool allowHand) {
        CardLocation returnLocation = card.card.location;

        PlaceCardCallback(card, CardLocation.None);
        IEnumerable<CardLocation> targets = GetAllowedFieldsCallback(card, allowHand);

        G.playCardView.Activate(card, fromDrag, targets, (CardView cardView, CardLocation location) => {
            if (location.type == CardLocationType.None)
                location = returnLocation;

            Target.DeactivateAll();
            G.myViews.hand.IsInteractive = true;

            cardView.ClearCallbacks();

            PlaceCardCallback(cardView, location);

            if (location.type == CardLocationType.Board) {
                cardView.IsInteractive = true;
                cardView.OnClickBegin = () => {
                    BeginPlayingCard(card, true, true);
                };
            }
            else {
                // if new location is in the hand, HandView is responsible for initiating the card play.
            }
        });
    }

}
