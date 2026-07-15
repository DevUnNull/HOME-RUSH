using Fusion;
using UnityEngine;

public class LobbyPlayerState : NetworkBehaviour
{
    [Networked] public NetworkString<_32> PlayerName { get; set; }
    [Networked] public NetworkBool IsReady { get; set; }

    private PlayerInput inputActions;


    public override void Spawned()
    {
        base.Spawned();

        inputActions = new PlayerInput();
        inputActions.Enable();
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.AddPlayerToList(this);
        }

        if (HasStateAuthority)
        {
            string savedName = PlayerPrefs.GetString("PlayerName", "");
            if (string.IsNullOrEmpty(savedName)) {
                PlayerName = "Player " + Runner.LocalPlayer.PlayerId;
            } else {
                PlayerName = savedName;
            }
            IsReady = false;
        }
    }

    public void ChangeName(string newName)
    {
        if (HasStateAuthority)
        {
            PlayerName = newName;
            PlayerPrefs.SetString("PlayerName", newName);
            PlayerPrefs.Save();
        }
    }

    private void Update()
    {
        if (inputActions.Player.Ready.WasPressedThisFrame())
        {
            if (HasStateAuthority)
            {
                Ready();
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        inputActions.Disable();
        LobbyManager.Instance.RemovePlayerFromList(this);
    }

    public void Ready()
    {
        if (HasStateAuthority)
        {
            IsReady = true;
        }
        LobbyManager.Instance.UpdateRoomUI();
    }
}
