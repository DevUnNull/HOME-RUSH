using Fusion;
using UnityEngine;

public class PaintCan : NetworkBehaviour
{
    [Networked] public bool hasAlreadySpilled { get; set; } = false;
    [SerializeField] public Color paintColor = Color.blue;

    [SerializeField] private GameObject paintLiquidPrefab;

    public void Spill(Vector3 colPoint)
    {
        if (hasAlreadySpilled) return;

        if (!Object.HasStateAuthority)
        {
            Object.RequestStateAuthority();
        }

        Vector3 pos = new Vector3(colPoint.x, colPoint.y + 0.005f, colPoint.z);

        NetworkObject aa = Runner.Spawn(paintLiquidPrefab, pos, Quaternion.identity);
        aa.GetComponent<PaintSpillColor>().Init(paintColor);
        hasAlreadySpilled = true;
    }
}
