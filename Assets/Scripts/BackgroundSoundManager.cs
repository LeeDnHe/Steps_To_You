using UnityEngine;
using System.Collections;

public class BackgroundSoundManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource backgroundAudioSource; // 배경음악용 오디오소스
    
    [Header("Background Music Clips")]
    public AudioClip defaultBackgroundMusic;     // 기본 배경음악
    public AudioClip explanationPhaseMusic;     // 설명 및 이지페이즈 음악
    public AudioClip mainGameMusic;             // 본 게임 음악
    public AudioClip finalDialogueMusic;        // 마지막 대화 배경음악
    
    [Header("Settings")]
    public float fadeTime = 1.0f; // 페이드 시간
    public bool debugMode = false;
    
    private static BackgroundSoundManager instance;
    private AudioClip currentClip;
    private Coroutine fadeCoroutine;
    
    public static BackgroundSoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BackgroundSoundManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("BackgroundSoundManager");
                    instance = go.AddComponent<BackgroundSoundManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // AudioSource 초기화
        if (backgroundAudioSource == null)
        {
            backgroundAudioSource = GetComponent<AudioSource>();
            if (backgroundAudioSource == null)
            {
                backgroundAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // 기본 설정
        backgroundAudioSource.loop = true;
        backgroundAudioSource.playOnAwake = false;
    }
    
    void Start()
    {
        // 기본 배경음악 재생
        PlayDefaultMusic();
    }
    
    /// <summary>
    /// 기본 배경음악 재생
    /// </summary>
    public void PlayDefaultMusic()
    {
        PlayMusic(defaultBackgroundMusic, "Default Background Music");
    }
    
    /// <summary>
    /// 설명 및 이지페이즈 음악 재생
    /// </summary>
    public void PlayExplanationPhaseMusic()
    {
        PlayMusic(explanationPhaseMusic, "Explanation Phase Music");
    }
    
    /// <summary>
    /// 본 게임 음악 재생
    /// </summary>
    public void PlayMainGameMusic()
    {
        PlayMusic(mainGameMusic, "Main Game Music");
    }
    
    /// <summary>
    /// 마지막 대화 배경음악 재생
    /// </summary>
    public void PlayFinalDialogueMusic()
    {
        PlayMusic(finalDialogueMusic, "Final Dialogue Music");
    }
    
    /// <summary>
    /// 음악 재생 (페이드 효과 포함)
    /// </summary>
    /// <param name="newClip">재생할 음악 클립</param>
    /// <param name="musicName">음악 이름 (디버그용)</param>
    private void PlayMusic(AudioClip newClip, string musicName)
    {
        if (newClip == null)
        {
            if (debugMode)
                Debug.LogWarning($"Audio clip is null for: {musicName}");
            return;
        }
        
        // 같은 클립이면 재생하지 않음
        if (currentClip == newClip && backgroundAudioSource.isPlaying)
        {
            if (debugMode)
                Debug.Log($"Already playing: {musicName}");
            return;
        }
        
        // 기존 페이드 중단
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // 페이드 효과와 함께 음악 변경
        fadeCoroutine = StartCoroutine(FadeToNewMusic(newClip, musicName));
    }
    
    /// <summary>
    /// 새로운 음악으로 페이드 전환
    /// </summary>
    /// <param name="newClip">새로운 음악 클립</param>
    /// <param name="musicName">음악 이름</param>
    /// <returns></returns>
    private IEnumerator FadeToNewMusic(AudioClip newClip, string musicName)
    {
        float startVolume = backgroundAudioSource.volume;
        
        // 페이드 아웃
        if (backgroundAudioSource.isPlaying)
        {
            float elapsedTime = 0;
            while (elapsedTime < fadeTime)
            {
                backgroundAudioSource.volume = Mathf.Lerp(startVolume, 0, elapsedTime / fadeTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            backgroundAudioSource.volume = 0;
        }
        
        // 새로운 클립 설정 및 재생
        backgroundAudioSource.clip = newClip;
        currentClip = newClip;
        backgroundAudioSource.Play();
        
        if (debugMode)
            Debug.Log($"Started playing: {musicName}");
        
        // 페이드 인
        float elapsedTime2 = 0;
        while (elapsedTime2 < fadeTime)
        {
            backgroundAudioSource.volume = Mathf.Lerp(0, startVolume, elapsedTime2 / fadeTime);
            elapsedTime2 += Time.deltaTime;
            yield return null;
        }
        backgroundAudioSource.volume = startVolume;
        
        fadeCoroutine = null;
    }
    
    /// <summary>
    /// 음악 정지
    /// </summary>
    public void StopMusic()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeOutMusic());
    }
    
    /// <summary>
    /// 음악 페이드 아웃
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOutMusic()
    {
        float startVolume = backgroundAudioSource.volume;
        float elapsedTime = 0;
        
        while (elapsedTime < fadeTime)
        {
            backgroundAudioSource.volume = Mathf.Lerp(startVolume, 0, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        backgroundAudioSource.volume = 0;
        backgroundAudioSource.Stop();
        currentClip = null;
        
        if (debugMode)
            Debug.Log("Music stopped");
        
        fadeCoroutine = null;
    }
    
    /// <summary>
    /// 볼륨 설정
    /// </summary>
    /// <param name="volume">볼륨 (0~1)</param>
    public void SetVolume(float volume)
    {
        if (backgroundAudioSource != null)
        {
            backgroundAudioSource.volume = Mathf.Clamp01(volume);
        }
    }
    
    /// <summary>
    /// 현재 재생 중인 음악 확인
    /// </summary>
    /// <returns>현재 음악 클립</returns>
    public AudioClip GetCurrentMusic()
    {
        return currentClip;
    }
    
    /// <summary>
    /// 음악이 재생 중인지 확인
    /// </summary>
    /// <returns>재생 중이면 true</returns>
    public bool IsPlaying()
    {
        return backgroundAudioSource != null && backgroundAudioSource.isPlaying;
    }
} 