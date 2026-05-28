using UnityEngine;

public class PaintColorManager : MonoBehaviour
{
    public static PaintColorManager Instance;

    public Color currentColor = Color.blue;

    void Awake()
    {
        Instance = this;
    }

    public void SetColor(Color color)
    {
        currentColor = color;

        Debug.Log("SELECT COLOR: " + color);
    }
}