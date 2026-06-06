using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkWallPainter : NetworkBehaviour
{
    [Header("Tool")]
    [SerializeField] private ToolController tool;

    [Header("Paint")]
    [SerializeField] private float paintSpeed = 2f;

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

        if (Keyboard.current.eKey.isPressed)
        {
            Debug.Log("NetworkWallPainter.Update: E pressed");
            TryPaint();
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
}