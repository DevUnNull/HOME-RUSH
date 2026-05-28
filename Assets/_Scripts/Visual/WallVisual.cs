using UnityEngine;

public class WallVisual : MonoBehaviour
{
    Renderer rend;

    MaterialPropertyBlock block;

    [Range(0, 1)]
    public float progress;

    public bool completed;

    public Color currentPaintColor = Color.blue;

    // GROUP OWNER
    public WallSequenceController sequenceController;

    void Awake()
    {
        rend = GetComponent<Renderer>();

        block = new MaterialPropertyBlock();
    }

    public void AddProgress(float amount)
    {
        if (completed)
            return;

        progress += amount;

        progress = Mathf.Clamp01(progress);

        UpdateVisual();

        if (progress >= 1f)
        {
            completed = true;

            Debug.Log(name + " COMPLETED");
        }
    }

    public void SetPaintColor(Color color)
    {
        currentPaintColor = color;

        UpdateVisual();
    }

    public void ResetWall()
    {
        progress = 0;

        completed = false;

        UpdateVisual();
    }

    public void UpdateVisual()
    {
        rend.GetPropertyBlock(block);

        block.SetFloat("_Progress", progress);

        block.SetColor("_PaintColor", currentPaintColor);

        rend.SetPropertyBlock(block);
    }
}