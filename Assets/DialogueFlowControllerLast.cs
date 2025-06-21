using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueFlowControllerLast : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource ttsAudioSource; // TTS 재생용 오디오 소스
    
    [Header("Success TTS")]
    public AudioClip[] successTTSClips; // 성공 TTS 리스트 (5개)
    
    [Header("Failure TTS")]
    public AudioClip[] failureTTSClips; // 실패 TTS 리스트 (3개)
    
    [Header("Success Dialogue Panels")]
    public GameObject successDialoguePanel1; // 성공 대화 패널 1
    public GameObject successDialoguePanel2; // 성공 대화 패널 2
    public GameObject successDialoguePanel3; // 성공 대화 패널 3
    
    [Header("Failure Dialogue Panels")]
    public GameObject failureDialoguePanel1; // 실패 대화 패널 1
    public GameObject failureDialoguePanel2; // 실패 대화 패널 2
    public GameObject failureDialoguePanel3; // 실패 대화 패널 3
    
    [Header("References")]
    public BranchingDialogue branchingDialogue; // 호감도 참조
    public VariousAudioController variousAudioController; // 오디오 컨트롤러
    public BackgroundSoundManager backgroundSoundManager; // 배경음악 매니저
    
    [Header("Character Animation")]
    public Animator heroineAnimator; // 여주인공 애니메이터
    
    [Header("Settings")]
    public int successThreshold = 200; // 성공 기준 점수
    public bool debugMode = false; // 디버그 모드
    
    // Private variables
    private int gameScore = 0; // 게임 점수
    private int affectionScore = 0; // 호감도 점수
    private int totalScore = 0; // 총합 점수
    private bool isSuccess = false; // 성공 여부
    private bool isFlowStarted = false; // 플로우 시작 여부
    private bool forceFailure = false; // 강제 실패 플래그
    
    // 패널 클릭 상태 변수들
    private bool successPanel1Clicked = false;
    private bool successPanel2Clicked = false;
    private bool successPanel3Clicked = false;
    private bool failurePanel1Clicked = false;
    private bool failurePanel2Clicked = false;
    private bool failurePanel3Clicked = false;
    
    void Start()
    {
        // 모든 패널 초기 비활성화
        HideAllPanels();
        
        // TTS 오디오 소스 설정 확인
        if (ttsAudioSource == null)
        {
            ttsAudioSource = GetComponent<AudioSource>();
            if (ttsAudioSource == null)
            {
                ttsAudioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("DialogueFlowControllerLast: AudioSource component added automatically");
            }
            else
            {
                Debug.Log("DialogueFlowControllerLast: AudioSource found and assigned");
            }
        }
    }
    
    void OnEnable()
    {
        // GameObject가 활성화될 때는 자동 시작하지 않음 (명시적으로 StartFinalDialogueFlow 호출 대기)
        Debug.Log("DialogueFlowControllerLast enabled - waiting for explicit StartFinalDialogueFlow call");
    }
    

    
    /// <summary>
    /// 게임 점수 설정 (DialogueFlowControllerAfterGame에서 호출)
    /// </summary>
    /// <param name="score">게임 점수</param>
    public void SetGameScore(int score)
    {
        gameScore = score;
        
        if (debugMode)
        {
            Debug.Log($"DialogueFlowControllerLast: Game score set to {gameScore}");
        }
    }
    
    /// <summary>
    /// 강제 실패 플래그 설정 (DialogueFlowControllerAfterGame에서 호출)
    /// </summary>
    /// <param name="force">강제 실패 여부</param>
    public void SetForceFailure(bool force)
    {
        forceFailure = force;
        
        if (debugMode)
        {
            Debug.Log($"DialogueFlowControllerLast: Force failure set to {forceFailure}");
        }
    }
    
    /// <summary>
    /// 최종 대화 플로우 시작
    /// </summary>
    public void StartFinalDialogueFlow()
    {
        Debug.Log("=== DialogueFlowControllerLast: StartFinalDialogueFlow called ===");
        
        if (isFlowStarted)
        {
            Debug.LogWarning("Final dialogue flow already started!");
            return;
        }
        
        isFlowStarted = true;
        
        // 마지막 대화 배경음악 재생
        if (backgroundSoundManager != null)
        {
            backgroundSoundManager.PlayFinalDialogueMusic();
            Debug.Log("Final dialogue background music started");
        }
        else
        {
            // BackgroundSoundManager 인스턴스 찾기 시도
            backgroundSoundManager = BackgroundSoundManager.Instance;
            if (backgroundSoundManager != null)
            {
                backgroundSoundManager.PlayFinalDialogueMusic();
                Debug.Log("Final dialogue background music started (found via Instance)");
            }
            else
            {
                Debug.LogWarning("BackgroundSoundManager not found - final dialogue music will not play");
            }
        }
        
        // 호감도 점수 가져오기
        if (branchingDialogue != null)
        {
            affectionScore = branchingDialogue.GetCurrentScore();
            Debug.Log($"Affection score retrieved: {affectionScore}");
        }
        else
        {
            Debug.LogWarning("BranchingDialogue is null - affection score will be 0");
        }
        
        // 총합 점수 계산
        totalScore = gameScore + affectionScore;
        
        // 성공/실패 판정 (강제 실패 플래그 확인)
        if (forceFailure)
        {
            isSuccess = false;
            Debug.Log("Force failure applied - ignoring score");
        }
        else
        {
            isSuccess = totalScore >= successThreshold;
        }
        
        Debug.Log($"=== DialogueFlowControllerLast: Final Flow Started ===");
        Debug.Log($"Game Score: {gameScore}, Affection Score: {affectionScore}");
        Debug.Log($"Total Score: {totalScore}, Success Threshold: {successThreshold}");
        Debug.Log($"Force Failure: {forceFailure}, Final Success: {isSuccess}");
        Debug.Log($"TTS AudioSource: {ttsAudioSource != null}");
        
        // 성공/실패에 따른 플로우 시작
        if (isSuccess)
        {
            Debug.Log("Starting Success Flow...");
            StartCoroutine(SuccessFlow());
        }
        else
        {
            Debug.Log("Starting Failure Flow...");
            StartCoroutine(FailureFlow());
        }
    }
    
    /// <summary>
    /// 성공 플로우
    /// </summary>
    IEnumerator SuccessFlow()
    {
        if (debugMode)
            Debug.Log("Starting Success Flow");
        
        // 1. TTS 1~3번 재생 (애니메이션 포함)
        string[] successAnimations = new string[] { "take_47", "take_48", "take_49" };
        for (int i = 0; i < 3; i++)
        {
            if (i < successTTSClips.Length && successTTSClips[i] != null)
            {
                string animationName = i < successAnimations.Length ? successAnimations[i] : "";
                yield return StartCoroutine(PlayTTSWithAnimation(successTTSClips[i], $"Success TTS {i + 1}", animationName));
            }
        }
        
        // 2. 성공 대화 패널 1 표시
        ShowPanel(successDialoguePanel1);
        
        // 3. 패널 1 클릭 대기
        successPanel1Clicked = false;
        yield return new WaitUntil(() => successPanel1Clicked);
        HidePanel(successDialoguePanel1);
        
        // 4. 성공 대화 패널 2 표시
        ShowPanel(successDialoguePanel2);
        
        // 5. 패널 2 클릭 대기
        successPanel2Clicked = false;
        yield return new WaitUntil(() => successPanel2Clicked);
        HidePanel(successDialoguePanel2);
        
        // 6. TTS 4번 재생 (애니메이션 포함)
        if (successTTSClips.Length > 3 && successTTSClips[3] != null)
        {
            yield return StartCoroutine(PlayTTSWithAnimation(successTTSClips[3], "Success TTS 4", "take_50"));
        }
        
        // 7. 성공 대화 패널 3 표시
        ShowPanel(successDialoguePanel3);
        
        // 8. 패널 3 클릭 대기
        successPanel3Clicked = false;
        yield return new WaitUntil(() => successPanel3Clicked);
        HidePanel(successDialoguePanel3);
        
        // 9. TTS 5번 재생 (애니메이션 포함)
        if (successTTSClips.Length > 4 && successTTSClips[4] != null)
        {
            yield return StartCoroutine(PlayTTSWithAnimation(successTTSClips[4], "Success TTS 5", "take_51"));
        }
        
        if (debugMode)
            Debug.Log("Success Flow Completed");
    }
    
    /// <summary>
    /// 실패 플로우
    /// </summary>
    IEnumerator FailureFlow()
    {
        if (debugMode)
            Debug.Log("Starting Failure Flow");
        
        // 1. 실패 대화 패널 1 표시
        ShowPanel(failureDialoguePanel1);
        
        // 2. 패널 1 클릭 대기
        failurePanel1Clicked = false;
        yield return new WaitUntil(() => failurePanel1Clicked);
        HidePanel(failureDialoguePanel1);
        
        // 3. 실패 대화 패널 2 표시
        ShowPanel(failureDialoguePanel2);
        
        // 4. 패널 2 클릭 대기
        failurePanel2Clicked = false;
        yield return new WaitUntil(() => failurePanel2Clicked);
        HidePanel(failureDialoguePanel2);
        
        // 5. TTS 1번 재생 (애니메이션 포함)
        if (failureTTSClips.Length > 0 && failureTTSClips[0] != null)
        {
            yield return StartCoroutine(PlayTTSWithAnimation(failureTTSClips[0], "Failure TTS 1", "take_52"));
        }
        
        // 6. 실패 대화 패널 3 표시
        ShowPanel(failureDialoguePanel3);
        
        // 7. 패널 3 클릭 대기
        failurePanel3Clicked = false;
        yield return new WaitUntil(() => failurePanel3Clicked);
        HidePanel(failureDialoguePanel3);
        
        // 8. TTS 2번, 3번 연속 재생 (애니메이션 포함)
        string[] failureAnimations = new string[] { "", "take_53", "take_54" }; // 인덱스 맞추기 위해 첫 번째는 빈 문자열
        for (int i = 1; i < 3; i++)
        {
            if (i < failureTTSClips.Length && failureTTSClips[i] != null)
            {
                string animationName = i < failureAnimations.Length ? failureAnimations[i] : "";
                yield return StartCoroutine(PlayTTSWithAnimation(failureTTSClips[i], $"Failure TTS {i + 1}", animationName));
            }
        }
        
        if (debugMode)
            Debug.Log("Failure Flow Completed");
    }
    
    /// <summary>
    /// TTS 재생
    /// </summary>
    IEnumerator PlayTTS(AudioClip clip, string clipName)
    {
        if (ttsAudioSource == null || clip == null)
        {
            Debug.LogWarning($"Cannot play TTS: {clipName} - AudioSource: {ttsAudioSource != null}, Clip: {clip != null}");
            yield break;
        }
        
        Debug.Log($"Playing TTS: {clipName} (Length: {clip.length}s)");
        
        ttsAudioSource.clip = clip;
        ttsAudioSource.Play();
        
        // TTS 재생 완료까지 대기
        yield return new WaitForSeconds(clip.length);
        
        Debug.Log($"TTS completed: {clipName}");
    }
    
    /// <summary>
    /// TTS 재생 (애니메이션 포함)
    /// </summary>
    IEnumerator PlayTTSWithAnimation(AudioClip clip, string clipName, string animationName)
    {
        if (ttsAudioSource == null || clip == null)
        {
            Debug.LogWarning($"Cannot play TTS: {clipName} - AudioSource: {ttsAudioSource != null}, Clip: {clip != null}");
            yield break;
        }
        
        Debug.Log($"Playing TTS: {clipName} (Length: {clip.length}s) with animation: {animationName}");
        
        ttsAudioSource.clip = clip;
        ttsAudioSource.Play();
        
        // 해당하는 애니메이션 재생
        if (heroineAnimator != null && !string.IsNullOrEmpty(animationName))
        {
            heroineAnimator.Play(animationName);
            Debug.Log($"Playing animation: {animationName}");
        }
        
        // TTS 재생 완료까지 대기
        yield return new WaitForSeconds(clip.length);
        
        Debug.Log($"TTS completed: {clipName}");
    }
    
    /// <summary>
    /// 패널 표시
    /// </summary>
    void ShowPanel(GameObject panel)
    {
        if (panel == null) return;
        
        HideAllPanels();
        panel.SetActive(true);
        
        // 선택지 등장 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceAppear();
        }
        
        if (debugMode)
            Debug.Log($"Panel shown: {panel.name}");
    }
    
    /// <summary>
    /// 패널 숨기기
    /// </summary>
    void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
            
            if (debugMode)
                Debug.Log($"Panel hidden: {panel.name}");
        }
    }
    
    // ===== 버튼 이벤트 함수들 (Unity Inspector에서 할당) =====
    
    /// <summary>
    /// 성공 대화 패널 1 클릭 이벤트
    /// </summary>
    public void OnSuccessPanel1Clicked()
    {
        successPanel1Clicked = true;
        
        // 선택지 클릭 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceClick();
        }
        
        if (debugMode)
            Debug.Log("Success Panel 1 clicked");
    }
    
    /// <summary>
    /// 성공 대화 패널 2 클릭 이벤트
    /// </summary>
    public void OnSuccessPanel2Clicked()
    {
        successPanel2Clicked = true;
        
        // 선택지 클릭 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceClick();
        }
        
        if (debugMode)
            Debug.Log("Success Panel 2 clicked");
    }
    
    /// <summary>
    /// 성공 대화 패널 3 클릭 이벤트
    /// </summary>
    public void OnSuccessPanel3Clicked()
    {
        successPanel3Clicked = true;
        
        // 선택지 클릭 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceClick();
        }
        
        if (debugMode)
            Debug.Log("Success Panel 3 clicked");
    }
    
    /// <summary>
    /// 실패 대화 패널 1 클릭 이벤트
    /// </summary>
    public void OnFailurePanel1Clicked()
    {
        failurePanel1Clicked = true;
        
        // 선택지 클릭 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceClick();
        }
        
        if (debugMode)
            Debug.Log("Failure Panel 1 clicked");
    }
    
    /// <summary>
    /// 실패 대화 패널 2 클릭 이벤트
    /// </summary>
    public void OnFailurePanel2Clicked()
    {
        failurePanel2Clicked = true;
        
        // 선택지 클릭 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceClick();
        }
        
        if (debugMode)
            Debug.Log("Failure Panel 2 clicked");
    }
    
    /// <summary>
    /// 실패 대화 패널 3 클릭 이벤트
    /// </summary>
    public void OnFailurePanel3Clicked()
    {
        failurePanel3Clicked = true;
        
        // 선택지 클릭 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceClick();
        }
        
        if (debugMode)
            Debug.Log("Failure Panel 3 clicked");
    }
    
    /// <summary>
    /// 모든 패널 숨기기
    /// </summary>
    void HideAllPanels()
    {
        if (successDialoguePanel1 != null) successDialoguePanel1.SetActive(false);
        if (successDialoguePanel2 != null) successDialoguePanel2.SetActive(false);
        if (successDialoguePanel3 != null) successDialoguePanel3.SetActive(false);
        if (failureDialoguePanel1 != null) failureDialoguePanel1.SetActive(false);
        if (failureDialoguePanel2 != null) failureDialoguePanel2.SetActive(false);
        if (failureDialoguePanel3 != null) failureDialoguePanel3.SetActive(false);
    }
}
