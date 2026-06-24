using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PaintSpill : NetworkBehaviour
{
    private List<PlayerMovement> playersInside = new List<PlayerMovement>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerMovement>(out var movement))
            {
                playersInside.Add(movement);
                if (movement.HasStateAuthority)
                    movement.PaintSpillCount++;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerMovement>(out var movement))
            {
                playersInside.Remove(movement);
                if (movement.HasStateAuthority)
                    movement.PaintSpillCount--;
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        foreach (var movement in playersInside)
        {
            if (movement != null && movement.HasStateAuthority)
            {
                movement.PaintSpillCount--;
            }
        }
        playersInside.Clear();
        base.Despawned(runner, hasState);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_CleanSpill()
    {
        if (Object != null && Object.IsValid)
        {
            Runner.Despawn(Object);
        }
    }
}
