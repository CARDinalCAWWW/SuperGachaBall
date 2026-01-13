using UnityEngine;
using UnityEngine.InputSystem;

public class DashController : MonoBehaviour
{
    [Header("Dash Settings")]
    [Tooltip("Base force multiplier for dash")]
    public float dashForceMultiplier = 20f;
    [Tooltip("Additional force based on current velocity")]
    public float velocityScaleMultiplier = 1.5f;
    [Tooltip("If true, cancels velocity opposing the dash direction")]
    public bool cancelOpposingVelocity = true;
    [Tooltip("Minimum speed guaranteed in dash direction (0 = no guarantee)")]
    public float minimumDashSpeed = 8f;
    [Tooltip("Cooldown duration in seconds")]
    public float cooldownDuration = 2f;
    [Tooltip("Duration of power meter oscillation (one full cycle)")]
    public float powerMeterCycleDuration = 1.5f;

    [Header("References")]
    [Tooltip("Reference to the ball's physics component")]
    public BallPhysics ballPhysics;
    [Tooltip("Reference to the camera controller")]
    public CameraTiltController cameraController;
    [Tooltip("Reference to the dash UI")]
    public DashUI dashUI;
    [Tooltip("Reference to Player Input for manual action polling")]
    public PlayerInput playerInput;
    [Tooltip("Reference to camera effects (optional)")]
    public DashCameraEffects cameraEffects;
    [Tooltip("Reference to visual effects (optional)")]
    public DashVisualEffects visualEffects;
    [Tooltip("Reference to ghost trail (optional)")]
    public DashGhostTrail ghostTrail;

    // State tracking
    private enum DashState
    {
        Idle,
        ChargingPower,
        Cooldown
    }

    private DashState currentState = DashState.Idle;
    private float cooldownTimer = 0f;
    private float powerMeterTimer = 0f;
    private bool isInitialDrop = false; // Track if we're in the quick drop phase
    private Vector3 lockedDashDirection;
    private InputAction dashAction;

    private void Start()
    {
        // Get the dash action and subscribe to its events
        if (playerInput != null)
        {
            dashAction = playerInput.actions.FindAction("Dash");
            if (dashAction != null)
            {
                // Subscribe to the action's phase events
                dashAction.started += OnDashButtonPress;
                dashAction.canceled += OnDashButtonRelease;
                Debug.Log("Subscribed to Dash action events!");
            }
            else
            {
                Debug.LogError("Dash action not found in PlayerInput!");
            }
        }
        else
        {
            Debug.LogError("PlayerInput reference not assigned!");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        if (dashAction != null)
        {
            dashAction.started -= OnDashButtonPress;
            dashAction.canceled -= OnDashButtonRelease;
        }
    }

    // Event callback for when dash button is pressed (started phase)
    private void OnDashButtonPress(InputAction.CallbackContext context)
    {
        Debug.Log("Dash button PRESSED (started event)");
        HandleDashPress();
    }

    // Event callback for when dash button is released (canceled phase)
    private void OnDashButtonRelease(InputAction.CallbackContext context)
    {
        Debug.Log("Dash button RELEASED (canceled event)");
        HandleDashRelease();
    }

    private void HandleDashPress()
    {
        // Only start charging if we're in Idle state
        if (currentState == DashState.Idle)
        {
            StartCharging();
        }
    }

    private void HandleDashRelease()
    {
        // Only execute dash if we're currently charging
        if (currentState == DashState.ChargingPower)
        {
            ExecuteDash();
        }
    }

    private void Update()
    {
        // Update cooldown timer
        if (currentState == DashState.Cooldown)
        {
            cooldownTimer -= Time.deltaTime;
            
            // Show cooldown progress in UI and light
            if (dashUI != null)
            {
                float cooldownProgress = 1f - (cooldownTimer / cooldownDuration);
                dashUI.ShowCooldown(cooldownProgress);
            }
            
            if (visualEffects != null)
            {
                float cooldownProgress = 1f - (cooldownTimer / cooldownDuration);
                visualEffects.UpdateCooldownLight(cooldownProgress);
            }
            
            if (cooldownTimer <= 0f)
            {
                currentState = DashState.Idle;
                
                // Start ready pulse
                if (dashUI != null)
                {
                    dashUI.HideCooldown();
                }
                
                if (visualEffects != null)
                {
                    visualEffects.StartReadyPulse();
                }
            }
        }

        // Update power meter and direction while charging
        if (currentState == DashState.ChargingPower)
        {
            powerMeterTimer += Time.deltaTime;
            float powerValue = GetPowerMeterValue();
            
            // Continuously update dash direction based on current input
            UpdateDashDirection();
            
            // Update UI
            if (dashUI != null)
            {
                dashUI.UpdatePowerMeter(powerValue);
            }
            
            // Update light intensity to match power
            if (visualEffects != null)
            {
                visualEffects.UpdateChargingPower(powerValue);
            }
            
            // Update ghost trail preview
            if (ghostTrail != null && ballPhysics != null)
            {
                Vector3 dashForce = lockedDashDirection * dashForceMultiplier * powerValue;
                ghostTrail.ShowTrail(ballPhysics.transform.position, dashForce, powerValue);
            }
        }
    }

    private void StartCharging()
    {
        currentState = DashState.ChargingPower;
        powerMeterTimer = 0f;
        isInitialDrop = false; // Skip initial drop, start oscillating immediately

        // Initialize dash direction
        UpdateDashDirection();

        // Start visual effects
        if (visualEffects != null)
        {
            visualEffects.StartCharging();
        }

        // Show UI and start oscillating
        if (dashUI != null)
        {
            dashUI.ShowPowerMeter();
            dashUI.UpdatePowerMeter(1f); // Start at 100%
        }
    }

    private void UpdateDashDirection()
    {
        // Update dash direction based on current input
        if (cameraController != null)
        {
            lockedDashDirection = cameraController.GetForceDirection();
            
            // If no input, dash forward
            if (lockedDashDirection.magnitude < 0.1f)
            {
                lockedDashDirection = Vector3.forward;
            }
            else
            {
                lockedDashDirection.Normalize();
            }
        }
        else
        {
            lockedDashDirection = Vector3.forward;
        }
    }

    private void ExecuteDash()
    {
        float powerValue = GetPowerMeterValue();
        
        if (ballPhysics == null)
        {
            Debug.LogWarning("BallPhysics reference missing!");
            return;
        }
        
        // Calculate dash power
        float dashPower = powerValue; // 0 to 1
        
        // Get current velocity
        Vector3 currentVelocity = ballPhysics.GetVelocity();
        
        // Calculate velocity component in dash direction
        float velocityInDashDirection = Vector3.Dot(currentVelocity, lockedDashDirection);
        
        // Handle opposing velocity
        if (cancelOpposingVelocity && velocityInDashDirection < 0)
        {
            // Cancel the opposing velocity component
            Vector3 opposingVelocity = lockedDashDirection * velocityInDashDirection;
            ballPhysics.AddVelocity(-opposingVelocity);
            
            Debug.Log($"Canceled opposing velocity: {opposingVelocity.magnitude:F2} m/s");
            
            // Reset velocity in dash direction to 0 for calculations
            velocityInDashDirection = 0;
        }
        
        // Calculate base dash force
        Vector3 baseDashForce = lockedDashDirection * dashForceMultiplier * dashPower;
        
        // Add velocity-relative force to make dash effective at high speeds
        Vector3 velocityBoost = Vector3.zero;
        float currentSpeed = currentVelocity.magnitude;
        if (currentSpeed > 0.1f)
        {
            velocityBoost = lockedDashDirection * currentSpeed * velocityScaleMultiplier * dashPower;
        }
        
        Vector3 totalDashForce = baseDashForce + velocityBoost;

        // Apply dash force
        ballPhysics.ApplyDashForce(totalDashForce);
        
        // Stop charging effects, start dash effects
        if (visualEffects != null)
        {
            visualEffects.StopCharging();
            visualEffects.PlayDashTrail();
            visualEffects.StartCooldownLight(); // Start cooldown light fade-in
        }
        
        // Trigger camera effects
        if (cameraController != null)
        {
            cameraController.StartDashZoom();
            cameraController.TriggerShake();
            
            // End zoom after delay
            StartCoroutine(EndDashZoomDelayed(0.5f));
        }
        
        // Hide ghost trail
        if (ghostTrail != null)
        {
            ghostTrail.HideTrail();
        }
        
        // Ensure minimum speed in dash direction if configured
        if (minimumDashSpeed > 0)
        {
            // Wait one physics frame for force to apply, then check speed
            StartCoroutine(EnsureMinimumDashSpeed(minimumDashSpeed * dashPower));
        }

        // Start smooth transition to cooldown
        if (dashUI != null)
        {
            dashUI.StartCooldownTransition(powerValue);
        }

        // Enter cooldown
        currentState = DashState.Cooldown;
        cooldownTimer = cooldownDuration;

        Debug.Log($"Dash executed! Power: {powerValue:F2}, VelInDir: {velocityInDashDirection:F2}, Base: {baseDashForce.magnitude:F2}, Boost: {velocityBoost.magnitude:F2}, Total: {totalDashForce.magnitude:F2}");
    }
    
    private System.Collections.IEnumerator EnsureMinimumDashSpeed(float minSpeed)
    {
        // Wait for physics update
        yield return new WaitForFixedUpdate();
        
        if (ballPhysics != null)
        {
            Vector3 velocity = ballPhysics.GetVelocity();
            float speedInDashDirection = Vector3.Dot(velocity, lockedDashDirection);
            
            // If we're below minimum speed, add velocity to reach it
            if (speedInDashDirection < minSpeed)
            {
                float speedDeficit = minSpeed - speedInDashDirection;
                Vector3 velocityBoost = lockedDashDirection * speedDeficit;
                ballPhysics.AddVelocity(velocityBoost);
                
                Debug.Log($"Minimum speed enforced: added {speedDeficit:F2} m/s to reach {minSpeed:F2} m/s");
            }
        }
    }
    
    private System.Collections.IEnumerator EndDashZoomDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (cameraController != null)
        {
            cameraController.EndDashZoom();
        }
    }

    private float GetPowerMeterValue()
    {
        // Quick drop phase: go from 100% to 0% quickly
        if (isInitialDrop)
        {
            if (dashUI != null)
            {
                float dropDuration = dashUI.chargeDropDuration;
                if (powerMeterTimer < dropDuration)
                {
                    // Lerp from 1.0 to 0.0
                    return Mathf.Lerp(1f, 0f, powerMeterTimer / dropDuration);
                }
                else
                {
                    // Drop complete, switch to normal oscillation
                    isInitialDrop = false;
                    powerMeterTimer = 0f; // Reset for oscillation phase
                }
            }
        }
        
        // Normal oscillation: 0 -> 1 -> 0
        float normalizedTime = (powerMeterTimer % powerMeterCycleDuration) / powerMeterCycleDuration;
        
        // Triangle wave: goes 0 -> 1 -> 0
        float powerValue = normalizedTime < 0.5f 
            ? normalizedTime * 2f  // 0 to 1
            : (1f - normalizedTime) * 2f;  // 1 to 0

        return Mathf.Clamp01(powerValue);
    }

    // Public methods for querying state
    public bool IsOnCooldown()
    {
        return currentState == DashState.Cooldown;
    }

    public float GetCooldownRemaining()
    {
        return currentState == DashState.Cooldown ? cooldownTimer : 0f;
    }
}
