using UnityEngine;

public class ToolController : MonoBehaviour
{
    [Header("Interaction")]
    public Transform interactionPoint;

    public float interactionDistance = 2f;

    public LayerMask interactionMask;

    [Header("Tool")]
    public int materialID;

    //------------------------------------------------
    // GENERIC HIT
    //------------------------------------------------

    public bool TryGetHit(out RaycastHit hit)
    {
        Vector3 origin = interactionPoint.position;
        // Offset raycast origin to chest/eye level if hand socket is at feet level
        if (interactionPoint.localPosition.y < 0.5f)
        {
            origin.y += 1.5f;
        }

        return Physics.Raycast(
            origin,
            interactionPoint.forward,
            out hit,
            interactionDistance,
            interactionMask,
            QueryTriggerInteraction.Collide
        );
    }

    //------------------------------------------------
    // WALL GROUP HIT
    //------------------------------------------------

    public bool TryGetWallGroup(
        out WallGroup group
    )
    {
        return TryGetWallGroup(out group, out _);
    }

    public bool TryGetWallGroup(
        out WallGroup group,
        out WallVisual hitWall
    )
    {
        group = null;
        hitWall = null;

        Vector3 origin = interactionPoint.position;
        // Offset raycast origin to chest/eye level if hand socket is at feet level
        if (interactionPoint.localPosition.y < 0.5f)
        {
            origin.y += 1.5f;
        }

        // Perform raycast with Collide interaction to hit triggers
        bool hasHit = Physics.Raycast(
            origin,
            interactionPoint.forward,
            out RaycastHit hit,
            interactionDistance,
            interactionMask,
            QueryTriggerInteraction.Collide
        );

        if (hasHit)
        {
            Debug.Log($"[TryGetWallGroup] Hit collider: {hit.collider.name} on Layer {hit.collider.gameObject.layer} at distance {hit.distance}");

            // Try to find WallGroup component on the hit collider or its ancestors
            group = hit.collider.GetComponentInParent<WallGroup>();
            hitWall = hit.collider.GetComponentInParent<WallVisual>();

            // Fallback: if WallGroup is not found, resolve it via WallVisual and its sequence controller
            if (group == null)
            {
                if (hitWall != null)
                {
                    Debug.Log($"[TryGetWallGroup] Found WallVisual: {hitWall.name}");
                    if (hitWall.sequenceController != null)
                    {
                        // Look for WallGroup on the sequence controller's GameObject
                        group = hitWall.sequenceController.GetComponent<WallGroup>();

                        // If still null, dynamically add WallGroup to the sequence controller's GameObject
                        if (group == null)
                        {
                            group = hitWall.sequenceController.gameObject.AddComponent<WallGroup>();
                            group.sequenceController = hitWall.sequenceController;
                            Debug.Log($"[TryGetWallGroup] Dynamically added WallGroup to {hitWall.sequenceController.name}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[TryGetWallGroup] WallVisual {hitWall.name} has no sequenceController reference!");
                    }
                }
                else
                {
                    Debug.LogWarning($"[TryGetWallGroup] Hit collider {hit.collider.name} has no WallGroup or WallVisual components!");
                }
            }

            return group != null;
        }
        else
        {
            // Debug check to see if we hit anything at all without mask restriction
            bool hitAnything = Physics.Raycast(
                origin,
                interactionPoint.forward,
                out RaycastHit testHit,
                interactionDistance,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide
            );

            if (hitAnything)
            {
                Debug.LogWarning($"[TryGetWallGroup] Raycast missed wall mask ({interactionMask.value}) but hit: {testHit.collider.name} on Layer {testHit.collider.gameObject.layer} at distance {testHit.distance}");
            }
            else
            {
                Debug.LogWarning($"[TryGetWallGroup] Raycast hit NOTHING at all within distance {interactionDistance} from origin {origin} along direction {interactionPoint.forward}");
            }
        }

        return false;
    }

    //------------------------------------------------
    // DEBUG
    //------------------------------------------------

    private void Update()
    {
        Vector3 origin = interactionPoint.position;
        if (interactionPoint.localPosition.y < 0.5f)
        {
            origin.y += 1.5f;
        }

        Debug.DrawRay(
            origin,
            interactionPoint.forward *
            interactionDistance,
            Color.red
        );
    }
}