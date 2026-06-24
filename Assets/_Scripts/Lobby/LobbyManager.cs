using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    public static LobbyManager Instance;

    [SerializeField] private GameObject playerStatePrefap;

    [SerializeField] private TextMeshProUGUI roomIdText;
    [SerializeField] private TextMeshProUGUI[] playerReadyTexts;

    public List<LobbyPlayerState> playersInLobby = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Runner == null) return;

        for (int i = 0; i < playerReadyTexts.Length; i++)
        {
            if (i < playersInLobby.Count)
            {
                playerReadyTexts[i].gameObject.SetActive(true);

                LobbyPlayerState p = playersInLobby[i];

                string readyStatus = p.IsReady ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
                playerReadyTexts[i].text = $"Player {p.Object.StateAuthority.PlayerId}: {readyStatus}";
            }
            else
            {
                playerReadyTexts[i].gameObject.SetActive(false);
            }
        }
    }

    public override void Spawned()
    {
        base.Spawned();

        UpdateRoomUI();

        Vector3 spawnPosition = new Vector3(2 * (Runner.LocalPlayer.PlayerId - 1), 1, 0);
        Runner.Spawn(playerStatePrefap.GetComponent<NetworkObject>(), spawnPosition, Quaternion.identity, Runner.LocalPlayer);

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
        if (Runner.IsSharedModeMasterClient)
        {
            if (!AllPlayersReady())
            {
                return;
            }

            Runner.LoadScene(SceneRef.FromIndex(2));
        }
    }
}