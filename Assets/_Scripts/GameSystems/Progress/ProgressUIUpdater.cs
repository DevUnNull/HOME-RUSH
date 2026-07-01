using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressUIUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalProgressText;
    [SerializeField] private Slider totalProgressBar;
    
    // For a single room display (e.g. BadRoom) in the Task Board
    [SerializeField] private TextMeshProUGUI roomProgressText;
    [SerializeField] private Slider roomProgressBar;
    [SerializeField] private string targetRoomName = "BadRoom";

    private void Update()
    {
        if (GameProgressManager.Instance == null) return;

        if (totalProgressText != null)
        {
            float total = GameProgressManager.Instance.GetTotalProgress();
            totalProgressText.text = $"{Mathf.FloorToInt(total * 100)}%";
            
            if (totalProgressBar != null)
            {
                totalProgressBar.value = total;
            }
        }

        if (roomProgressText != null)
        {
            RoomProgress room = GameProgressManager.Instance.GetRoomByName(targetRoomName);
            if (room != null)
            {
                float roomProg = room.GetRoomProgress();
                roomProgressText.text = $"{Mathf.FloorToInt(roomProg * 100)}%";
                
                if (roomProgressBar != null)
                {
                    roomProgressBar.value = roomProg;
                }
            }
            else 
            {
                roomProgressText.text = "0%";
                if (roomProgressBar != null)
                {
                    roomProgressBar.value = 0f;
                }
            }
        }
    }
}
