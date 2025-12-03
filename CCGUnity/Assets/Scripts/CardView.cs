using UnityEngine;

class CardView : MonoBehaviour {
    public float speed = 10.0f;
    public UnityCard card;

    TransformProps target;
    MeshRenderer meshRenderer;

    void Awake() {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    void Update() {
        Move(Time.deltaTime * speed);
    }

    public void SetTarget(TransformProps props) {
        target = props;
    }

    public void JumpToTarget() {
        Move(1);
    }

    public void ToggleShadowCast(bool cast) {
        meshRenderer.shadowCastingMode = cast ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    void Move(float t) {
        transform.position = Vector3.Lerp(transform.position, target.position, t);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, t);
        transform.localScale = Vector3.Lerp(transform.localScale, target.scale, t);
    }

}
