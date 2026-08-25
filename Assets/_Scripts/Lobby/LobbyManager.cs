using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    public static LobbyManager Instance;

    [SerializeField] private GameObject playerStatePrefap;
    [SerializeField] private ChooseLevelManger chooseLevelManger;

    [SerializeField] private TextMeshProUGUI roomIdText;
    [SerializeField] private TextMeshProUGUI[] playerReadyTexts;

    [SerializeField] private int sceneIndex = 3;

    public List<LobbyPlayerState> playersInLobby = new();

    [Header("Name Edit UI")]
    [SerializeField] private TextMeshProUGUI[] playerNameTexts;
    [SerializeField] private GameObject nameInputPanel;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Button submitNameButton;

    private LobbyPlayerState localPlayer;

    private void Awake()
    {
        Instance = this;

        if (submitNameButton != null)
        {
            submitNameButton.onClick.AddListener(OnSubmitName);
        }

        if (playerNameTexts != null)
        {
            for (int i = 0; i < playerNameTexts.Length; i++)
            {
                int index = i;
                Button btn = playerNameTexts[i].GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => OnPlayerNameClicked(index));
                }
            }
        }
    }

    private void Update()
    {
        if (Runner == null) return;

        for (int i = 0; i < playerReadyTexts.Length; i++)
        {
            if (i < playersInLobby.Count)
            {
                playerReadyTexts[i].gameObject.SetActive(true);
                if (playerNameTexts != null && i < playerNameTexts.Length)
                {
                    playerNameTexts[i].gameObject.SetActive(true);
                }

                LobbyPlayerState p = playersInLobby[i];

                string readyStatus = p.IsReady ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
                playerReadyTexts[i].text = $"{p.PlayerName}: {readyStatus}";

                if (playerNameTexts != null && i < playerNameTexts.Length)
                {
                    playerNameTexts[i].text = p.PlayerName.ToString();
                }
            }
            else
            {
                playerReadyTexts[i].gameObject.SetActive(false);
                if (playerNameTexts != null && i < playerNameTexts.Length)
                {
                    playerNameTexts[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public override void Spawned()
    {
        base.Spawned();

        UpdateRoomUI();

        Vector3 spawnPosition = new Vector3(2 * (Runner.LocalPlayer.PlayerId - 1), 0, 0);
        Runner.Spawn(playerStatePrefap.GetComponent<NetworkObject>(), spawnPosition, Quaternion.Euler(0,-215f,0), Runner.LocalPlayer);

        LobbyPlayerState[] existingPlayers = FindObjectsByType<LobbyPlayerState>(FindObjectsSortMode.None);
        foreach (var player in existingPlayers)
        {
            AddPlayerToList(player);
        }
    }

    public void PlayerJoined(PlayerRef player)
    {
        UpdateRoomUI();
    }

    public void PlayerLeft(PlayerRef player)
    {
        UpdateRoomUI();
    }

    public void UpdateRoomUI()
    {
        if (roomIdText != null && Runner != null)
        {
            roomIdText.text = $"ID: {Runner.SessionInfo.Name}";
        }
    }

    public void AddPlayerToList(LobbyPlayerState playerState)
    {
        if (playersInLobby.Contains(playerState)) return;
        playersInLobby.Add(playerState);

        if (playerState.HasStateAuthority)
        {
            localPlayer = playerState;
        }
    }

    public void RemovePlayerFromList(LobbyPlayerState playerState)
    {
        if (playersInLobby.Contains(playerState))
        {
            playersInLobby.Remove(playerState);
        }
    }

    private bool AllPlayersReady()
    {
        if (playersInLobby.Count == 0) return false;

        foreach (var player in playersInLobby)
        {
            if (!player.IsReady) return false;
        }
        return true;
    }

    public void OnStartGameClicked()
    {
        if (Runner.IsSharedModeMasterClient || Runner.IsServer)
        {
            if (!AllPlayersReady()) return;

            // Chuyển toàn bộ Host và Client sang Scene InGame (Index = 3)
            Runner.LoadScene(SceneRef.FromIndex(sceneIndex));
        }
    }

    private void OnPlayerNameClicked(int index)
    {
        if (localPlayer == null) return;
        
        // Only allow clicking our own name
        int localIndex = playersInLobby.IndexOf(localPlayer);
        if (index == localIndex && nameInputPanel != null)
        {
            nameInputPanel.SetActive(true);
            playerNameInputField.text = localPlayer.PlayerName.ToString();
            playerNameInputField.ActivateInputField();
        }
    }

    private void OnSubmitName()
    {
        if (localPlayer != null && !string.IsNullOrWhiteSpace(playerNameInputField.text))
        {
            localPlayer.ChangeName(playerNameInputField.text);
        }

        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(false);
        }
    }
}