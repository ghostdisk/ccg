using UnityEngine;

class CardView : MonoBehaviour {

    public float speed = 10.0f;

    Vector3 targetPosition;
    Quaternion targetRotation = Quaternion.identity;
    Vector3 targetScale = new Vector3(1, 1, 1);

    public void SetSortingOrder(string layer, int order) {
    }

    public void ResetTarget() {
        SetTarget(transform.position, transform.rotation, transform.localScale);
    }

    public void SetTarget(Vector3 position, Quaternion rotation, Vector3 scale) {
        targetPosition = position;
        targetRotation = rotation;
        targetScale = scale;
    }

    public void JumpToTarget() {
        Move(1);
    }

    void Update() {
        Move(Time.deltaTime * speed);
    }

    void Move(float t) {
        transform.position = Vector3.Lerp(transform.position, targetPosition, t);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, t);
    }

}
