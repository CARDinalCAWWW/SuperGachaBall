using UnityEngine;

/// <summary>
/// Handles visual effects for the dash ability (particles, glow, etc.)
/// </summary>
public class DashVisualEffects : MonoBehaviour
{
    [Header("Dash Trail")]
    [Tooltip("Particle system for dash trail")]
    public ParticleSystem dashTrailParticles;
    [Tooltip("Duration to play trail particles")]
    public float trailDuration = 0.5f;

    [Header("Charging Glow")]
    [Tooltip("Light component for charging glow")]
    public Light chargingLight;
    [Tooltip("Maximum light intensity")]
    public float maxLightIntensity = 3f;
    [Tooltip("Glow color while charging")]
    public Color chargingColor = Color.cyan;
    [Tooltip("Glow color when ready")]
    public Color readyColor = Color.green;
    [Tooltip("Number of ready pulses")]
    public int readyPulseCount = 3;
    [Tooltip("Speed of ready pulses")]
    public float readyPulseSpeed = 4f;

    [Header("References")]
    [Tooltip("Ball's rigidbody to get velocity direction")]
    public Rigidbody ballRigidbody;

    private enum LightState
    {
        Off,
        Charging,
        Cooldown,
        ReadyPulse
    }

    private LightState lightState = LightState.Off;
    private float currentPowerValue = 0f;
    private float pulseTimer = 0f;
    private int pulsesCompleted = 0;

    private void Start()
    {
        // Setup charging light
        if (chargingLight != null)
        {
            chargingLight.color = chargingColor;
            chargingLight.enabled = false;
        }

        // Stop trail particles initially
        if (dashTrailParticles != null)
        {
            dashTrailParticles.Stop();
            
            // Ensure particle system is in world space
            var main = dashTrailParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
        }
    }

    private void Update()
    {
        if (chargingLight == null) return;

        switch (lightState)
        {
            case LightState.Charging:
                // Intensity matches power percentage
                chargingLight.intensity = currentPowerValue * maxLightIntensity;
                break;

            case LightState.ReadyPulse:
                // Pulse animation
                pulseTimer += Time.deltaTime;
                float pulse = Mathf.Abs(Mathf.Sin(pulseTimer * readyPulseSpeed));
                chargingLight.intensity = pulse * maxLightIntensity;

                // Count pulses
                if (pulseTimer >= (Mathf.PI / readyPulseSpeed))
                {
                    pulseTimer = 0f;
                    pulsesCompleted++;

                    if (pulsesCompleted >= readyPulseCount)
                    {
                        // Done pulsing, turn off
                        lightState = LightState.Off;
                        chargingLight.enabled = false;
                    }
                }
                break;
        }
    }

    private void LateUpdate()
    {
        // Orient particle system opposite to velocity for proper trail
        if (dashTrailParticles != null && dashTrailParticles.isPlaying && ballRigidbody != null)
        {
            Vector3 velocity = ballRigidbody.linearVelocity;
            if (velocity.magnitude > 0.1f)
            {
                // Point particles opposite to movement direction
                Quaternion rotation = Quaternion.LookRotation(-velocity.normalized);
                dashTrailParticles.transform.rotation = rotation;
            }
        }
    }

    /// <summary>
    /// Start charging effects
    /// </summary>
    public void StartCharging()
    {
        lightState = LightState.Charging;
        currentPowerValue = 1f;

        if (chargingLight != null)
        {
            chargingLight.color = chargingColor;
            chargingLight.enabled = true;
        }
    }

    /// <summary>
    /// Update charging light intensity based on power
    /// </summary>
    public void UpdateChargingPower(float powerValue)
    {
        currentPowerValue = powerValue;
    }

    /// <summary>
    /// Stop charging effects
    /// </summary>
    public void StopCharging()
    {
        lightState = LightState.Off;

        if (chargingLight != null)
        {
            chargingLight.enabled = false;
        }
    }

    /// <summary>
    /// Start cooldown light animation
    /// </summary>
    public void StartCooldownLight()
    {
        lightState = LightState.Cooldown;
        
        if (chargingLight != null)
        {
            chargingLight.color = chargingColor;
            chargingLight.enabled = true;
            chargingLight.intensity = 0f;
        }
    }

    /// <summary>
    /// Update cooldown light (fades from 0 to max)
    /// </summary>
    public void UpdateCooldownLight(float cooldownProgress)
    {
        if (lightState == LightState.Cooldown && chargingLight != null)
        {
            chargingLight.intensity = cooldownProgress * maxLightIntensity;
        }
    }

    /// <summary>
    /// Start ready pulse sequence
    /// </summary>
    public void StartReadyPulse()
    {
        lightState = LightState.ReadyPulse;
        pulseTimer = 0f;
        pulsesCompleted = 0;

        if (chargingLight != null)
        {
            chargingLight.color = readyColor;
            chargingLight.enabled = true;
        }
    }

    /// <summary>
    /// Play dash trail effect
    /// </summary>
    public void PlayDashTrail()
    {
        if (dashTrailParticles != null)
        {
            dashTrailParticles.Play();
            StartCoroutine(StopTrailAfterDelay());
        }
    }

    private System.Collections.IEnumerator StopTrailAfterDelay()
    {
        yield return new WaitForSeconds(trailDuration);
        
        if (dashTrailParticles != null)
        {
            dashTrailParticles.Stop();
        }
    }
}
