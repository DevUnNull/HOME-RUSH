using UnityEngine;

public class PanelController : MonoBehaviour
{
    [SerializeField] public GameObject panel;

    public void OpenPanel()
    {
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }
}