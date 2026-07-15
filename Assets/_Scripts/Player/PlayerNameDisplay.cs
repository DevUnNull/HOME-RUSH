using Fusion;
using TMPro;
using UnityEngine;

public class PlayerNameDisplay : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnPlayerNameChanged))] 
    public NetworkString<_32> PlayerName { get; set; }

    [SerializeField] private TextMeshProUGUI nameText;

    public override void Spawned()
    {
        base.Spawned();

        if (HasStateAuthority)
        {
            string savedName = PlayerPrefs.GetString("PlayerName", "");
            if (string.IsNullOrEmpty(savedName))
            {
                PlayerName = "Player " + Runner.LocalPlayer.PlayerId;
            }
            else
            {
                PlayerName = savedName;
            }
        }
        
        UpdateNameUI();
    }

    void OnPlayerNameChanged()
    {
        UpdateNameUI();
    }

    private void UpdateNameUI()
    {
        if (nameText != null)
        {
            nameText.text = PlayerName.ToString();
        }
    }

    private void LateUpdate()
    {
        if (nameText != null && Camera.main != null)
        {
            nameText.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
