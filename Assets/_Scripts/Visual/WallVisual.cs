using UnityEngine;

public class WallVisual : MonoBehaviour
{
    Renderer rend;

    MaterialPropertyBlock block;

    [Range(0, 1)]
    public float progress;

    public bool completed;

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

    public void UpdateVisual()
    {
        rend.GetPropertyBlock(block);

        block.SetFloat("_Progress", progress);

        rend.SetPropertyBlock(block);
    }
    
}