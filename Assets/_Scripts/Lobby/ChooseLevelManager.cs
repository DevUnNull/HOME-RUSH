using Fusion;
using UnityEngine;

public class ChooseLevelManager : NetworkBehaviour
{
    [SerializeField] private GameObject levelChooseCanva;

    public void OpenLevelChoosing()
    {
        RPC_SetLevelCanvaState(true);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetLevelCanvaState(bool isOpen)
    {
        levelChooseCanva.SetActive(isOpen);
    }

    public void ChooseLevel(int levelIndexInProfiles)
    {
        if (Runner.IsSharedModeMasterClient)
        {
            Runner.LoadScene(SceneRef.FromIndex(levelIndexInProfiles));
        }
    }
}
