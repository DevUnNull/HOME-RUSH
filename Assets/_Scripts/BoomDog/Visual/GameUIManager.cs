using UnityEngine;
using TMPro;

namespace BoomDog.Visual
{
    public class GameUIManager : MonoBehaviour
    {
        public static GameUIManager Instance { get; private set; }

        [Header("End Game UI")]
        public GameObject endGamePanel;
        public TextMeshProUGUI endGameText;

        [Header("Log System")]
        public TextMeshProUGUI logTextPrefab;
        public Transform logContainer;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void ShowWinner(string winnerName)
        {
            if (endGamePanel != null)
            {
                endGamePanel.SetActive(true);
                endGameText.text = $"🎉 Người thắng cuộc:\n{winnerName}";
            }
        }

        public void ShowLoser()
        {
            if (endGamePanel != null)
            {
                endGamePanel.SetActive(true);
                endGameText.text = "💥 Bạn đã bị NỔ! 💥";
            }
        }

        public void AddLog(string message)
        {
            if (logTextPrefab != null && logContainer != null)
            {
                TextMeshProUGUI newLog = Instantiate(logTextPrefab, logContainer);
                newLog.text = message;
                
                // Tự động xóa log sau 5 giây
                Destroy(newLog.gameObject, 5f);
            }
            else
            {
                Debug.Log($"[Game Log]: {message}");
            }
        }
    }
}
