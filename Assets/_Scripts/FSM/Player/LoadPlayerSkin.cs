using Fusion;
using UnityEngine;

public class LoadPlayerSkin : NetworkBehaviour
{
    [SerializeField] private SkinnedMeshRenderer playerMeshRender;

    public override void Spawned()
    {
        base.Spawned();

        PlayerRef owner = Object.StateAuthority;

        PlayerColor savedColor = PlayerSkinData.Instance.GetPlayerSkin(owner);

        if (playerMeshRender != null && PlayerSkinData.Instance != null)
        {
            Material targetMat = PlayerSkinData.Instance.GetMaterial(savedColor);
            if (targetMat != null)
            {
                playerMeshRender.material = targetMat;
            }
        }
    }
}
