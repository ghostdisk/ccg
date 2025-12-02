using UnityEngine;

class DeckView : MonoBehaviour {

    public Transform deckTransform;

    public CardView cardViewPrefab;

    public CardView CreateCardView() {
        CardView card = Instantiate(cardViewPrefab);

        card.SetTarget(deckTransform.position, Quaternion.Euler(0, 0, -180), deckTransform.localScale);
        card.JumpToTarget();

        return card;
    }

}
