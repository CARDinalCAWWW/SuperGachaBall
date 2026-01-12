using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public float startTime = 61f;

    private float currentTime;
    private bool timerRunning = true;

    public TextMeshProUGUI timerText;
     void Start()
    {
        currentTime = startTime;
            UpdateTimerText();
    }

    void Update()
    {
        if (!timerRunning) return;
        
        if(currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            currentTime = Mathf.Clamp(currentTime, 0, startTime);
            UpdateTimerText();
        }
        else
        {
            timerRunning = false;
            OnTimerEnd();
        }
    }

    void UpdateTimerText()
    {
        int seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = $"{seconds:00}";
    }

    void OnTimerEnd()
    {
        timerText.text = "00";
        Debug.Log("Timer Done");
    }











}
