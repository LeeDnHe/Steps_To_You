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
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        Debug.Log("BoxingTTSManager initialized - waiting for external trigger");
    }
    
    /// <summary>
    /// 복싱 시작 전 TTS 재생 (BoxingInitializer에서 호출)
    /// </summary>
    /// <param name="onComplete">완료 시 호출할 콜백</param>
    public void StartPreGameTTS(System.Action onComplete = null)
    {
        if (preGameTTSList.Count > 0)
        {
            Debug.Log("Starting Pre-Game TTS");
            currentTTSCoroutine = StartCoroutine(PlayTTSSequence(preGameTTSList, onComplete));
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
    /// TTS 시퀀스를 순차적으로 재생하는 코루틴
    /// </summary>
    /// <param name="ttsClips">재생할 TTS 클립 리스트</param>
    /// <param name="onComplete">완료 시 호출할 콜백</param>
    /// <returns></returns>
    private IEnumerator PlayTTSSequence(List<AudioClip> ttsClips, System.Action onComplete)
    {
        isPlayingTTS = true;
        
        for (int i = 0; i < ttsClips.Count; i++)
        {
            AudioClip clip = ttsClips[i];
            if (clip != null && audioSource != null)
            {
                Debug.Log($"Playing TTS clip {i + 1}/{ttsClips.Count}: {clip.name}");
                
                // 오디오 재생
                audioSource.clip = clip;
                audioSource.Play();
                
                // 오디오가 끝날 때까지 대기
                yield return new WaitForSeconds(clip.length);
                
                // 다음 오디오 재생 전 0.5초 대기 (마지막 클립이 아닌 경우)
                if (i < ttsClips.Count - 1)
                {
                    yield return new WaitForSeconds(delayBetweenTTS);
                }
            }
        }
        
        isPlayingTTS = false;
        
        // 완료 콜백 호출
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
