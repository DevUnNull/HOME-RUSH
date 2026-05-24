using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFieldOfView : NetworkBehaviour
{
    [Header("Radius")]
    [SerializeField] float viewRadius = 8f;

    [Header("Angle")]
    [Range(0, 360), SerializeField] private float viewAngle = 90f;

    [Header("Target Mask")]
    [SerializeField] private LayerMask targetMask;
    public List<LayerMask> obstacleMasks = new List<LayerMask>();

    [Header("Targets")]
    public List<Transform> visibleOrderedTargets = new();

    private List<Transform> unorder = new();

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (!HasStateAuthority) return;

        FindVisibleTargets();
    }

    private void FindVisibleTargets()
    {
        visibleOrderedTargets.Clear();
        int combinedObstacleMask = 0;

        foreach (LayerMask mask in obstacleMasks)
        {
            combinedObstacleMask |= mask.value;
        }

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, combinedObstacleMask))
                {
                    visibleOrderedTargets.Add(target);
                }
            }
        }

        unorder.Sort((a, b) =>
        {  
            float distA = (a.position - transform.position).sqrMagnitude;
            float distB = (b.position - transform.position).sqrMagnitude;
            return distA.CompareTo(distB);
        });

        visibleOrderedTargets.AddRange(unorder);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);

        Gizmos.color = Color.red;
        foreach (Transform visibleTarget in visibleOrderedTargets)
        {
            Gizmos.DrawLine(transform.position, visibleTarget.position);
        }
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
