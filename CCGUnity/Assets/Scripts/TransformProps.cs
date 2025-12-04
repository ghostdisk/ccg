using UnityEngine;

public struct TransformProps {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public TransformProps(Vector3 position) {
        this.position = position;
        this.rotation = Quaternion.identity;
        this.scale = new Vector3(1,1,1);
    }

    public TransformProps(Vector3 position, Quaternion rotation) {
        this.position = position;
        this.rotation = rotation;
        this.scale = new Vector3(1,1,1);
    }

    public TransformProps(Vector3 position, Quaternion rotation, Vector3 scale) {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }

    public TransformProps(Transform transform) {
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.localScale;
    }
}
