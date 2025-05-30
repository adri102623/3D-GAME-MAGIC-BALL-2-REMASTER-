using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    [Header("Music")]
    public AudioClip backgroundMusic;
    
    [Header("Sound Effects")]
    public AudioClip hitSound; // Sonido para hit normal
    public AudioClip destroySound; // Sonido para destruir objeto
    public AudioClip powerUpSound; // Sonido para recoger power-up
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.45f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.6f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        PlayBackgroundMusic();
    }
    
    void InitializeAudio()
    {
        // Si no hay AudioSources asignados, crearlos
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
        }
        
        // Configurar AudioSources
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
    }
    
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
            Debug.Log("Background music started");
        }
    }
    
    public void PlayHitSound()
    {
        if (hitSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(hitSound);
            Debug.Log("Hit sound played");
        }
    }
    
    public void PlayDestroySound()
    {
        if (destroySound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(destroySound);
            Debug.Log("Destroy sound played");
        }
    }
    
    public void PlayPowerUpSound()
    {
        if (powerUpSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(powerUpSound);
            Debug.Log("PowerUp sound played");
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }
    
    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
}