using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("-------- Audio Source --------")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;

    [Header("-------- Audio Clip --------")]
    public AudioClip backgroundMusic;
    public AudioClip coinsSfx;
    public AudioClip jumpSfx;
    public AudioClip WinSfx;
    public AudioClip LooseSfx;
    public AudioClip ShootSfx;
    public AudioClip portalSfx;
    public AudioClip SwordAttackSfx;
    public AudioClip MonsterAttackSfx;
    [Header("-------- Audio Clip UI --------")]
    public AudioClip ButtonUI;
    public AudioClip HoverUi;
    private void Start()
    {
        // Play background music on start
        musicSource.clip = backgroundMusic;
        musicSource.Play();
    }

    public void Stop()
    {
        // Stop all audio sources
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
        if (sfxSource != null)
        {
            sfxSource.Stop();
            sfxSource.clip = null;
        }
    }
    public void PlaySFX(AudioClip clip)
    {
        // Play a one-shot sound effect
        sfxSource.PlayOneShot(clip);
    }
    public void PlayUI(AudioClip clip)
    {
        // Play a one-shot UI sound effect
        uiSource.PlayOneShot(clip);
    }
    public void PlayLoopedSfx(AudioClip clip)
    {
        // Play a looped sound effect
        sfxSource.clip = clip;
        sfxSource.loop = true;
        sfxSource.Play();
    }
}