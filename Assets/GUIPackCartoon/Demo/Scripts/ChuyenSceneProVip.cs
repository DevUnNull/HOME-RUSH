using UnityEngine;
using UnityEngine.SceneManagement;

public class ChuyenSceneProVip : MonoBehaviour
{
    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void GoToLevel()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}