using UnityEngine;
using CCG.Shared;
using System.Collections.Generic;

public class Target : MonoBehaviour {

    public static Target hoveredTarget = null;
    private static List<Target> targets = new();

    public Position position;

    [SerializeField] private Color normalColor;
    [SerializeField] private Color hoverColor;

    private Material material;

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
