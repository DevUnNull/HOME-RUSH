using UnityEngine;

public class PaintUIButton : MonoBehaviour
{
    public Color buttonColor;

    public void SelectColor()
    {
        PaintColorManager.Instance.SetColor(buttonColor);
    }
}