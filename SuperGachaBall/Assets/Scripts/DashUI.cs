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

    [Header("Meter Dimensions")]
    [Tooltip("Width of the dash meter")]
    public float meterWidth = 400f;
    [Tooltip("Height of the dash meter")]
    public float meterHeight = 50f;
    
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
    [Tooltip("Ready state color (bright green)")]
    public Color readyColor = new Color(0f, 1f, 0.2f); // Bright green
    [Tooltip("Outline thickness as percentage of meter height (0.05 = 5%)")]
    public float outlineThicknessPercent = 0.05f;
    [Tooltip("Duration of cyan-to-green transition (seconds)")]
    public float readyTransitionDuration = 0.3f;
    [Tooltip("Duration of initial charge drop (seconds)")]
    public float chargeDropDuration = 0.2f;
    [Tooltip("Fade in/out duration")]
    public float fadeDuration = 0.2f;
    [Tooltip("Duration for ready to charging drop (seconds)")]
    public float readyToChargingDuration = 0.3f;
    [Tooltip("Duration for dash power to zero transition (seconds)")]
    public float dashToZeroDuration = 0.4f;

    private CanvasGroup canvasGroup;
    private RectTransform fillRectTransform;
    private Outline fillOutline;
    private Shadow fillShadow;
    private float maxFillWidth;
    private bool isVisible = false;
    private UnityEngine.Coroutine activeTransition = null;
    private bool isTransitioning = false;

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
            
            // Apply meter dimensions
            ApplyMeterDimensions();
            
            // Calculate outline/shadow thickness based on meter height
            UpdateOutlineThickness();
            
            // Store the max width
            maxFillWidth = meterWidth;
        }

        // Always visible - start in ready state
        dashCanvas.gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        isVisible = true;
        ShowReady();
    }
    
    // Called when inspector values change (for live editing)
    private void OnValidate()
    {
        if (Application.isPlaying && fillRectTransform != null)
        {
            ApplyMeterDimensions();
            UpdateOutlineThickness();
        }
    }

    private void Update()
    {
        // Fade logic removed - UI is now always visible
    }

    public void ShowPowerMeter()
    {
        // UI is always visible, meter will update via UpdatePowerMeter()
    }

    public void HidePowerMeter()
    {
        // UI is always visible, no need to hide
    }
    
    private void StopActiveTransition()
    {
        if (activeTransition != null)
        {
            StopCoroutine(activeTransition);
            activeTransition = null;
        }
    }
    
    private System.Collections.IEnumerator TransitionReadyToCharging()
    {
        float elapsed = 0f;
        
        while (elapsed < readyToChargingDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / readyToChargingDuration;
            
            // Drop from 100% to 0% while transitioning color from green to red
            if (fillRectTransform != null)
            {
                // Lerp width from 100% to 0%
                float currentWidth = Mathf.Lerp(maxFillWidth, 0f, t);
                fillRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
                
                // Lerp color from green (ready) to red (weak/0%)
                Color transitionColor = Color.Lerp(readyColor, weakColor, t);
                SetFillColor(transitionColor);
            }
            
            yield return null;
        }
        
        // Ensure we end at exactly 0%
        if (fillRectTransform != null)
        {
            fillRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
            SetFillColor(weakColor);
        }
        
        activeTransition = null;
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
    
    // Apply meter dimensions to background and fill
    private void ApplyMeterDimensions()
    {
        if (powerMeterBackground != null)
        {
            RectTransform bgRect = powerMeterBackground.GetComponent<RectTransform>();
            bgRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, meterWidth);
            bgRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, meterHeight);
        }
        
        if (fillRectTransform != null)
        {
            fillRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, meterHeight);
        }
    }
    
    // Update outline thickness based on current meter size
    private void UpdateOutlineThickness()
    {
        if (fillRectTransform != null)
        {
            // Calculate thickness as percentage of meter height
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
        // Don't update if we're still transitioning
        if (isTransitioning)
        {
            return;
        }
        
        // Keep UI visible during cooldown
        if (!isVisible)
        {
            dashCanvas.gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            isVisible = true;
        }
        
        StopActiveTransition();
        
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
    
    // New method to transition from dash power to cooldown
    public void StartCooldownTransition(float fromPowerValue)
    {
        StopActiveTransition();
        isTransitioning = true;
        activeTransition = StartCoroutine(TransitionDashToCooldown(fromPowerValue));
    }
    
    private System.Collections.IEnumerator TransitionDashToCooldown(float startPower)
    {
        float elapsed = 0f;
        
        // Get the color at the start power level for charging
        Color startColor;
        if (startPower < 0.5f)
        {
            float t = startPower / 0.5f;
            startColor = Color.Lerp(weakColor, goodColor, t);
        }
        else
        {
            float t = (startPower - 0.5f) / 0.5f;
            startColor = Color.Lerp(goodColor, optimalColor, t);
        }
        
        while (elapsed < dashToZeroDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashToZeroDuration;
            
            // Lerp from start power to 0
            float currentPower = Mathf.Lerp(startPower, 0f, t);
            
            // Lerp color from start charging color to recharging start color (gray)
            Color currentColor = Color.Lerp(startColor, cooldownStartColor, t);
            
            if (fillRectTransform != null)
            {
                fillRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxFillWidth * currentPower);
                SetFillColor(currentColor);
            }
            
            yield return null;
        }
        
        // Ensure we end at 0 with the right color
        if (fillRectTransform != null)
        {
            fillRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
            SetFillColor(cooldownStartColor);
        }
        
        if (powerText != null)
        {
            powerText.text = "Recharging...";
        }
        
        isTransitioning = false;
        activeTransition = null;
    }
    
    public void HideCooldown()
    {
        // Check if object is active before starting coroutine
        if (!gameObject.activeInHierarchy) return;

        // Transition from cyan to green over time
        StartCoroutine(TransitionToReady());
    }
    
    private System.Collections.IEnumerator TransitionToReady()
    {
        float elapsed = 0f;
        
        while (elapsed < readyTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / readyTransitionDuration;
            
            // Lerp from cyan to green
            Color transitionColor = Color.Lerp(cooldownEndColor, readyColor, t);
            SetFillColor(transitionColor);
            
            yield return null;
        }
        
        // Ensure we end at exactly ready state
        ShowReady();
    }
    
    public void ShowReady()
    {
        // Show full bar in green to indicate ready state
        if (fillRectTransform != null)
        {
            fillRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxFillWidth);
            SetFillColor(readyColor); // Bright green (ready to dash!)
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
