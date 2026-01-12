using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Forwards specific input actions from the Camera to other GameObjects.
/// This is needed because Player Input uses "Send Messages" behavior,
/// which only sends to scripts on the same GameObject.
/// </summary>
public class InputForwarder : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The ball/player GameObject that has the DashController")]
    public GameObject ballObject;

    private DashController dashController;

    private void Awake()
    {
        // Get the DashController from the ball
        if (ballObject != null)
        {
            dashController = ballObject.GetComponent<DashController>();
            if (dashController == null)
            {
                Debug.LogWarning("InputForwarder: No DashController found on ball object!");
            }
        }
        else
        {
            Debug.LogError("InputForwarder: Ball object not assigned!");
        }
    }

    // Called by Input System when Dash action is triggered
    // This receives the input because Player Input is on the Camera
    public void OnDash(InputValue value)
    {
        // Forward to the DashController on the ball
        if (dashController != null)
        {
            dashController.OnDash(value);
        }
    }
}
