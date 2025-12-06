using CCG.Shared;
using UnityEngine;

public class DeckView : MonoBehaviour {

    [SerializeField] Transform deckTransform;
    [SerializeField] float fullDeckY;
    [SerializeField] float emptyDeckY;

    int _remainingCards = 0;

    public int RemainingCards {
        set {
            _remainingCards = value;
            UpdateHeight();
        }
        get {
            return _remainingCards;
        }
    }

    public TransformProps GetTransformProps() {
        return new TransformProps(deckTransform.position, Quaternion.identity, deckTransform.localScale);
    }

    void UpdateHeight() {
        Vector3 position = deckTransform.position;
        float t = (float)RemainingCards / (float)(GameRules.DeckSize - 1);
        position.y = Mathf.Lerp(emptyDeckY, fullDeckY, t);
        deckTransform.position = position; 
    }
}
