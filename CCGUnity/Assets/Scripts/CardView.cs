using System;
using UnityEngine;

[Flags]
public enum CardViewInteractionMode {
    None = 0,
    Click = 1,
    DragFromHand = 2,
};

public class CardView : MonoBehaviour {
    public UnityCard card;
    public Action onClick;

    [SerializeField] float speed = 10.0f;
    [SerializeField] float hoverSpeed = 20.0f;

    [SerializeField] private Transform child;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color emissionColor;
    [SerializeField] private float emissionStrength = 0.0f;

    private TransformProps target;
    private TransformProps childTarget;

    private Material material;
    private Material artMaterial;
    private float currentEmissionStrength = 0.0f;
    private float targetEmissionStrength = 0.0f;

    private bool isHovered = false;
    private bool isMouseDown = false;
    private bool isDragged = false;
    private Vector2 dragOffset;
    private Vector2 mouseDownPos;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private static CardView downCard = null;

    public CardViewInteractionMode InteractionMode {
        get {
            return _interactionMode;
        }
        set {
            _interactionMode = value;
            if ((_interactionMode & CardViewInteractionMode.Click) == 0) {
                onClick = null;
            }
            UpdateHoverState();
        }
    }


    private CardViewInteractionMode _interactionMode;

    void Awake() {
        material = Instantiate(meshRenderer.materials[0]);
        artMaterial = Instantiate(meshRenderer.materials[1]);
        Material[] materials = meshRenderer.materials;
        materials[0] = material;
        materials[1] = artMaterial;
        meshRenderer.materials = materials;

        target = new TransformProps(Vector3.zero);
        childTarget = new TransformProps(Vector3.zero);

        InteractionMode = CardViewInteractionMode.None;

        OnCardUpdate();
    }

    void Update() {
        if (currentEmissionStrength != targetEmissionStrength) {
            currentEmissionStrength = Mathf.Lerp(currentEmissionStrength, targetEmissionStrength, hoverSpeed * Time.deltaTime);

            child.localPosition = Vector3.Lerp(child.localPosition, childTarget.position, hoverSpeed * Time.deltaTime);
            child.localRotation = Quaternion.Lerp(child.localRotation, childTarget.rotation, hoverSpeed * Time.deltaTime);
            child.localScale = Vector3.Lerp(child.localScale, childTarget.scale, hoverSpeed * Time.deltaTime);

            material.SetColor(EmissionColor, emissionColor * currentEmissionStrength);
        }

        if (isMouseDown) {
            if (Input.GetMouseButtonUp(0)) {
                isMouseDown = false;
                downCard = null;

                if (isDragged) {
                }
                else if (isHovered) {
                    if (onClick != null)
                        onClick();
                }
                UpdateHoverState();
            }
        }
        else {
            Move(Time.deltaTime * speed);
        }
    }

    void UpdateHoverState() {
        bool interactive = (downCard == null || downCard == this) && (isHovered || isMouseDown);

        if ((InteractionMode & CardViewInteractionMode.DragFromHand) != 0) {
            if (interactive) {
                childTarget.position = new Vector3(0, 0.25f, 0.3f);
            }
            else {
                childTarget.position = Vector3.zero;
            }
        }

        if (InteractionMode != CardViewInteractionMode.None && interactive) {
            targetEmissionStrength = emissionStrength;
        }
        else {
            // childTarget.scale = new Vector3(1,1,1);
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
        if ((InteractionMode & CardViewInteractionMode.DragFromHand) != 0) {
            isMouseDown = true;
            downCard = this;
            mouseDownPos = Input.mousePosition;
        }
        else if ((InteractionMode & CardViewInteractionMode.Click) != 0) {
            isMouseDown = true;
            downCard = this;
            mouseDownPos = Input.mousePosition;
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

    public void OnCardUpdate() {
        if (card != null) {
            artMaterial.SetTexture("_BaseMap", GlobalRefs.Instance.cardArt[card.prototype.id]);
        }
    }

    void Move(float t) {
        transform.position = Vector3.Lerp(transform.position, target.position, t);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, t);
        transform.localScale = Vector3.Lerp(transform.localScale, target.scale, t);
    }

}
