using UnityEngine;
using UnityEngine.UI;

public class AudioSettingCanva : MonoBehaviour
{
    [SerializeField] private Slider masterSlide;
    [SerializeField] private Slider bgmSlide;
    [SerializeField] private Slider sfxSlide;

    private void OnEnable()
    {
        masterSlide.value = PlayerPrefs.GetFloat("Master", 1f);
        bgmSlide.value = PlayerPrefs.GetFloat("BGM", 1f);
        sfxSlide.value = PlayerPrefs.GetFloat("SFX", 1f);
    }
}
