using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using GameSystems.Time.Managers;
using GameSystems.Time.Models;

namespace GameSystems.Time.UI
{
    /// <summary>
    /// Displays final level statistics, listening to Fusion state changes.
    /// </summary>
    public class ResultScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI remainingTimeText;
        [SerializeField] private TextMeshProUGUI totalTimeText;
        [SerializeField] private TextMeshProUGUI completionPercentageText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        
        [Header("Stars")]
        [SerializeField] private GameObject[] starIcons;

        [Header("Buttons")]
        [SerializeField] private UnityEngine.UI.Button exitButton;

        private void Start()
        {
            if (resultPanel != null) resultPanel.SetActive(false);

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(OnExitButtonClicked);
            }
        }

        private void Update()
        {
            // Wait until TimeManager is valid
            if (TimeManager.Instance != null && TimeManager.Instance.Object != null && TimeManager.Instance.Object.IsValid)
            {
                // Hook up event if not done (doing it here ensures Instance is fully initialized by Fusion)
                TimeManager.Instance.OnStateChangedLocal -= HandleStateChanged;
                TimeManager.Instance.OnStateChangedLocal += HandleStateChanged;
                
                // Remove this block from Update so it only runs once to hook event
                this.enabled = false; 
            }
        }

        private void HandleStateChanged(LevelState newState)
        {
            if (newState == LevelState.Completed || newState == LevelState.Failed)
            {
                ShowResult(TimeManager.Instance.FinalResult);
            }
        }

        private void ShowResult(LevelResult result)
        {
            if (result == null) return;

            if (resultPanel != null) resultPanel.SetActive(true);

            if (titleText != null)
                titleText.text = result.IsCompleted ? "Level Completed!" : "Level Failed!";

            if (remainingTimeText != null)
            {
                int min = Mathf.FloorToInt(result.RemainingTime / 60f);
                int sec = Mathf.FloorToInt(result.RemainingTime % 60f);
                remainingTimeText.text = string.Format("Time Remaining: {0:00}:{1:00}", min, sec);
            }

            if (totalTimeText != null)
            {
                float elapsedTime = result.TotalTime - result.RemainingTime;
                int min = Mathf.FloorToInt(elapsedTime / 60f);
                int sec = Mathf.FloorToInt(elapsedTime % 60f);
                totalTimeText.text = string.Format("Completion Time: {0:00}:{1:00}", min, sec);
            }

            if (completionPercentageText != null)
                completionPercentageText.text = string.Format("Completion: {0}%", Mathf.RoundToInt(result.CompletionPercentage * 100f));

            if (finalScoreText != null)
                finalScoreText.text = $"Score: {result.FinalScore}";

            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                {
                    starIcons[i].SetActive(i < result.StarCount);
                }
            }
        }

        public void OnExitButtonClicked()
        {
            // Ngắt kết nối mạng (Fusion)
            if (TimeManager.Instance != null && TimeManager.Instance.Runner != null)
            {
                TimeManager.Instance.Runner.Shutdown();
            }

            // Chuyển về Scene mặc định (thường Scene 0 là Lobby / Menu chính)
            SceneManager.LoadScene(0);
        }
    }
}
