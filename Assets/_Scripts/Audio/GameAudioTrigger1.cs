using Fusion;
using UnityEngine;

public class GameAudioTrigger1MenuAudioTrigger : NetworkBehaviour, IPlayerJoined
{
    [SerializeField] private AudioChannelSO audioChannel;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;

    public void PlayerJoined(PlayerRef player)
    {
        audioChannel.TriggerAudio(audioClip, audioSource);
    }

    //public override void Spawned()
    //{
    //    base.Spawned();

    //    audioChannel.TriggerAudio(audioClip, audioSource);
    //}
}
