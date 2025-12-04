using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MulliganView))]
public class MulliganViewEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        MulliganView mulliganView = (MulliganView)target;

        if (GUILayout.Button("Show Cards Positions")) {
            ShowCardsPositions(mulliganView);
        }

        if (GUILayout.Button("Hide Cards Positions")) {
            HideCardsPositions(mulliganView);
        }
    }

    private void ShowCardsPositions(MulliganView mulliganView) {
        if (mulliganView.positions == null || mulliganView.positions.Count == 0) {
            Debug.LogWarning("No positions defined in MulliganView.");
            return;
        }

        if (mulliganView.cardViewPrefab == null) {
            Debug.LogWarning("CardView Prefab is not assigned in MulliganView.");
            return;
        }

        ClearTempCards(mulliganView); // Clear any existing temp cards first

        foreach (Transform position in mulliganView.positions)
        {
            if (position != null)
            {
                CardView tempCard = (CardView)PrefabUtility.InstantiatePrefab(mulliganView.cardViewPrefab);
                tempCard.transform.SetParent(position);
                tempCard.transform.localPosition = Vector3.zero;
                tempCard.transform.localRotation = Quaternion.identity;
                tempCard.transform.localScale = Vector3.one;
                tempCard.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
        }
    }

    private void HideCardsPositions(MulliganView mulliganView)
    {
        // Implementation for hiding cards
        ClearTempCards(mulliganView);
    }

    private void ClearTempCards(MulliganView mulliganView)
    {
        // Find and destroy all temporary cards
        foreach (Transform position in mulliganView.positions)
        {
            if (position != null)
            {
                List<GameObject> childrenToDestroy = new List<GameObject>();
                foreach (Transform child in position)
                {
                    if (child.gameObject.hideFlags == HideFlags.HideAndDontSave)
                    {
                        childrenToDestroy.Add(child.gameObject);
                    }
                }
                foreach (GameObject child in childrenToDestroy)
                {
                    DestroyImmediate(child);
                }
            }
        }
    }
}