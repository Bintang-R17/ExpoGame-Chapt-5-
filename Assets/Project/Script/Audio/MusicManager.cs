using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
    }

    [Header("Music Settings")]
    [SerializeField] private SceneMusic[] sceneMusicList;
    [SerializeField] private AudioClip defaultMusic;
    
    [Header("Volume Control")]
    [SerializeField] private float musicVolume = 0.4f;
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float fadeOutDuration = 1.5f;
    
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    private static MusicManager instance;
    private string currentSceneName;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        // Singleton pattern - only one MusicManager exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        // Ensure we mark the ROOT GameObject as persistent
        if (transform.parent != null)
        {
            Debug.LogWarning("MusicManager: GameObject is not root. Marking root GameObject as DontDestroyOnLoad.");
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }

        // Setup AudioSource if not assigned
        if (audioSource == null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Configure AudioSource
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = 0f; // Start silent for fade in

        // Subscribe to scene change events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // Play music for initial scene
        currentSceneName = SceneManager.GetActiveScene().name;
        PlayMusicForScene(currentSceneName, true);
    }

    void OnDestroy()
    {
        // Unsubscribe from scene events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string newSceneName = scene.name;
        
        // Only change music if scene actually changed
        if (newSceneName != currentSceneName)
        {
            currentSceneName = newSceneName;
            PlayMusicForScene(newSceneName, false);
        }
    }

    void PlayMusicForScene(string sceneName, bool immediate = false)
    {
        AudioClip targetClip = GetMusicForScene(sceneName);

        // If same music is playing, don't restart
        if (audioSource.clip == targetClip && audioSource.isPlaying)
        {
            return;
        }

        // Stop current fade if any
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (immediate)
        {
            // Immediate play (for initial scene)
            audioSource.clip = targetClip;
            if (targetClip != null)
            {
                fadeCoroutine = StartCoroutine(FadeIn());
            }
        }
        else
        {
            // Fade out current, then fade in new
            fadeCoroutine = StartCoroutine(CrossfadeMusic(targetClip));
        }
    }

    AudioClip GetMusicForScene(string sceneName)
    {
        // Search for scene-specific music
        foreach (SceneMusic sceneMusic in sceneMusicList)
        {
            if (sceneMusic.sceneName.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return sceneMusic.musicClip;
            }
        }

        // Return default music if no specific music found
        return defaultMusic;
    }

    IEnumerator FadeIn()
    {
        audioSource.Play();
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / fadeInDuration);
            yield return null;
        }

        audioSource.volume = musicVolume;
    }

    IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        // Fade out current music
        if (audioSource.isPlaying)
        {
            yield return StartCoroutine(FadeOut());
        }

        // Change clip
        audioSource.clip = newClip;

        // Fade in new music
        if (newClip != null)
        {
            yield return StartCoroutine(FadeIn());
        }
    }

    // Public methods for external control
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        audioSource.volume = musicVolume;
    }

    public void StopMusic(bool fadeOut = true)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (fadeOut)
        {
            fadeCoroutine = StartCoroutine(FadeOut());
        }
        else
        {
            audioSource.Stop();
            audioSource.volume = 0f;
        }
    }

    public void PlayMusic(AudioClip clip, bool fadeIn = true)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (fadeIn)
        {
            fadeCoroutine = StartCoroutine(CrossfadeMusic(clip));
        }
        else
        {
            audioSource.clip = clip;
            audioSource.volume = musicVolume;
            audioSource.Play();
        }
    }

    public void PauseMusic()
    {
        audioSource.Pause();
    }

    public void ResumeMusic()
    {
        audioSource.UnPause();
    }

    public static MusicManager Instance => instance;
}
