using UnityEngine;
using UnityEngine.UIElements;

public class BoostUIManager : MonoBehaviour
{
    [Header("Required References")]
    public PlayerMovement playerMovement;
    public UIDocument uiDocument;
    
    [Header("UI Size & Position")]
    [Tooltip("Width of the entire boost UI")]
    public float containerWidth = 150f;
    [Tooltip("Height of the boost bar")]
    public float barHeight = 20f;
    [Tooltip("Font size of the status text")]
    public int fontSize = 12;
    [Tooltip("Distance from top of screen")]
    public float topPosition = 20f;
    [Tooltip("Distance from right of screen")]
    public float rightPosition = 20f;
    
    [Header("UI Colors")]
    public Color readyColor = Color.green;
    public Color boostingColor = new Color(1f, 0.5f, 0f); // Orange
    public Color rechargingColor = new Color(0f, 0.5f, 1f); // Blue
    
    // Private UI elements
    private VisualElement boostFill;
    private Label boostStatus;
    private VisualElement container;
    
    void Start()
    {
        if (uiDocument == null)
        {
            Debug.LogError("BoostUIManager: No UIDocument assigned!");
            return;
        }
        
        if (playerMovement == null)
        {
            Debug.LogError("BoostUIManager: No PlayerMovement assigned!");
            return;
        }
        
        CreateBoostUI();
        Debug.Log("Boost UI created in top-right corner with size: " + containerWidth + "x" + barHeight);
    }
    
    void CreateBoostUI()
    {
        var root = uiDocument.rootVisualElement;
        
        // Remove any existing boost UI (just in case)
        var existing = root.Q<VisualElement>("BoostContainer");
        if (existing != null)
        {
            Debug.Log("Removing existing boost UI");
            existing.RemoveFromHierarchy();
        }
        
        // ===== CREATE COMPACT BOOST UI =====
        
        // 1. Main Container (top-right corner)
        container = new VisualElement();
        container.name = "BoostContainer";
        container.style.position = Position.Absolute;
        container.style.top = topPosition;
        container.style.right = rightPosition;
        container.style.width = containerWidth;
        
        // 2. Background Bar (black with border)
        var background = new VisualElement();
        background.name = "BoostBackground";
        background.style.width = Length.Percent(100);
        background.style.height = barHeight;
        background.style.backgroundColor = new Color(0, 0, 0, 0.5f); // Semi-transparent black
        
        // Add thin white border
        background.style.borderTopWidth = 1;
        background.style.borderBottomWidth = 1;
        background.style.borderLeftWidth = 1;
        background.style.borderRightWidth = 1;
        background.style.borderTopColor = Color.white;
        background.style.borderBottomColor = Color.white;
        background.style.borderLeftColor = Color.white;
        background.style.borderRightColor = Color.white;
        
        // 3. Fill Bar (shows boost percentage)
        boostFill = new VisualElement();
        boostFill.name = "BoostFill";
        boostFill.style.height = Length.Percent(100);
        boostFill.style.width = Length.Percent(100); // Start at 100%
        boostFill.style.backgroundColor = readyColor;
        
        // 4. Status Text
        boostStatus = new Label("BOOST READY");
        boostStatus.name = "BoostStatus";
        boostStatus.style.color = Color.white;
        boostStatus.style.fontSize = fontSize;
        boostStatus.style.unityTextAlign = TextAnchor.MiddleCenter;
        boostStatus.style.marginTop = 2; // Small gap between bar and text
        boostStatus.style.unityFontStyleAndWeight = FontStyle.Bold;
        
        // ===== BUILD THE UI HIERARCHY =====
        background.Add(boostFill);
        container.Add(background);
        container.Add(boostStatus);
        root.Add(container);
    }
    
    void Update()
    {
        if (playerMovement == null || boostFill == null) return;
        
        // Update the fill width based on current boost amount (0-100%)
        boostFill.style.width = Length.Percent(playerMovement.boostAmount);
        
        // Update colors and text based on boost state
        UpdateUIState();
    }
    
    void UpdateUIState()
    {
        if (playerMovement.isBoosting)
        {
            boostFill.style.backgroundColor = boostingColor;
            boostStatus.text = "BOOSTING!";
        }
        else if (playerMovement.boostAmount < 100f)
        {
            boostFill.style.backgroundColor = rechargingColor;
            boostStatus.text = "RECHARGING";
        }
        else
        {
            boostFill.style.backgroundColor = readyColor;
            boostStatus.text = "BOOST READY";
        }
        
        // Optional: Add warning when almost empty
        if (playerMovement.boostAmount < 20f && playerMovement.boostAmount > 0f && !playerMovement.isBoosting)
        {
            boostStatus.text = "LOW BOOST";
        }
    }
    
    // Optional: Public method to adjust UI position/size at runtime
    public void UpdateUIPosition(float newTop, float newRight)
    {
        if (container != null)
        {
            container.style.top = newTop;
            container.style.right = newRight;
        }
    }
    
    public void UpdateUISize(float newWidth, float newHeight)
    {
        if (container != null)
        {
            container.style.width = newWidth;
            
            var background = container.Q<VisualElement>("BoostBackground");
            if (background != null)
            {
                background.style.height = newHeight;
            }
        }
    }
}