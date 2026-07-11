using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyChangeSkin : NetworkBehaviour
{
    [SerializeField] private SkinnedMeshRenderer playerMeshRender;

    private PlayerInput inputActions;
    private ChangeDetector changeDetector;

    [Networked] public PlayerColor CurrentColor { get; set; }

    public override void Spawned()
    {
        base.Spawned();

        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (HasStateAuthority)
        {
            inputActions = new PlayerInput();
            inputActions.Enable();
            inputActions.Player.ChangeSkin.performed += OnChangeSkinInput;
        }

        UpdateSkinColor();

        PlayerSkinData.Instance.SetPlayerSkin(Object.StateAuthority, CurrentColor);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (inputActions != null)
        {
            inputActions.Player.ChangeSkin.performed -= OnChangeSkinInput;
            inputActions.Disable();
        }
    }

    private void OnChangeSkinInput(InputAction.CallbackContext context)
    {
        if (!HasStateAuthority) return;

        Debug.Log("AA");

        float direction = context.ReadValue<float>();
        int totalColors = System.Enum.GetValues(typeof(PlayerColor)).Length;
        int currentColorIndex = (int)CurrentColor;

        if (direction > 0)
        {
            currentColorIndex = (currentColorIndex + 1) % totalColors;
        }
        else if (direction < 0)
        {
            currentColorIndex = (currentColorIndex - 1 + totalColors) % totalColors;
        }

        CurrentColor = (PlayerColor)currentColorIndex;
    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(CurrentColor):
                    UpdateSkinColor();
                    PlayerSkinData.Instance.SetPlayerSkin(Object.StateAuthority, CurrentColor);
                    break;
            }
        }
    }

    private void UpdateSkinColor()
    {
        if (playerMeshRender != null)
        {
            Material newMat = PlayerSkinData.Instance.GetMaterial(CurrentColor);
            if (newMat != null)
            {
                playerMeshRender.material = newMat;
            }
        }
    }
}