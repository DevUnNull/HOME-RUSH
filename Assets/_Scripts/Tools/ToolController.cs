using UnityEngine;

public class ToolController : MonoBehaviour
{
    public Transform interactionPoint;

    public float interactionDistance = 1f;

    public LayerMask floorMask;

    public int materialID;

    public bool TryGetHit(out RaycastHit hit)
    {
        return Physics.Raycast(
            interactionPoint.position,
            interactionPoint.forward,
            out hit,
            interactionDistance,
            floorMask
        );
    }
}