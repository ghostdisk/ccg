using UnityEngine;
using System;
using System.Collections.Generic;
using CCG.Shared;
using CCG.Client;
using UnityEngine.UI;
using TMPro;

public class BlindStageView : MonoBehaviour {
    public GameView G;

    [Header("UI")]
    [SerializeField] GameObject uiRoot;
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] Button doneButton;

    [Header("Misc")]
    [SerializeField] ParticleSystem fog;

    public Action<CardView, CardLocation> PlaceCardCallback;
    public Func<CardView, bool, IEnumerable<CardLocation>> GetAllowedFieldsCallback; // args: card, allowHand
    public Action DoneCallback;

    bool final = false;

    public void Init(GameView G) {
        this.G = G;
    }

    public void Activate(
        Action<CardView, CardLocation> placeCardCallback,
        Func<CardView, bool, IEnumerable<CardLocation>> getAllowedFieldsCallback,
        Action doneCallback,
        int remaining)
    {
        PlaceCardCallback = placeCardCallback;
        GetAllowedFieldsCallback = getAllowedFieldsCallback;
        DoneCallback = doneCallback;

        uiRoot.SetActive(true);
        UpdateUI(remaining);

        doneButton.onClick.RemoveAllListeners();
        doneButton.onClick.AddListener(() => DoneCallback());
    }

    public void MakeFinal() {
        final = true;
        UpdateUI(0);
    }

    public void Deactivate() {
        uiRoot.SetActive(false);
        fog.Stop();
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

    public void UpdateUI(int remaining) {
        if (final) {
            infoText.text = $"Waiting for opponent...";
            doneButton.gameObject.SetActive(false);
        }
        else {
            infoText.text = $"You can play {remaining} more cards.";
        }
    }
}
