using UnityEngine;
using System.Collections.Generic;

public class PlayerDetectionZone : MonoBehaviour
{
    [Header("Activation Settings")]
    [SerializeField] private KeyCode activationKey = KeyCode.E;
    [SerializeField] private float activationRadius = 5f;

    [Header("Visual Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color inRangeColor = Color.green;
    [SerializeField] private Color outOfRangeColor = Color.gray;

    [Header("UI Settings (Optional)")]
    [SerializeField] private bool showActivationPrompt = true;
    [SerializeField] private string promptText = "Appuyez sur [E] pour alerter l'IA";

    private List<AI_people> aisInRange = new List<AI_people>();
    private GUIStyle guiStyle;

    void Start()
    {
        // Setup GUI style for on-screen prompt
        guiStyle = new GUIStyle();
        guiStyle.fontSize = 20;
        guiStyle.fontStyle = FontStyle.Bold;
        guiStyle.normal.textColor = Color.white;
        guiStyle.alignment = TextAnchor.MiddleCenter;
    }

    void Update()
    {
        // Find all AIs in range
        UpdateAIsInRange();

        // Check for activation key press
        if (Input.GetKeyDown(activationKey) && aisInRange.Count > 0)
        {
            ActivateNearbyAIs();
        }
    }

    void UpdateAIsInRange()
    {
        aisInRange.Clear();

        // Find all colliders in range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, activationRadius);

        foreach (var hitCollider in hitColliders)
        {
            AI_people ai = hitCollider.GetComponent<AI_people>();
            if (ai != null && !ai.IsDead())
            {
                aisInRange.Add(ai);
            }
        }
    }

    void ActivateNearbyAIs()
    {
        int activatedCount = 0;

        foreach (AI_people ai in aisInRange)
        {
            if (ai != null)
            {
                ai.ActivateByPlayer(transform);
                activatedCount++;
            }
        }

        Debug.Log($"Activé {activatedCount} IA(s) !");
    }

    void OnGUI()
    {
        if (!showActivationPrompt) return;

        // Show prompt when AIs are in range
        if (aisInRange.Count > 0)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            GUI.Label(new Rect(screenWidth / 2 - 200, screenHeight - 100, 400, 50),
                      promptText + $" ({aisInRange.Count} IA)", guiStyle);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw activation radius
        Gizmos.color = aisInRange != null && aisInRange.Count > 0 ? inRangeColor : outOfRangeColor;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }

    // Public methods
    public int GetAICountInRange()
    {
        return aisInRange.Count;
    }

    public void SetActivationRadius(float newRadius)
    {
        activationRadius = newRadius;
    }

    public float GetActivationRadius()
    {
        return activationRadius;
    }
}
