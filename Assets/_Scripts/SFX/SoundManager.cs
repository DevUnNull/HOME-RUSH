using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;

    [Header("SFX")]
    public AudioClip buttonClick;
    public AudioClip pickup;
    public AudioClip paint;
    public AudioClip sprint;
    public AudioClip _throw;
    public AudioClip collide;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //private void Start()
    //{
    //    PlayMusic(menuMusic);
    //}

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayLoopSFX(AudioClip clip)
    {
        if (sfxSource.clip == clip && sfxSource.isPlaying)
            return;

        sfxSource.clip = clip;
        sfxSource.loop = true;
        sfxSource.Play();
    }

    public void StopLoopSFX()
    {
        sfxSource.Stop();
        sfxSource.loop = false;
    }
}