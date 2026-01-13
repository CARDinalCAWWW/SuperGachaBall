using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DashUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The main canvas for dash UI")]
    public Canvas dashCanvas;
    [Tooltip("The power meter fill image")]
    public Image powerMeterFill;
    [Tooltip("Background panel for the power meter")]
    public Image powerMeterBackground;
    [Tooltip("Text showing power percentage (optional)")]
    public TextMeshProUGUI powerText;

    [Header("Visual Settings")]
    [Tooltip("Color for weak power (0-30%) - Neon Red")]
    public Color weakColor = new Color(1f, 0.1f, 0.1f); // Bright neon red
    [Tooltip("Color for good power (30-70%) - Neon Yellow")]
    public Color goodColor = new Color(1f, 0.95f, 0.1f); // Bright neon yellow
    [Tooltip("Color for optimal power (70-100%) - Neon Green")]
    public Color optimalColor = new Color(0.1f, 1f, 0.3f); // Bright neon green
    [Tooltip("Cooldown start color (gray)")]
    public Color cooldownStartColor = new Color(0.4f, 0.4f, 0.45f); // Lighter gray
    [Tooltip("Cooldown mid color (dark muted blue)")]
    public Color cooldownMidColor = new Color(0.3f, 0.5f, 0.9f); // Brighter blue
    [Tooltip("Cooldown end color (light-dark cyan)")]
    public Color cooldownEndColor = new Color(0.2f, 0.9f, 1f); // Bright cyan
    [Tooltip("Outline thickness as percentage of meter height (0.05 = 5%)")]
    public float outlineThicknessPercent = 0.05f;
    [Tooltip("Fade in/out duration")]
    public float fadeDuration = 0.2f;

    private CanvasGroup canvasGroup;
    private RectTransform fillRectTransform;
    private Outline fillOutline;
    private Shadow fillShadow;
    private float maxFillWidth;
    private bool isVisible = false;

    private void Awake()
    {
        // Get or add canvas group for fading
        canvasGroup = dashCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = dashCanvas.gameObject.AddComponent<CanvasGroup>();
        }

        // Get fill bar rect transform and outline
        if (powerMeterFill != null)
        {
            fillRectTransform = powerMeterFill.GetComponent<RectTransform>();
            
            // Get or add Outline component for glow effect
            fillOutline = powerMeterFill.GetComponent<Outline>();
            if (fillOutline == null)
            {
                fillOutline = powerMeterFill.gameObject.AddComponent<Outline>();
            }
            
            // Get or add Shadow component for depth
            fillShadow = powerMeterFill.GetComponent<Shadow>();
            if (fillShadow == null)
            {
                fillShadow = powerMeterFill.gameObject.AddComponent<Shadow>();
                fillShadow.effectColor = new Color(0, 0, 0, 0.5f); // Semi-transparent black
            }
            
            // Calculate outline/shadow thickness based on meter height
            UpdateOutlineThickness();
            
            // Store the max width from the background
            if (powerMeterBackground != null)
            {
                maxFillWidth = powerMeterBackground.GetComponent<RectTransform>().rect.width;
            }
            else
            {
                maxFillWidth = fillRectTransform.rect.width;
            }
        }

        // Always visible - start in ready state
        dashCanvas.gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        isVisible = true;
        ShowReady();
    }

    private void Update()
    {
        // Fade logic removed - UI is now always visible
    }

    public void ShowPowerMeter()
    {
        // UI is always visible, no need to show/hide
    }

    public void HidePowerMeter()
    {
        // UI is always visible, no need to hide
    }
    
    // Helper method to create a lighter version of a color for the outline (neon glow)
    private Color GetLighterColor(Color baseColor)
    {
        // Increase brightness significantly for neon glow effect
        return Color.Lerp(baseColor, Color.white, 0.6f);
    }
    
    // Helper method to update both fill and outline colors
    private void SetFillColor(Color color)
    {
        if (powerMeterFill != null)
        {
            powerMeterFill.color = color;
        }
        
        if (fillOutline != null)
        {
            fillOutline.effectColor = GetLighterColor(color);
        }
    }
    
    // Update outline thickness based on current meter size
    private void UpdateOutlineThickness()
    {
        if (fillRectTransform != null)
        {
            // Calculate thickness as percentage of meter height
            float meterHeight = fillRectTransform.rect.height;
            float thickness = meterHeight * outlineThicknessPercent;
            
            // Apply to outline (positive X for right, negative Y for down)
            if (fillOutline != null)
            {
                fillOutline.effectDistance = new Vector2(thickness, -thickness);
            }
            
            // Apply to shadow (slightly larger offset for depth)
            if (fillShadow != null)
            {
                fillShadow.effectDistance = new Vector2(thickness * 1.5f, -thickness * 1.5f);
            }
        }
    }
    
    public void ShowCooldown(float cooldownProgress)
    {
        // Keep UI visible during cooldown
        if (!isVisible)
        {
            dashCanvas.gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            isVisible = true;
        }
        
        // Display cooldown as a "recharging" bar with color gradient
        if (fillRectTransform != null)
        {
            // Progress goes from 0 (just entered cooldown) to 1 (ready to use)
            fillRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxFillWidth * cooldownProgress);
            
            // Smooth color transitions: gray → dark blue → cyan
            Color targetColor;
            if (cooldownProgress < 0.5f)
            {
                // Lerp from gray to dark blue between 0-50%
                float t = cooldownProgress / 0.5f; // Normalize to 0-1
                targetColor = Color.Lerp(cooldownStartColor, cooldownMidColor, t);
            }
            else
            {
                // Lerp from dark blue to cyan between 50-100%
                float t = (cooldownProgress - 0.5f) / 0.5f; // Normalize to 0-1
                targetColor = Color.Lerp(cooldownMidColor, cooldownEndColor, t);
            }
            
            SetFillColor(targetColor);
        }
        
        if (powerText != null)
        {
            powerText.text = "Recharging...";
        }
    }
    
    public void HideCooldown()
    {
        // Show ready state instead of hiding
        ShowReady();
    }
    
    public void ShowReady()
    {
        // Show full bar in cyan to indicate ready state
        if (fillRectTransform != null)
        {
            fillRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxFillWidth);
            SetFillColor(cooldownEndColor); // Cyan (ready)
        }
        
        if (powerText != null)
        {
            powerText.text = "Ready";
        }
    }

    public void UpdatePowerMeter(float powerValue)
    {
        // Clamp power value between 0 and 1
        powerValue = Mathf.Clamp01(powerValue);

        // Update fill width by changing the RectTransform width
        if (fillRectTransform != null)
        {
            fillRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxFillWidth * powerValue);

            // Smooth color transitions using Lerp
            Color targetColor;
            if (powerValue < 0.5f)
            {
                // Lerp from weak (red) to good (yellow) between 0-50%
                float t = powerValue / 0.5f; // Normalize to 0-1
                targetColor = Color.Lerp(weakColor, goodColor, t);
            }
            else
            {
                // Lerp from good (yellow) to optimal (green) between 50-100%
                float t = (powerValue - 0.5f) / 0.5f; // Normalize to 0-1
                targetColor = Color.Lerp(goodColor, optimalColor, t);
            }
            
            SetFillColor(targetColor);
        }

        // Update text if available
        if (powerText != null)
        {
            powerText.text = $"{(powerValue * 100):F0}%";
        }
    }
}
