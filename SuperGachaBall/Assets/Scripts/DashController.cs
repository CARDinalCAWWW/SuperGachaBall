using UnityEngine;
using UnityEngine.InputSystem;

public class DashController : MonoBehaviour
{
    [Header("Dash Settings")]
    [Tooltip("Base force multiplier for dash")]
    public float dashForceMultiplier = 20f;
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
    private Vector3 lockedDashDirection;

    // Called by Input System when Dash action is triggered
    public void OnDash(InputValue value)
    {
        if (value.isPressed)
        {
            HandleDashInput();
        }
    }

    private void Update()
    {
        // Update cooldown timer
        if (currentState == DashState.Cooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                currentState = DashState.Idle;
            }
        }

        // Update power meter
        if (currentState == DashState.ChargingPower)
        {
            powerMeterTimer += Time.deltaTime;
            float powerValue = GetPowerMeterValue();
            
            if (dashUI != null)
            {
                dashUI.UpdatePowerMeter(powerValue);
            }
        }
    }

    private void HandleDashInput()
    {
        switch (currentState)
        {
            case DashState.Idle:
                // Start charging
                StartCharging();
                break;

            case DashState.ChargingPower:
                // Lock in power and execute dash
                ExecuteDash();
                break;

            case DashState.Cooldown:
                // Ignore input during cooldown
                Debug.Log("Dash is on cooldown!");
                break;
        }
    }

    private void StartCharging()
    {
        currentState = DashState.ChargingPower;
        powerMeterTimer = 0f;

        // Lock the dash direction based on current input
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

        // Show UI
        if (dashUI != null)
        {
            dashUI.ShowPowerMeter();
        }
    }

    private void ExecuteDash()
    {
        float powerValue = GetPowerMeterValue();
        
        // Calculate dash force
        float dashPower = powerValue; // 0 to 1
        Vector3 dashForce = lockedDashDirection * dashForceMultiplier * dashPower;

        // Apply dash force to ball
        if (ballPhysics != null)
        {
            ballPhysics.ApplyDashForce(dashForce);
        }

        // Hide UI
        if (dashUI != null)
        {
            dashUI.HidePowerMeter();
        }

        // Enter cooldown
        currentState = DashState.Cooldown;
        cooldownTimer = cooldownDuration;

        Debug.Log($"Dash executed! Power: {powerValue:F2}, Direction: {lockedDashDirection}, Force: {dashForce}");
    }

    private float GetPowerMeterValue()
    {
        // Oscillate between 0 and 1 based on timer
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
