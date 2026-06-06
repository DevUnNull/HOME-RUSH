using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HideableObject : MonoBehaviour
{
    [Header("Materials")]
    [Tooltip("Material được sử dụng khi vật thể ở trạng thái bình thường.")]
    public Material solidMaterial;

    [Tooltip("Material được sử dụng khi vật thể bị làm mờ.")]
    public Material transparentMaterial;

    [Tooltip("Độ mờ dùng khi không có transparentMaterial và hệ thống tạo fallback material.")]
    [Range(0.05f, 1f)]
    public float fallbackAlpha = 0.35f;

    private Renderer renderComponent;
    private Material[] originalMaterials;
    private Material[] transparentMaterials;
    private bool isTransparent;

    private void Awake()
    {
        renderComponent = GetComponent<Renderer>();
        originalMaterials = renderComponent.sharedMaterials;

        if (solidMaterial == null && originalMaterials.Length > 0)
        {
            solidMaterial = originalMaterials[0];
        }

        if (transparentMaterial != null)
        {
            transparentMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < transparentMaterials.Length; i++)
            {
                transparentMaterials[i] = transparentMaterial;
            }
        }
    }

    public void MakeTransparent()
    {
        if (renderComponent == null || isTransparent)
            return;

        if (transparentMaterial == null)
        {
            CreateFallbackTransparentMaterials();
        }
        else if (transparentMaterials == null || transparentMaterials.Length != originalMaterials.Length)
        {
            BuildTransparentMaterialsFromReference();
        }

        if (transparentMaterials != null)
        {
            renderComponent.materials = transparentMaterials;
            isTransparent = true;
        }
    }

    public void MakeSolid()
    {
        if (renderComponent == null || !isTransparent)
            return;

        if (originalMaterials != null && originalMaterials.Length > 0)
        {
            renderComponent.materials = originalMaterials;
        }

        isTransparent = false;
    }

    private void OnDisable()
    {
        MakeSolid();
    }

    private void OnDestroy()
    {
        MakeSolid();
    }

    private void CreateFallbackTransparentMaterials()
    {
        Material[] sourceMaterials = renderComponent.sharedMaterials;
        transparentMaterials = new Material[sourceMaterials.Length];

        for (int i = 0; i < sourceMaterials.Length; i++)
        {
            Material original = sourceMaterials[i];
            if (original == null)
            {
                transparentMaterials[i] = null;
                continue;
            }

            Material clone = new Material(original);
            if (clone.HasProperty("_Color"))
            {
                Color c = clone.color;
                c.a = fallbackAlpha;
                clone.color = c;
            }
            clone.SetFloat("_Mode", 2);
            clone.EnableKeyword("_ALPHABLEND_ON");
            clone.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            transparentMaterials[i] = clone;
        }
    }

    private void BuildTransparentMaterialsFromReference()
    {
        transparentMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < transparentMaterials.Length; i++)
        {
            transparentMaterials[i] = transparentMaterial;
        }
    }
}
