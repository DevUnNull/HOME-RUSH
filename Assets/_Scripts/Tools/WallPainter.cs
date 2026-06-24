using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkWallPainter : NetworkBehaviour
{
    [Header("Tool")]
    [SerializeField] private ToolController tool;

    [Header("Paint")]
    [SerializeField] private float paintSpeed = 2f;
    [SerializeField] private Key paintKey = Key.E;
    [SerializeField] private Key eraseKey = Key.R;

    void Update()
    {
        if (Object == null)
        {
            Debug.Log("NetworkWallPainter.Update: Object is null");
            return;
        }

        if (!Object.HasInputAuthority)
        {
            Debug.Log("NetworkWallPainter.Update: no input authority");
            return;
        }

        if (Keyboard.current[paintKey].isPressed)
        {
            Debug.Log("NetworkWallPainter.Update: Paint key pressed");
            TryPaint();
        }
        else if (Keyboard.current[eraseKey].isPressed)
        {
            Debug.Log("NetworkWallPainter.Update: Erase key pressed");
            TryErase();
        }
    }

    public void TryPaint()
    {
        if (tool == null)
        {
            Debug.LogWarning("NetworkWallPainter.TryPaint: tool is null");
            return;
        }

        if (!tool.TryGetWallGroup(
            out NetworkWallGroup group,
            out NetworkWallVisual wall))
        {
            Debug.LogWarning("NetworkWallPainter.TryPaint: TryGetWallGroup returned false");
            return;
        }

        if (group == null)
        {
            Debug.LogWarning("NetworkWallPainter.TryPaint: group is null after TryGetWallGroup");
            return;
        }

        if (wall == null)
        {
            Debug.Log("NetworkWallPainter.TryPaint: hit wall is null, trying from sequence controller");
            if (group.sequenceController == null)
            {
                Debug.LogWarning("NetworkWallPainter.TryPaint: group.sequenceController is null");
                return;
            }

            wall = group.sequenceController.CurrentWall;
            if (wall == null)
            {
                Debug.LogWarning("NetworkWallPainter.TryPaint: no current wall available");
                return;
            }
        }

        if (PaintColorManager.Instance == null)
        {
            Debug.LogError("NetworkWallPainter.TryPaint: PaintColorManager.Instance is null");
            return;
        }

        Color selectedColor = PaintColorManager.Instance.currentColor;
        Debug.Log($"NetworkWallPainter.TryPaint: selectedColor={selectedColor} wall={wall.name} wallId={wall.Object.Id}");

        RPC_PaintWall(
            wall.Object.Id,
            selectedColor
        );
    }

    [Rpc(RpcSources.InputAuthority,
         RpcTargets.All)]
    void RPC_PaintWall(
        NetworkId wallId,
        Color color)
    {
        Debug.Log($"NetworkWallPainter.RPC_PaintWall: received wallId={wallId} color={color} on {gameObject.name}");

        NetworkObject obj =
            Runner.FindObject(wallId);

        if (obj == null)
        {
            Debug.LogWarning($"NetworkWallPainter.RPC_PaintWall: Runner.FindObject returned null for {wallId}");
            return;
        }

        NetworkWallVisual wall =
            obj.GetComponent<NetworkWallVisual>();

        if (wall == null)
        {
            Debug.LogWarning($"NetworkWallPainter.RPC_PaintWall: NetworkWallVisual not found on object {obj.name}");
            return;
        }

        wall.SetPaintColor(color);

        wall.AddProgress(
            Runner.DeltaTime * paintSpeed
        );

        Debug.Log($"NetworkWallPainter.RPC_PaintWall: applied progress={wall.Progress} completed={wall.Completed} color={wall.PaintColor}");

        if (wall.sequenceController != null)
        {
            wall.sequenceController.CheckProgress();
        }
    }

    public void TryErase()
    {
        if (tool == null)
        {
            Debug.LogWarning("NetworkWallPainter.TryErase: tool is null");
            return;
        }

        if (!tool.TryGetWallGroup(
            out NetworkWallGroup group,
            out NetworkWallVisual wall))
        {
            Debug.LogWarning("NetworkWallPainter.TryErase: TryGetWallGroup returned false");
            return;
        }

        if (group == null)
        {
            Debug.LogWarning("NetworkWallPainter.TryErase: group is null after TryGetWallGroup");
            return;
        }

        if (wall == null)
        {
            Debug.Log("NetworkWallPainter.TryErase: hit wall is null, trying from sequence controller");
            if (group.sequenceController == null)
            {
                Debug.LogWarning("NetworkWallPainter.TryErase: group.sequenceController is null");
                return;
            }

            wall = group.sequenceController.GetWallToErase();
            if (wall == null)
            {
                Debug.LogWarning("NetworkWallPainter.TryErase: no wall available to erase");
                return;
            }
        }

        if (wall.Progress > 0)
        {
            Debug.Log($"NetworkWallPainter.TryErase: wall={wall.name} wallId={wall.Object.Id}");
            RPC_EraseWall(wall.Object.Id);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    void RPC_EraseWall(NetworkId wallId)
    {
        Debug.Log($"NetworkWallPainter.RPC_EraseWall: received wallId={wallId} on {gameObject.name}");

        NetworkObject obj = Runner.FindObject(wallId);

        if (obj == null)
        {
            Debug.LogWarning($"NetworkWallPainter.RPC_EraseWall: Runner.FindObject returned null for {wallId}");
            return;
        }

        NetworkWallVisual wall = obj.GetComponent<NetworkWallVisual>();

        if (wall == null)
        {
            Debug.LogWarning($"NetworkWallPainter.RPC_EraseWall: NetworkWallVisual not found on object {obj.name}");
            return;
        }

        wall.RemoveProgress(Runner.DeltaTime * paintSpeed);

        Debug.Log($"NetworkWallPainter.RPC_EraseWall: applied progress={wall.Progress} completed={wall.Completed}");

        if (wall.sequenceController != null)
        {
            wall.sequenceController.CheckProgress();
        }
    }
}