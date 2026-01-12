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
    [Tooltip("Color for weak power (0-30%)")]
    public Color weakColor = new Color(0.8f, 0.2f, 0.2f); // Red
    [Tooltip("Color for good power (30-70%)")]
    public Color goodColor = new Color(0.9f, 0.8f, 0.2f); // Yellow
    [Tooltip("Color for optimal power (70-100%)")]
    public Color optimalColor = new Color(0.2f, 0.8f, 0.2f); // Green
    [Tooltip("Fade in/out duration")]
    public float fadeDuration = 0.2f;

    private CanvasGroup canvasGroup;
    private RectTransform fillRectTransform;
    private float maxFillWidth;
    private bool isVisible = false;
    private float fadeTimer = 0f;
    private bool isFadingIn = false;

    private void Awake()
    {
        // Get or add canvas group for fading
        canvasGroup = dashCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = dashCanvas.gameObject.AddComponent<CanvasGroup>();
        }

        // Get fill bar rect transform
        if (powerMeterFill != null)
        {
            fillRectTransform = powerMeterFill.GetComponent<RectTransform>();
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

        // Start hidden
        canvasGroup.alpha = 0f;
        dashCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Handle fading
        if (fadeTimer > 0f)
        {
            fadeTimer -= Time.deltaTime;
            float progress = 1f - (fadeTimer / fadeDuration);

            if (isFadingIn)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            }
            else
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
                
                if (fadeTimer <= 0f)
                {
                    dashCanvas.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ShowPowerMeter()
    {
        if (!isVisible)
        {
            dashCanvas.gameObject.SetActive(true);
            isVisible = true;
            isFadingIn = true;
            fadeTimer = fadeDuration;
        }
    }

    public void HidePowerMeter()
    {
        if (isVisible)
        {
            isVisible = false;
            isFadingIn = false;
            fadeTimer = fadeDuration;
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

            // Update color based on power level
            if (powerValue < 0.3f)
            {
                powerMeterFill.color = weakColor;
            }
            else if (powerValue < 0.7f)
            {
                powerMeterFill.color = goodColor;
            }
            else
            {
                powerMeterFill.color = optimalColor;
            }
        }

        // Update text if available
        if (powerText != null)
        {
            powerText.text = $"{(powerValue * 100):F0}%";
        }
    }
}
