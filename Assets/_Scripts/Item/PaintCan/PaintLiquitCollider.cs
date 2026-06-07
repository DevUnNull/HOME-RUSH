using Fusion;
using UnityEngine;

public class PaintLiquitCollider : NetworkBehaviour
{
    [SerializeField] private PaintCan paintCan;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            Vector3 collisionPoint = other.ClosestPoint(transform.position);

            paintCan.Spill(collisionPoint);
        }
    }
}
