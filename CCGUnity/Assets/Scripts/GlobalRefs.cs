using UnityEngine;

public class GlobalRefs : MonoBehaviour {

    public static GlobalRefs Instance;

    public Texture2D[] cardArt;

    void Awake() {
        Instance = this;
    }
}
