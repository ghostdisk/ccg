using UnityEngine;
using System.Collections.Generic;
using CCG.Shared;

public class Target : MonoBehaviour {

    private static Target hoveredTarget = null;
    private static List<Target> targets = new();

    public CardLocation location;

    [SerializeField] private Color normalColor;
    [SerializeField] private Color hoverColor;

    private Material material;

    public static CardLocation HoverLocation =>
        hoveredTarget ? hoveredTarget.location : CardLocation.None;

    void Awake() {
        targets.Add(this);

        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        material = Instantiate(meshRenderer.material);
        meshRenderer.material = material;
    }

    private void OnMouseEnter() {
        material.SetColor("_BaseColor", hoverColor);
        hoveredTarget = this;
    }

    private void OnMouseExit() {
        if (hoveredTarget == this)
            hoveredTarget = null;

        material.SetColor("_BaseColor", normalColor);
    }

    public void Activate() {
        gameObject.SetActive(true);
        material.SetColor("_BaseColor", normalColor);
    }

    public void Deactivate() {
        gameObject.SetActive(false);
        if (hoveredTarget == this)
            hoveredTarget = null;
    }

    public static void DeactivateAll() {
        foreach (Target target in targets)
            target.Deactivate();
        hoveredTarget = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetClass() {
        targets = new();
        hoveredTarget = null;
    }

}
