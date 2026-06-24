using Fusion;
using UnityEngine;

public class SpawnPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    public override void Spawned()
    {
        base.Spawned();

        Runner.Spawn(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity, Runner.LocalPlayer);
    }
}
