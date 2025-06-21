using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BoxingTTSManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    
    [Header("TTS Audio Lists")]
    [SerializeField] private List<AudioClip> preGameTTSList = new List<AudioClip>();
    [SerializeField] private List<AudioClip> easyPhaseTTSList = new List<AudioClip>();
    [SerializeField] private List<AudioClip> normalPhaseTTSList = new List<AudioClip>();
    [SerializeField] private List<AudioClip> hardPhaseTTSList = new List<AudioClip>();
    [SerializeField] private List<AudioClip> failureTTSList = new List<AudioClip>();
    
    [Header("Boxing Manager Reference")]
    public BoxingManager boxingManager;
    
    [Header("Settings")]
    public float delayBetweenTTS = 0.5f;
    
    // Internal variables
    private bool isPlayingTTS = false;
    private Coroutine currentTTSCoroutine;
    
    void Start()
    {
        // AudioSource 초기화
        Debug.Log("BoxingTTSManager Start() called");
        if (audioSource == null)
        {
            Debug.Log("AudioSource is null, trying to get component");
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.Log("No AudioSource component found, adding one");
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        Debug.Log($"AudioSource initialized: {audioSource != null}");
        
        Debug.Log("BoxingTTSManager initialized - waiting for external trigger");
    }
    
    /// <summary>
    /// 복싱 시작 전 TTS 재생 (BoxingInitializer에서 호출)
    /// </summary>
    /// <param name="onComplete">완료 시 호출할 콜백</param>
    /// <param name="onClipStart">각 클립 시작 시 호출할 콜백 (클립 인덱스 전달)</param>
    public void StartPreGameTTS(System.Action onComplete = null, System.Action<int> onClipStart = null)
    {
        Debug.Log($"StartPreGameTTS called - preGameTTSList.Count: {preGameTTSList.Count}");
        
        // AudioSource 재확인 및 초기화
        if (audioSource == null)
        {
            Debug.Log("AudioSource is null in StartPreGameTTS, trying to initialize");
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.Log("Creating new AudioSource component");
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        Debug.Log($"AudioSource null? {audioSource == null}");
        Debug.Log($"GameObject active? {gameObject.activeInHierarchy}");
        
        // 오브젝트가 비활성화되어 있으면 활성화
        if (!gameObject.activeInHierarchy)
        {
            Debug.Log("BoxingTTS 오브젝트가 비활성화되어 있어 활성화합니다.");
            gameObject.SetActive(true);
        }
        
        if (preGameTTSList.Count > 0)
        {
            Debug.Log("Starting Pre-Game TTS");
            currentTTSCoroutine = StartCoroutine(PlayTTSSequenceWithCallback(preGameTTSList, onComplete, onClipStart));
        }
        else
        {
            Debug.Log("No Pre-Game TTS clips found, calling completion callback directly");
            onComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// 복싱 시작 후 Easy Phase TTS 재생
    /// </summary>
    public void StartEasyPhaseTTS()
    {
        if (easyPhaseTTSList.Count > 0)
        {
            Debug.Log("Starting Easy Phase TTS");
            currentTTSCoroutine = StartCoroutine(PlayTTSSequence(easyPhaseTTSList, null));
        }
    }
    
    /// <summary>
    /// 복싱 시작 후 Normal Phase TTS 재생
    /// </summary>
    public void StartNormalPhaseTTS()
    {
        if (normalPhaseTTSList.Count > 0)
        {
            Debug.Log("Starting Normal Phase TTS");
            currentTTSCoroutine = StartCoroutine(PlayTTSSequence(normalPhaseTTSList, null));
        }
    }
    
    /// <summary>
    /// 복싱 시작 후 Hard Phase TTS 재생
    /// </summary>
    public void StartHardPhaseTTS()
    {
        if (hardPhaseTTSList.Count > 0)
        {
            Debug.Log("Starting Hard Phase TTS");
            currentTTSCoroutine = StartCoroutine(PlayTTSSequence(hardPhaseTTSList, null));
        }
    }
    
    /// <summary>
    /// 실패 시 TTS 재생
    /// </summary>
    public void StartFailureTTS()
    {
        // 현재 재생 중인 TTS가 있다면 중단
        StopCurrentTTS();
        
        if (failureTTSList.Count > 0)
        {
            Debug.Log("Starting Failure TTS");
            currentTTSCoroutine = StartCoroutine(PlayTTSSequence(failureTTSList, null));
        }
    }
    
    /// <summary>
    /// TTS 시퀀스 재생
    /// </summary>
    IEnumerator PlayTTSSequence(List<AudioClip> ttsClips, System.Action onComplete = null)
    {
        Debug.Log($"Starting TTS sequence with {ttsClips.Count} clips");
        
        foreach (AudioClip clip in ttsClips)
        {
            if (clip != null && audioSource != null)
            {
                Debug.Log($"Playing TTS clip: {clip.name}");
                audioSource.clip = clip;
                audioSource.Play();
                
                // 클립 재생 완료까지 대기
                yield return new WaitForSeconds(clip.length);
            }
            else
            {
                Debug.LogWarning($"Skipping null clip or audioSource is null");
            }
        }
        
        // 모든 TTS 재생 완료 후 AudioSource 비우기
        if (audioSource != null)
        {
            audioSource.clip = null;
            audioSource.Stop();
            Debug.Log("AudioSource cleared after TTS sequence completion");
        }
        
        Debug.Log("TTS sequence completed");
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// TTS 시퀀스 재생 (클립 시작 콜백 포함)
    /// </summary>
    IEnumerator PlayTTSSequenceWithCallback(List<AudioClip> ttsClips, System.Action onComplete = null, System.Action<int> onClipStart = null)
    {
        Debug.Log($"Starting TTS sequence with callback - {ttsClips.Count} clips");
        
        for (int i = 0; i < ttsClips.Count; i++)
        {
            AudioClip clip = ttsClips[i];
            if (clip != null && audioSource != null)
            {
                Debug.Log($"Playing TTS clip {i}: {clip.name}");
                
                // 클립 시작 콜백 호출
                onClipStart?.Invoke(i);
                
                audioSource.clip = clip;
                audioSource.Play();
                
                // 클립 재생 완료까지 대기
                yield return new WaitForSeconds(clip.length);
            }
            else
            {
                Debug.LogWarning($"Skipping null clip {i} or audioSource is null");
            }
        }
        
        // 모든 TTS 재생 완료 후 AudioSource 비우기
        if (audioSource != null)
        {
            audioSource.clip = null;
            audioSource.Stop();
            Debug.Log("AudioSource cleared after TTS sequence completion");
        }
        
        Debug.Log("TTS sequence with callback completed");
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// 현재 재생 중인 TTS 중단
    /// </summary>
    public void StopCurrentTTS()
    {
        if (currentTTSCoroutine != null)
        {
            StopCoroutine(currentTTSCoroutine);
            currentTTSCoroutine = null;
        }
        
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        isPlayingTTS = false;
    }
    
    /// <summary>
    /// 현재 TTS가 재생 중인지 확인
    /// </summary>
    /// <returns></returns>
    public bool IsPlayingTTS()
    {
        return isPlayingTTS;
    }
    
    /// <summary>
    /// TTS 리스트에 오디오 클립 추가 (에디터에서 사용)
    /// </summary>
    public void AddPreGameTTS(AudioClip clip)
    {
        if (clip != null && !preGameTTSList.Contains(clip))
        {
            preGameTTSList.Add(clip);
        }
    }
    
    public void AddEasyPhaseTTS(AudioClip clip)
    {
        if (clip != null && !easyPhaseTTSList.Contains(clip))
        {
            easyPhaseTTSList.Add(clip);
        }
    }
    
    public void AddNormalPhaseTTS(AudioClip clip)
    {
        if (clip != null && !normalPhaseTTSList.Contains(clip))
        {
            normalPhaseTTSList.Add(clip);
        }
    }
    
    public void AddHardPhaseTTS(AudioClip clip)
    {
        if (clip != null && !hardPhaseTTSList.Contains(clip))
        {
            hardPhaseTTSList.Add(clip);
        }
    }
    
    public void AddFailureTTS(AudioClip clip)
    {
        if (clip != null && !failureTTSList.Contains(clip))
        {
            failureTTSList.Add(clip);
        }
    }
    
    /// <summary>
    /// 특정 TTS 리스트 클리어
    /// </summary>
    public void ClearPreGameTTS() { preGameTTSList.Clear(); }
    public void ClearEasyPhaseTTS() { easyPhaseTTSList.Clear(); }
    public void ClearNormalPhaseTTS() { normalPhaseTTSList.Clear(); }
    public void ClearHardPhaseTTS() { hardPhaseTTSList.Clear(); }
    public void ClearFailureTTS() { failureTTSList.Clear(); }
}
