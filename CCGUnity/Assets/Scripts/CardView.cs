using UnityEngine;

class CardView : MonoBehaviour {
    public float speed = 10.0f;

    TransformProps target;

    public void SetTarget(TransformProps props) {
        target = props;
    }

    public void JumpToTarget() {
        Move(1);
    }

    void Update() {
        Move(Time.deltaTime * speed);
    }

    void Move(float t) {
        transform.position = Vector3.Lerp(transform.position, target.position, t);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, t);
        transform.localScale = Vector3.Lerp(transform.localScale, target.scale, t);
    }

}
