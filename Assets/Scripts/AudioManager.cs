using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip menuMusic;     // Menu.mp3
    public AudioClip creditsMusic;  // Credits.mp3
    public AudioClip level1Music;   // Music1.ogg
    public AudioClip level2Music;   // Music2.mp3
    public AudioClip level3Music;   // Music3.mp3
    public AudioClip level4Music;   // Music4.mp3
    public AudioClip level5Music;   // Music5.mp3

    [Header("Sound Effects")]
    public AudioClip hitSound;
    public AudioClip destroySound;
    public AudioClip powerUpSound;
    public AudioClip wallsPlayerSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.45f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.6f;

    private AudioClip currentMusic;

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
        // No reproducir música automáticamente aquí
        // El SceneTransitionManager se encargará
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

    public void PlayMusicForScene(string sceneName)
    {
        AudioClip targetMusic = GetMusicForScene(sceneName);
        
        if (targetMusic != null && targetMusic != currentMusic)
        {
            currentMusic = targetMusic;
            
            if (musicSource != null)
            {
                musicSource.clip = targetMusic;
                musicSource.Play();
                Debug.Log($"Playing music for scene: {sceneName} - {targetMusic.name}");
            }
        }
    }

    private AudioClip GetMusicForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Menu":
                return menuMusic;
            case "Credtis": // Mantienes el typo original
                return creditsMusic;
            case "lvl1":
                return level1Music;
            case "lvl2":
                return level2Music;
            case "lvl3":
                return level3Music;
            case "lvl4":
                return level4Music;
            case "lvl5":
                return level5Music;
            default:
                Debug.LogWarning($"No music defined for scene: {sceneName}. Using menu music as fallback.");
                return menuMusic;
        }
    }

    public void PlayBackgroundMusic()
    {
        // Método mantenido para compatibilidad, pero ahora usa PlayMusicForScene
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        PlayMusicForScene(currentScene);
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

    public void PlayWallsPlayerSound()
    {
        if (wallsPlayerSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(wallsPlayerSound);
            Debug.Log("Walls/Player bounce sound played");
        }
        else
        {
            Debug.LogWarning("wallsPlayerSound or sfxSource is null!");
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
            currentMusic = null;
        }
    }

    public void FadeOutMusic(float duration = 1f)
    {
        if (musicSource != null)
        {
            StartCoroutine(FadeOutCoroutine(duration));
        }
    }

    private System.Collections.IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentVolume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            musicSource.volume = currentVolume;
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume; // Restaurar volumen original
    }
}