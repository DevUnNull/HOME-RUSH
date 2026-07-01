using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioChannel", menuName = "Audio/AudioChannel")]
public class AudioChannelSO : ScriptableObject
{
    public Action<AudioClip, AudioSource> OnTriggerAudio;
    public Action OnStopAudio;

    public void TriggerAudio(AudioClip clip, AudioSource audioSource)
    {
        OnTriggerAudio?.Invoke(clip, audioSource);
    }

    public void StopAudio()
    {
        OnStopAudio?.Invoke();
    }
}
