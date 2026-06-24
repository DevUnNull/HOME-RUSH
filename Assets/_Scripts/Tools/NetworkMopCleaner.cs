using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkMopCleaner : NetworkBehaviour
{
    [Header("Mop Settings")]
    [SerializeField] private Key cleanKey = Key.E;
    [SerializeField] private float cleanRadius = 1.5f;
    [SerializeField] private LayerMask spillLayerMask = ~0; // Mặc định quét mọi Layer

    void Update()
    {
        if (Object == null || !Object.HasInputAuthority)
            return;

        if (Keyboard.current[cleanKey].isPressed)
        {
            TryClean();
        }
    }

    private void TryClean()
    {
        PlayerTopEntity playerEntity = GetComponentInParent<PlayerTopEntity>();
        if (playerEntity == null || playerEntity.HeldItem == null)
            return;

        Item heldItem = playerEntity.HeldItem.GetComponent<Item>();
        if (heldItem == null || heldItem.itemType != ItemType.Mop)
            return;

        // Quét xung quanh vị trí người chơi
        Collider[] hits = Physics.OverlapSphere(transform.position, cleanRadius, spillLayerMask, QueryTriggerInteraction.Collide);

        foreach (var hit in hits)
        {
            PaintSpill spill = hit.GetComponentInParent<PaintSpill>();
            if (spill != null)
            {
                Debug.Log($"NetworkMopCleaner: Cleaning spill {spill.name}");
                spill.RPC_CleanSpill();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, cleanRadius);
    }
}
