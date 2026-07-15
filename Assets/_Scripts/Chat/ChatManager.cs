using Fusion;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        base.Spawned();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SendChat(string playerName, string message)
    {
        RPC_ReceiveChat(playerName, message);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReceiveChat(string playerName, string message)
    {
        ChatUI.Instance.AddMessage(playerName, message);
    }
}