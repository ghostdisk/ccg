using UnityEngine;

class DeckView : MonoBehaviour {

    [SerializeField] private Transform deckTransform;

    public TransformProps GetTransformProps() {
        return new TransformProps(deckTransform.position, Quaternion.Euler(0, 0, -180), deckTransform.localScale);
    }
}
