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
            PlayerName = "Player_" + LobbyManager.Instance.playersInLobby.Count;
            IsReady = false;
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
