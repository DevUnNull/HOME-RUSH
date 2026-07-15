using Fusion;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class AudioManager : NetworkBehaviour
{
    [SerializeField] private AudioChannelSO sfxChannel;
    [SerializeField] private AudioChannelSO bgmChannel;

    [SerializeField] private AudioMixer mainMixer;

    [SerializeField] private GameObject audioCanvas;

    private PlayerInput playerInput;

    private void Awake()
    {
        sfxChannel.OnTriggerAudio += PlaySFX;
        bgmChannel.OnTriggerAudio += PlayBGM;

        sfxChannel.OnStopAudio += StopSFX;
        bgmChannel.OnStopAudio += StopBGM;

        playerInput = new PlayerInput();
        playerInput.Enable();
        playerInput.Player.Setting.started += ToggleSettingCanva;
    }

    public override void Spawned()
    {
        base.Spawned();
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnDestroy()
    {
        sfxChannel.OnTriggerAudio -= PlaySFX;
        bgmChannel.OnTriggerAudio -= PlayBGM;

        sfxChannel.OnStopAudio -= StopSFX;
        bgmChannel.OnStopAudio -= StopBGM;
    }

    private void PlaySFX(AudioClip clip, AudioSource audioSource)
    {
        audioSource.PlayOneShot(clip);
    }

    private void PlayBGM(AudioClip clip, AudioSource audioSource)
    {
        audioSource.clip = clip;
        audioSource.Play();
        audioSource.loop = true;
    }

    private void StopSFX()
    {

    }

    private void StopBGM()
    {

    }

    private void ToggleSettingCanva(InputAction.CallbackContext context)
    {
        if (audioCanvas.gameObject.activeSelf)
        {
            audioCanvas.gameObject.SetActive(false);
        }
        else
        {
            audioCanvas.gameObject.SetActive(true);
        }
    }
}
