using UnityEngine;
using TMPro;
using GameSystems.Time.Managers;
using GameSystems.Time.Models;

namespace GameSystems.Time.UI
{
    /// <summary>
    /// Handles the UI representation of the networked level timer.
    /// Safely reads Fusion networked variables in Unity Update for smooth rendering.
    /// </summary>
    public class TimerUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private CanvasGroup warningEffect; 
        [SerializeField] private Animator timerAnimator; 
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip warningClip;

        [Header("Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color criticalColor = Color.red; 
        
        private float nextBeepTime;
        private bool isWarningActive;

        private void Start()
        {
            if (timeText != null) timeText.color = normalColor;
            if (warningEffect != null) warningEffect.alpha = 0f;
        }

        private void Update()
        {
            if (TimeManager.Instance == null || !TimeManager.Instance.Object.IsValid) return;

            // Only update UI if we are in Playing state
            if (TimeManager.Instance.CurrentStateEnum == LevelState.Playing)
            {
                float remTime = TimeManager.Instance.RemainingTime;

                UpdateText(remTime);
                UpdateVisualWarnings(remTime);
                UpdateAudioWarnings(remTime);
            }
        }

        private void UpdateText(float remainingTime)
        {
            if (timeText == null) return;
            
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        private void UpdateVisualWarnings(float remainingTime)
        {
            // Critical (< 30s)
            // if (remainingTime <= 30f)
            // {
            //     if (timeText != null) timeText.color = criticalColor;
            //     if (warningEffect != null) warningEffect.alpha = 0.5f;
            // }
            // else
            // {
            //     if (timeText != null) timeText.color = normalColor;
            //     if (warningEffect != null) warningEffect.alpha = 0f;
            // }

            // Warning Shake (< 10s)
            if (remainingTime <= 10f && remainingTime > 0)
            {
                if (!isWarningActive && timerAnimator != null)
                {
                    timerAnimator.SetTrigger("Shake");
                    isWarningActive = true;
                }
            }
            else
            {
                isWarningActive = false;
            }
        }

        private void UpdateAudioWarnings(float remainingTime)
        {
            if (remainingTime <= 10f && remainingTime > 0)
            {
                if (UnityEngine.Time.time >= nextBeepTime)
                {
                    if (audioSource != null && warningClip != null)
                    {
                        audioSource.PlayOneShot(warningClip);
                    }
                    nextBeepTime = UnityEngine.Time.time + 1f; // Beep every 1 second
                }
            }
        }
    }
}
