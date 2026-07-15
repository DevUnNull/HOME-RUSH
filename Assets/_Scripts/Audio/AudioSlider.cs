using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private string paraName;
    [SerializeField] private AudioMixer mainMixer;

    private void Start()
    {
        slider.onValueChanged.AddListener(SetVolumn);
    }

    private void SetVolumn(float volumn)
    {
        float dB = Mathf.Log10(Mathf.Max(0.0001f, volumn)) * 20;
        mainMixer.SetFloat(paraName, dB);

        PlayerPrefs.SetFloat(paraName, volumn);
    }
}
