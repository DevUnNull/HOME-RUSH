using Fusion;
using UnityEngine;

public class ChooseLevelManger : NetworkBehaviour
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

    public void ChooseLevel(int indexInProfile)
    {
        Runner.LoadScene(SceneRef.FromIndex(indexInProfile));
    }
}
