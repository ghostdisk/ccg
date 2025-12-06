using System;
using UnityEngine;
using CCG.Shared;
using TMPro;

public enum CardViewTweenMode {
    None,
    Instant,
    FixedDuration,
    ExponentialDecay,
};

public class CardView : MonoBehaviour {
    public UnityCard card;

    public Action OnClickBegin;
    public Action<bool, bool> OnClickEnd; // args: wasDown, isHover
    public Action<bool> OnHoverChanged; // args: isHover
    [NonSerialized] public int savedIndexInHand = -1;

    [SerializeField] float speed = 10.0f;
    [SerializeField] float hoverSpeed = 20.0f;

    [SerializeField] Transform child;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Color emissionColor;
    [SerializeField] float emissionStrength = 0.0f;
    [SerializeField] Animation cardAnimation;
    [SerializeField] TextMeshPro strengthText;

    TransformProps target;
    public TransformProps childTarget;

    Material material;
    Material artMaterial;
    float currentEmissionStrength = 0.0f;
    float targetEmissionStrength = 0.0f;
    bool isHovered = false;
    bool isMouseDown = false;
    CardViewTweenMode tweenMode;
    bool faceDown = true;

    // fixed duration tween:
    TransformProps tweenStartProps;
    float tweenDuration;
    float tweenTime;
    Func<float, float> tweenEase;

    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    public bool IsInteractive {
        get {
            return _isInteractive;
        }
        set {
            _isInteractive = value;
            UpdateHoverState();
        }
    }


    bool _isInteractive = false;

    void Awake() {
        material = Instantiate(meshRenderer.materials[0]);
        artMaterial = Instantiate(meshRenderer.materials[1]);
        Material[] materials = meshRenderer.materials;
        materials[0] = material;
        materials[1] = artMaterial;
        meshRenderer.materials = materials;

        target = new TransformProps(Vector3.zero);
        childTarget = new TransformProps(Vector3.zero);
        IsInteractive = false;
    }

    void Update() {
        // TODO: Fix the tweening situation
        currentEmissionStrength = Mathf.Lerp(currentEmissionStrength, targetEmissionStrength, hoverSpeed * Time.deltaTime);
        child.localPosition = Vector3.Lerp(child.localPosition, childTarget.position, hoverSpeed * Time.deltaTime);
        child.localRotation = Quaternion.Lerp(child.localRotation, childTarget.rotation, hoverSpeed * Time.deltaTime);
        child.localScale = Vector3.Lerp(child.localScale, childTarget.scale, hoverSpeed * Time.deltaTime);
        material.SetColor(EmissionColor, emissionColor * currentEmissionStrength);

        if (tweenMode == CardViewTweenMode.ExponentialDecay) {
            Move(Time.deltaTime * speed);
        }
        else if (tweenMode == CardViewTweenMode.FixedDuration) {
            tweenTime += Time.deltaTime;
            float t = tweenTime / tweenDuration;
            if (t > 1) {
                t = 1;
                tweenMode = CardViewTweenMode.None;
            }
            t = tweenEase(t);
            transform.position = Vector3.Lerp(tweenStartProps.position, target.position, t);
            transform.rotation = Quaternion.Lerp(tweenStartProps.rotation, target.rotation, t);
            transform.localScale = Vector3.Lerp(tweenStartProps.scale, target.scale, t);
        }

        if (OnClickEnd != null && (isMouseDown || isHovered)) {
            if (Input.GetMouseButtonUp(0)) {
                OnClickEnd(isMouseDown, isHovered);
                isMouseDown = false;
            }
        }
    }

    void UpdateHoverState() {
        if (IsInteractive && (isHovered || isMouseDown)) {
            targetEmissionStrength = emissionStrength;
        }
        else {
            targetEmissionStrength = 0.0f;
        }
    }

    void OnMouseEnter() {
        isHovered = true;
        if (IsInteractive && OnHoverChanged != null) OnHoverChanged(true);
        UpdateHoverState();
    }

    void OnMouseExit() {
        isHovered = false;
        if (IsInteractive && OnHoverChanged != null) OnHoverChanged(false);
        UpdateHoverState();
    }

    void OnMouseDown() {
        if (IsInteractive) {
            isMouseDown = true;
            if (OnClickBegin != null) OnClickBegin();
        }
    }

    public void SetTarget(TransformProps props, CardViewTweenMode tweenMode) {
        target = props;
        this.tweenMode = tweenMode;
    }

    public void SetTarget(TransformProps props, float duration, Func<float,float> ease) {
        tweenMode = CardViewTweenMode.FixedDuration;
        target = props;
        tweenStartProps = new TransformProps(transform);
        tweenTime = 0.0f;
        tweenDuration = duration;
        tweenEase = ease;
    }

    public void JumpToTarget() {
        Move(1);
    }

    public void ToggleShadowCast(bool cast) {
        meshRenderer.shadowCastingMode = cast ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public void UpdateInfo() {
        artMaterial.mainTexture = GlobalRefs.Instance.cardArt[card.prototype.id];
        strengthText.text = card.strength.ToString();
    }

    public void Flip(bool faceDown, float duration) {
        if (faceDown != this.faceDown) {
            this.faceDown = faceDown;
            string name = faceDown ? "anim_card_flip" : "anim_card_unflip";
            cardAnimation.Play(name);
            cardAnimation[name].speed = 1.0f / duration;
        }
    }

    void Move(float t) {
        transform.position = Vector3.Lerp(transform.position, target.position, t);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, t);
        transform.localScale = Vector3.Lerp(transform.localScale, target.scale, t);
    }

    public void ClearCallbacks() {
        IsInteractive = false;
        OnClickBegin = null;
        OnClickEnd = null;
        OnHoverChanged = null;
    }

}
