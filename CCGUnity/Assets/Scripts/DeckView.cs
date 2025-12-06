using UnityEngine;

public class DeckView : MonoBehaviour {

    [SerializeField] private Transform deckTransform;

    public TransformProps GetTransformProps() {
        return new TransformProps(deckTransform.position, Quaternion.identity, deckTransform.localScale);
    }
}
