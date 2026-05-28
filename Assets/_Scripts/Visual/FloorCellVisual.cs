using UnityEngine;

public class FloorCellVisual : MonoBehaviour
{
    Renderer rend;

    MaterialPropertyBlock block;

    private void Awake()
    {
        rend = GetComponent<Renderer>();

        block = new MaterialPropertyBlock();
    }

    public void UpdateVisual(float progress)
    {
        rend.GetPropertyBlock(block);

        block.SetFloat("_Progress", progress);

        rend.SetPropertyBlock(block);
    }
}