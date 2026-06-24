using Fusion;
using UnityEngine;

public class NetworkWallVisual : NetworkBehaviour
{
    Renderer rend;
    MaterialPropertyBlock block;

    [Networked]
    public float Progress { get; set; }

    [Networked]
    public bool Completed { get; set; }

    [Networked]
    public Color PaintColor { get; set; }

    public NetworkWallSequence sequenceController;

    public override void Spawned()
    {
        rend = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
    }

    public override void Render()
    {
        if (rend == null)
        {
            rend = GetComponent<Renderer>();
            if (rend == null)
                return;
        }

        UpdateVisual();
    }

    public void AddProgress(float amount)
    {
        if (!Object.HasStateAuthority)
            return;

        if (Completed)
            return;

        Progress += amount;

        if (Progress >= 1f)
        {
            Progress = 1f;
            Completed = true;
        }
    }

    public void RemoveProgress(float amount)
    {
        if (!Object.HasStateAuthority)
            return;

        Progress -= amount;

        if (Progress <= 0f)
        {
            Progress = 0f;
        }

        if (Progress < 1f && Completed)
        {
            Completed = false;
        }
    }

    public void SetPaintColor(Color color)
    {
        if (!Object.HasStateAuthority)
            return;

        PaintColor = color;
    }

    public void UpdateVisual()
    {
        rend.GetPropertyBlock(block);

        block.SetFloat("_Progress", Progress);
        block.SetColor("_PaintColor", PaintColor);

        rend.SetPropertyBlock(block);
    }
}