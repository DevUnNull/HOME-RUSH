using Fusion;
using UnityEngine;

public class MenuAudioTrigger : NetworkBehaviour
{
    [SerializeField] private AudioChannelSO audioChannel;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;

    public override void Spawned()
    {
        base.Spawned();
        
        Debug.Log("MenuAudioTrigger Spawned");
        audioChannel.TriggerAudio(audioClip, audioSource);
    }
}
