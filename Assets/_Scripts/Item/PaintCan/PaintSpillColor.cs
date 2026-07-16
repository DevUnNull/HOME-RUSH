using UnityEngine;

public class PaintSpillColor : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_Renderer;

    public void Init(Color color)
    {
        m_Renderer.material.color = color;
    }
}
