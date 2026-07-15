using UnityEngine;

public class LevelSelectButton : MonoBehaviour
{
    [Header("Level Information")]
    [Tooltip("Tên của Scene tương ứng với màn chơi này (ví dụ: Level1)")]
    public string levelSceneName;

    [Header("UI References")]
    [SerializeField] private GameObject[] starIcons; // Kéo thả 3 ngôi sao vào đây (Star 1, Star 2, Star 3)

    private void Start()
    {
        UpdateStars();
    }

    private void OnEnable()
    {
        UpdateStars();
    }

    public void UpdateStars()
    {
        int savedStars = 0;
        if (!string.IsNullOrEmpty(levelSceneName))
        {
            // Lấy số sao đã lưu trong máy
            savedStars = PlayerPrefs.GetInt("LevelStars_" + levelSceneName, 0);
        }

        // Hiển thị sao
        if (starIcons != null)
        {
            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                {
                    // Hiện ngôi sao nếu số sao đạt được lớn hơn index của ngôi sao đó
                    starIcons[i].SetActive(i < savedStars);
                }
            }
        }
    }
}
