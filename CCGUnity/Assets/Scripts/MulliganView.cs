using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class MulliganView : MonoBehaviour {

    [NonSerialized] public List<CardView> cards;
    [NonSerialized] public List<CardView> discardPile;

    [Header("Prefabs")]
    public CardView cardViewPrefab;

    [Header("Positions")]
    [SerializeField] public List<Transform> positions;
    [SerializeField] Transform firstDiscardPosition;

    [Header("UI")]
    [SerializeField] GameObject uiRoot;
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] Button doneButton;

    [Header("Discard Tween")]
    [SerializeField] float discardClimbPerCard = 0.2f;
    [SerializeField] float discardRandomRotation = 45.0f;
    [SerializeField] AnimationCurve discardTweenEase;
    [SerializeField] float discardFlipDuration = 0.3f;
    [SerializeField] float discardTweenDuration = 0.3f;

    [Header("Misc")]
    [SerializeField] ParticleSystem fog;

    public bool IsActive { get; private set; }

    [NonSerialized] public int mulligansRemaining;
    [NonSerialized] public int confirmedMulligansRemaining;

    float discardY = 0;
    Action<CardView> cardSwapAction;

    void Start() {
        uiRoot.SetActive(false);
        fog.Stop();
        IsActive = false;
    }

    public async Task Activate(List<CardView> cards, Action doneButtonAction, Action<CardView> cardSwapAction, int remaining) {
        this.cardSwapAction = cardSwapAction;

        if (!IsActive) {
            IsActive = true;

            doneButton.onClick.RemoveAllListeners();
            doneButton.onClick.AddListener(() => {
                doneButtonAction();
                doneButton.onClick.RemoveAllListeners();
            });

            if (cards.Count > positions.Count) {
                Debug.LogError("There are more cards in hand than mulligan positions set up.\n");
            }
            this.cards = cards;
            this.discardPile = new List<CardView>();

            for (int i = 0; i < cards.Count; i++)
                MakeCardSwappable(cards[i], i);

            fog.Play();

            await Task.WhenAll(cards.Select(async (card, index) => {
                await Task.Delay(index * 75);
                card.SetTarget(new TransformProps(positions[index]), CardViewTweenMode.ExponentialDecay);
            }).ToArray());

            uiRoot.SetActive(true);
            SetRemaining(remaining, remaining);
        }
        else {
            Debug.LogWarning("Calling MulliganView.Deactivate with already active mulligan view");
        }
    }

    public void SetRemaining(int remaining, int confirmedRemaining) {
        if (IsActive) {
            this.mulligansRemaining = remaining;
            this.confirmedMulligansRemaining = confirmedRemaining;

            if (mulligansRemaining > 0) {
                doneButton.gameObject.SetActive(true);
                infoText.text = $"You can swap out {remaining} more cards.";
            }
            else {
                infoText.text = $"Waiting for opponent...";
                doneButton.gameObject.SetActive(false);

                foreach (CardView card in cards) {
                    if (card != null)
                        MakeCardNonSwappable(card);
                }
            }
        }
        else {
            Debug.LogWarning("Calling MulliganView.SetRemaining with inactive mulligan view");
        }
    }

    public List<CardView> Deactivate() {
        if (IsActive) {
            IsActive = false;
            uiRoot.SetActive(false);
            List<CardView> allCards = cards;
            cards = null;

            foreach (CardView card in discardPile) {
                card.gameObject.SetActive(false);
            }
            discardPile = null;

            return allCards;
        }
        else {
            Debug.LogWarning("Calling MulliganView.Deactivate with inactive mulligan view");
            return new List<CardView>();
        }
    }

    public void AddReplacedCard(CardView card) {
        int indexInHand = card.card.location.IndexInHand;

        card.SetTarget(new TransformProps(positions[indexInHand]), discardTweenDuration, (t) => discardTweenEase.Evaluate(t));
        card.Flip(false, discardFlipDuration);

        card.gameObject.SetActive(true);
        cards[indexInHand] = card;

        if (mulligansRemaining > 0)
            MakeCardSwappable(card, indexInHand);
    }

    TransformProps GetNextDiscardTransformProps() {
        TransformProps props = new TransformProps(firstDiscardPosition);
        props.position.y += discardY;
        props.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(-discardRandomRotation / 2.0f, discardRandomRotation / 2.0f), 0);
        discardY += discardClimbPerCard;
        return props;
    }

    void MakeCardSwappable(CardView card, int indexInHand) {
        card.IsInteractive = true;

        card.OnClickBegin = () => {
            cardSwapAction(card);

            card.SetTarget(GetNextDiscardTransformProps(), discardTweenDuration, (t) => discardTweenEase.Evaluate(t));
            card.Flip(true, discardFlipDuration);
            discardPile.Append(card);
            cards[indexInHand] = null;
            card.IsInteractive = false;
        };
    }

    void MakeCardNonSwappable(CardView card) {
        card.IsInteractive = false;
    }
}
