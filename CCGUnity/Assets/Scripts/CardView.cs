using System;
using UnityEngine;

public class CardView : MonoBehaviour {

    public UnityCard card;
    public Action onClick;

    [SerializeField] float speed = 10.0f;
    [SerializeField] float hoverSpeed = 20.0f;

    [SerializeField] Transform child;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Color emissionColor;
    [SerializeField] float emissionStrength = 0.0f;

    private TransformProps target;
    private TransformProps childTarget;

    private Material material;
    private float currentEmissionStrength = 0.0f;
    private float targetEmissionStrength = 0.0f;
    private bool _isInteractive = false;
    private bool isHovered = false;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    public bool Interactive {
        get { return _isInteractive; }
        set { _isInteractive = value; UpdateHoverState(); }
    }

    void Awake() {
        material = Instantiate(meshRenderer.material);
        meshRenderer.material = material;

        target = new TransformProps(Vector3.zero);
        childTarget = new TransformProps(Vector3.zero, Quaternion.Euler(-90, 0, -180));

        Interactive = false;
    }

    void Update() {
        if (currentEmissionStrength != targetEmissionStrength) {
            currentEmissionStrength = Mathf.Lerp(currentEmissionStrength, targetEmissionStrength, hoverSpeed * Time.deltaTime);

            child.localPosition = Vector3.Lerp(child.localPosition, childTarget.position, hoverSpeed * Time.deltaTime);
            child.localRotation = Quaternion.Lerp(child.localRotation, childTarget.rotation, hoverSpeed * Time.deltaTime);
            child.localScale = Vector3.Lerp(child.localScale, childTarget.scale, hoverSpeed * Time.deltaTime);

            material.SetColor(EmissionColor, emissionColor * currentEmissionStrength);
        }
        Move(Time.deltaTime * speed);
    }

    void UpdateHoverState() {
        if (isHovered && _isInteractive) {
            childTarget.scale = new Vector3(1.1f, 1.1f, 1.1f);
            targetEmissionStrength = emissionStrength;
        }
        else {
            childTarget.scale = new Vector3(1,1,1);
            targetEmissionStrength = 0.0f;
        }
    }

    private void OnMouseEnter() {
        isHovered = true;
        UpdateHoverState();
    }

    private void OnMouseExit() {
        isHovered = false;
        UpdateHoverState();
    }

    private void OnMouseDown() {
        if (_isInteractive && onClick != null) {
            onClick();
        }
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
