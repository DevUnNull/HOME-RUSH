using Fusion;
using UnityEngine;

public class PaintSpill : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerMovement>(out var movement))
            {
                if (Object.HasStateAuthority)
                    movement.IsOnPaint = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerMovement>(out var movement))
            {
                if (Object.HasStateAuthority)
                    movement.IsOnPaint = false;
            }
        }
    }
}
