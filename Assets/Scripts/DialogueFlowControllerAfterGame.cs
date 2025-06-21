using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DialogueFlowControllerAfterGame : MonoBehaviour
{
    [Header("Game Result Input")]
    public int gameScore = 0; // 복싱 게임 점수 (BoxingInitializer에서 설정)
    public int minScoreForSuccess = 100; // 성공 기준 점수
    
    [Header("TTS Audio Lists")]
    public List<AudioClip> successInitialTTSList = new List<AudioClip>(); // 게임 성공 시 초기 TTS
    public List<AudioClip> failureInitialTTSList = new List<AudioClip>(); // 게임 실패 시 초기 TTS
    public List<AudioClip> intermediateTTSList = new List<AudioClip>();   // 중간 TTS (여주인공 이동 후)
    public List<AudioClip> firstChoice1TTSList = new List<AudioClip>();   // 첫 번째 선택지 1
    public List<AudioClip> firstChoice2TTSList = new List<AudioClip>();   // 첫 번째 선택지 2
    public List<AudioClip> firstChoice3TTSList = new List<AudioClip>();   // 첫 번째 선택지 3
    public List<AudioClip> lastTTSList = new List<AudioClip>();          // 마지막 TTS (첫 번째 선택 후)
    public List<AudioClip> secondChoice1TTSList = new List<AudioClip>();  // 두 번째 선택지 1
    public List<AudioClip> secondChoice2TTSList = new List<AudioClip>();  // 두 번째 선택지 2
    public List<AudioClip> secondChoice3TTSList = new List<AudioClip>();  // 두 번째 선택지 3
    
    [Header("Audio Source")]
    public AudioSource audioSource;
    
    [Header("Various Audio Controller")]
    public VariousAudioController variousAudioController; // 오디오 관리 컨트롤러
    
    [Header("UI Panels")]
    public GameObject firstChoicePanel;  // 첫 번째 선택지 패널 (버튼 1,2,3)
    public GameObject secondChoicePanel; // 두 번째 선택지 패널 (버튼 1,2,3)
    
    [Header("Character Settings")]
    public GameObject heroineCharacter; // 여주인공 캐릭터
    public Transform heroineEndPosition;   // 여주인공 목적지 위치
    public float heroineWalkDuration = 1.0f; // 여주인공 이동 시간
    public Animator heroineAnimator; // 여주인공 애니메이터
    
    [Header("Managers")]
    public BranchingDialogue branchingDialogue; // 호감도 점수 관리
    
    [Header("Next Scene Settings")]
    public GameObject nextDialogueController; // 다음 대화 컨트롤러
    
    [Header("White Background Transition")]
    public GameObject whiteBackgroundObject; // 흰색 배경 GameObject
    
    [Header("Background Sound Manager")]
    public BackgroundSoundManager backgroundSoundManager; // 배경음악 매니저
    
    [Header("Settings")]
    public float delayBetweenTTS = 0.5f;
    
    // Internal variables
    private bool isPlayingTTS = false;
    private int firstSelectedChoice = -1;
    private int secondSelectedChoice = -1;
    private bool gameSuccessful = false;
    private bool forceFailure = false; // 강제 실패 플래그 (두 번째 선택지 1번 선택 시)
    private float originalAmbientIntensity; // 원래 감마값 저장용
    
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
        
        // BranchingDialogue 초기화
        if (branchingDialogue == null)
        {
            branchingDialogue = FindObjectOfType<BranchingDialogue>();
        }
        
        // 배경음악 매니저 초기화
        if (backgroundSoundManager == null)
        {
            backgroundSoundManager = BackgroundSoundManager.Instance;
        }
        
        // 여주인공 애니메이터 초기화 (수동 할당 우선, 없으면 자동으로 찾기)
        if (heroineAnimator == null && heroineCharacter != null)
        {
            heroineAnimator = heroineCharacter.GetComponent<Animator>();
        }
        
        // 흰색 배경 초기화 (비활성화)
        if (whiteBackgroundObject != null)
        {
            whiteBackgroundObject.SetActive(false);
        }
        
        // 원래 감마값 저장
        originalAmbientIntensity = RenderSettings.ambientIntensity;
        
        // 모든 패널 비활성화
        SetAllPanelsInactive();
        
        // 게임 성공/실패 판정
        gameSuccessful = gameScore >= minScoreForSuccess;
        
        // 대화 시퀀스 시작
        StartCoroutine(StartDialogueSequence());
    }
    
    void SetAllPanelsInactive()
    {
        if (firstChoicePanel != null) firstChoicePanel.SetActive(false);
        if (secondChoicePanel != null) secondChoicePanel.SetActive(false);
        if (nextDialogueController != null) nextDialogueController.SetActive(false);
    }
    
    /// <summary>
    /// 전체 대화 시퀀스 시작
    /// </summary>
    IEnumerator StartDialogueSequence()
    {
        // 1. 게임 점수에 따른 초기 TTS 재생
        yield return StartCoroutine(PlayInitialTTS());
        
        // 2. 여주인공 캐릭터 1초 동안 걸어오기
        yield return StartCoroutine(MoveHeroineCharacter());
        
        // 3. 중간 TTS 재생
        string[] intermediateAnimations = new string[] { "take_31", "take_32", "take_33" };
        yield return StartCoroutine(PlayTTSListWithAnimation(intermediateTTSList, intermediateAnimations));
        
        // 4. 첫 번째 선택지 등장
        yield return StartCoroutine(ShowFirstChoicePanel());
        
        // 5. 첫 번째 선택에 따른 TTS 재생
        yield return StartCoroutine(PlayFirstChoiceTTS());
        
        // 6. 마지막 TTS 재생
        string[] lastAnimations = new string[] { "take_39_01", "take_39" };
        yield return StartCoroutine(PlayTTSListWithAnimation(lastTTSList, lastAnimations));
        
        // 7. 두 번째 선택지 등장
        yield return StartCoroutine(ShowSecondChoicePanel());
        
        // 8. 두 번째 선택에 따른 TTS 재생
        yield return StartCoroutine(PlaySecondChoiceTTS());
        
        // 9. 최종 점수 계산 및 다음 단계로 이동
        yield return StartCoroutine(CalculateFinalScoreAndProceed());
    }
    
    /// <summary>
    /// 게임 결과에 따른 초기 TTS 재생
    /// </summary>
    IEnumerator PlayInitialTTS()
    {
        List<AudioClip> initialTTSList = gameSuccessful ? successInitialTTSList : failureInitialTTSList;
        string[] animationNames = gameSuccessful ? new string[] { "take_29" } : new string[] { "take_30" };
        
        if (initialTTSList.Count > 0)
        {
            Debug.Log($"Playing initial TTS - Game {(gameSuccessful ? "Success" : "Failure")}");
            yield return StartCoroutine(PlayTTSListWithAnimation(initialTTSList, animationNames));
        }
    }
    
    /// <summary>
    /// 여주인공 캐릭터 이동 (현재 위치에서 목적지로)
    /// </summary>
    IEnumerator MoveHeroineCharacter()
    {
        if (heroineCharacter != null && heroineEndPosition != null)
        {
            Debug.Log("Moving heroine character from current position");
            
            // 현재 위치를 시작점으로 사용
            Vector3 startPos = heroineCharacter.transform.position;
            Vector3 endPos = heroineEndPosition.position;
            Quaternion endRot = heroineEndPosition.rotation;
            
            // 목적지 방향으로 즉시 회전 (자연스러운 이동을 위해)
            Vector3 direction = (endPos - startPos).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                heroineCharacter.transform.rotation = lookRotation;
                Debug.Log("Heroine rotated to face destination");
            }
            
            // 걷기 애니메이션 시작 (애니메이터가 있다면)
            if (heroineAnimator != null)
            {
                heroineAnimator.SetBool("IsWalking", true);
            }
            
            // 이동 (회전은 목적지 도착 후 최종 회전으로 처리)
            float elapsedTime = 0;
            
            while (elapsedTime < heroineWalkDuration)
            {
                float t = elapsedTime / heroineWalkDuration;
                heroineCharacter.transform.position = Vector3.Lerp(startPos, endPos, t);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 최종 위치 설정
            heroineCharacter.transform.position = endPos;
            
            // 플레이어를 향해 자연스럽게 회전 (XR Origin 또는 카메라 방향으로)
            if (Camera.main != null)
            {
                Vector3 playerPosition = Camera.main.transform.position;
                Vector3 directionToPlayer = (playerPosition - heroineCharacter.transform.position).normalized;
                directionToPlayer.y = 0; // Y축 회전만 적용 (수평 회전)
                
                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion lookAtPlayer = Quaternion.LookRotation(directionToPlayer);
                    heroineCharacter.transform.rotation = lookAtPlayer;
                    Debug.Log("Heroine rotated to face player");
                }
            }
            
            // 걷기 애니메이션 종료 (애니메이터가 있다면)
            if (heroineAnimator != null)
            {
                heroineAnimator.SetBool("IsWalking", false);
            }
            
            Debug.Log("Heroine character movement completed");
        }
        else
        {
            Debug.LogWarning("Heroine character or end position not set");
        }
    }
    
    /// <summary>
    /// 첫 번째 선택지 패널 표시
    /// </summary>
    IEnumerator ShowFirstChoicePanel()
    {
        // 선택지 등장 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceAppear();
        }
        
        // 첫 번째 선택지 패널 활성화
        if (firstChoicePanel != null)
        {
            firstChoicePanel.SetActive(true);
        }
        
        // 선택 대기
        yield return new WaitUntil(() => firstSelectedChoice != -1);
        
        // 선택지 클릭 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceClick();
        }
        
        // 패널 비활성화
        if (firstChoicePanel != null)
        {
            firstChoicePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 두 번째 선택지 패널 표시
    /// </summary>
    IEnumerator ShowSecondChoicePanel()
    {
        // 선택지 등장 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceAppear();
        }
        
        // 두 번째 선택지 패널 활성화
        if (secondChoicePanel != null)
        {
            secondChoicePanel.SetActive(true);
        }
        
        // 선택 대기
        yield return new WaitUntil(() => secondSelectedChoice != -1);
        
        // 선택지 클릭 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceClick();
        }
        
        // 패널 비활성화
        if (secondChoicePanel != null)
        {
            secondChoicePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 첫 번째 선택에 따른 TTS 재생
    /// </summary>
    IEnumerator PlayFirstChoiceTTS()
    {
        List<AudioClip> selectedTTSList = null;
        string[] animationNames = null;
        
        switch (firstSelectedChoice)
        {
            case 1:
                selectedTTSList = firstChoice1TTSList;
                animationNames = new string[] { "take_34", "take_35" };
                break;
            case 2:
                selectedTTSList = firstChoice2TTSList;
                animationNames = new string[] { "take_36", "take_37" };
                break;
            case 3:
                selectedTTSList = firstChoice3TTSList;
                animationNames = new string[] { "take_38" };
                break;
        }
        
        if (selectedTTSList != null && selectedTTSList.Count > 0)
        {
            Debug.Log($"Playing first choice {firstSelectedChoice} TTS");
            yield return StartCoroutine(PlayTTSListWithAnimation(selectedTTSList, animationNames));
        }
    }
    
    /// <summary>
    /// 두 번째 선택에 따른 TTS 재생
    /// </summary>
    IEnumerator PlaySecondChoiceTTS()
    {
        List<AudioClip> selectedTTSList = null;
        string[] animationNames = null;
        
        switch (secondSelectedChoice)
        {
            case 1:
                selectedTTSList = secondChoice1TTSList;
                animationNames = new string[] { "take_41", "take_42", "take_43" };
                break;
            case 2:
                selectedTTSList = secondChoice2TTSList;
                animationNames = new string[] { "take_44", "take_45" };
                break;
            case 3:
                selectedTTSList = secondChoice3TTSList;
                animationNames = new string[] { "take_46" };
                break;
        }
        
        if (selectedTTSList != null && selectedTTSList.Count > 0)
        {
            Debug.Log($"Playing second choice {secondSelectedChoice} TTS");
            yield return StartCoroutine(PlayTTSListWithAnimation(selectedTTSList, animationNames));
        }
    }
    
    /// <summary>
    /// 최종 점수 계산 및 다음 단계 진행
    /// </summary>
    IEnumerator CalculateFinalScoreAndProceed()
    {
        // 호감도 점수 가져오기
        int affectionScore = 0;
        if (branchingDialogue != null)
        {
            affectionScore = branchingDialogue.GetCurrentScore();
        }
        
        // 최종 점수 계산
        int finalScore = gameScore + affectionScore;
        
        Debug.Log($"Final Score Calculation:");
        Debug.Log($"Game Score: {gameScore}");
        Debug.Log($"Affection Score: {affectionScore}");
        Debug.Log($"Final Score: {finalScore}");
        Debug.Log($"First Choice: {firstSelectedChoice}, Second Choice: {secondSelectedChoice}");
        
        // 잠시 대기
        yield return new WaitForSeconds(1.0f);
        
        // 흰색 배경 전환과 함께 다음 단계로 이동
        yield return StartCoroutine(TransitionToNextDialogue());
        
        Debug.Log("DialogueFlowControllerAfterGame completed");
    }
    
    /// <summary>
    /// 흰색 배경 전환과 함께 다음 대화로 이동
    /// </summary>
    IEnumerator TransitionToNextDialogue()
    {
        // 1. 흰색 배경 페이드인 (2초)
        yield return StartCoroutine(FadeWhiteBackground(0f, 1f, 2f));
        
        // 2. 배경음악 변경 (LastDialogue용 음악으로 변경)
        if (backgroundSoundManager != null)
        {
            // 기본 음악으로 변경 (필요시 다른 음악으로 변경 가능)
            backgroundSoundManager.PlayDefaultMusic();
            Debug.Log("Background music changed for last dialogue");
        }
        
        // 3. 다음 대화 컨트롤러 설정 (활성화만 하고 TTS는 아직 시작하지 않음)
        if (nextDialogueController != null)
        {
            // DialogueFlowControllerLast에 점수 정보 및 강제 실패 플래그 전달
            var lastController = nextDialogueController.GetComponent<DialogueFlowControllerLast>();
            if (lastController != null)
            {
                lastController.SetGameScore(gameScore);
                
                // 두 번째 선택지 1번 선택 시 강제 실패 설정
                if (secondSelectedChoice == 1)
                {
                    lastController.SetForceFailure(true);
                    Debug.Log("Force failure set due to second choice 1 selection");
                }
            }
            
            nextDialogueController.SetActive(true);
            Debug.Log("마지막 dialogue controller activated (TTS will start after white background transition)");
        }
        
        // 4. 흰색 배경 페이드아웃 (2초)
        yield return StartCoroutine(FadeWhiteBackground(1f, 0f, 2f));
        
        // 5. 흰색 화면 전환 완료 후 TTS 시작
        if (nextDialogueController != null)
        {
            var finalController = nextDialogueController.GetComponent<DialogueFlowControllerLast>();
            if (finalController != null)
            {
                finalController.StartFinalDialogueFlow();
                Debug.Log("Final dialogue flow started after white background transition completed");
            }
        }
        
        // 6. 현재 오브젝트 비활성화 (다음 파트로 넘어가므로)
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 흰색 배경 표시/숨김 효과 (CanvasGroup 알파값 + 감마값 페이드)
    /// </summary>
    /// <param name="startAlpha">시작 상태 (0: 숨김, 1: 표시)</param>
    /// <param name="endAlpha">끝 상태 (0: 숨김, 1: 표시)</param>
    /// <param name="duration">지속 시간</param>
    /// <returns></returns>
    IEnumerator FadeWhiteBackground(float startAlpha, float endAlpha, float duration)
    {
        if (whiteBackgroundObject == null) yield break;
        
        float elapsedTime = 0f;
        
        if (startAlpha < endAlpha) // 페이드 인 (0 -> 1)
        {
            Debug.Log("White background fade IN started");
            
            // 1. GameObject 활성화
            whiteBackgroundObject.SetActive(true);
            
            // 2. 감마값 0으로 설정 (완전 어둠)
            RenderSettings.ambientIntensity = 0f;
            Debug.Log("Gamma set to 0 (complete darkness)");
            
            // 3. 점차 증가 (어둠 -> 원래 밝기)
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                float smoothT = t * t * (3f - 2f * t); // 부드러운 곡선 보간
                
                RenderSettings.ambientIntensity = Mathf.Lerp(0f, originalAmbientIntensity, smoothT);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 4. 최종값 설정 (원래 밝기로 복원)
            RenderSettings.ambientIntensity = originalAmbientIntensity;
            Debug.Log($"White background fade IN completed - Gamma restored to {originalAmbientIntensity}");
        }
        else // 페이드 아웃 (1 -> 0)
        {
            Debug.Log("White background fade OUT started");
            
            // 1. GameObject는 이미 활성화되어 있음
            // 2. 현재 감마값에서 0으로 점차 감소 (밝음 -> 어둠)
            float currentGamma = RenderSettings.ambientIntensity;
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                float smoothT = t * t * (3f - 2f * t); // 부드러운 곡선 보간
                
                RenderSettings.ambientIntensity = Mathf.Lerp(currentGamma, 0f, smoothT);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 3. 감마값 0으로 설정 (완전 어둠)
            RenderSettings.ambientIntensity = 0f;
            Debug.Log("Gamma set to 0 (complete darkness)");
            
            // 4. GameObject 비활성화
            whiteBackgroundObject.SetActive(false);
            
            // 5. 감마값 원래대로 복원
            RenderSettings.ambientIntensity = originalAmbientIntensity;
            Debug.Log($"White background fade OUT completed - GameObject deactivated, Gamma restored to {originalAmbientIntensity}");
        }
    }
    
    /// <summary>
    /// TTS 리스트를 순차적으로 재생
    /// </summary>
    IEnumerator PlayTTSList(List<AudioClip> ttsList)
    {
        if (ttsList == null || ttsList.Count == 0)
            yield break;
            
        isPlayingTTS = true;
        
        for (int i = 0; i < ttsList.Count; i++)
        {
            AudioClip clip = ttsList[i];
            if (clip != null && audioSource != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                
                // 오디오가 끝날 때까지 대기
                yield return new WaitUntil(() => !audioSource.isPlaying);
                
                // 다음 오디오 재생 전 딜레이 (마지막이 아닌 경우)
                if (i < ttsList.Count - 1)
                {
                    yield return new WaitForSeconds(delayBetweenTTS);
                }
            }
        }
        
        isPlayingTTS = false;
    }
    
    /// <summary>
    /// TTS 리스트를 순차적으로 재생 (애니메이션 포함)
    /// </summary>
    IEnumerator PlayTTSListWithAnimation(List<AudioClip> ttsList, string[] animationNames)
    {
        if (ttsList == null || ttsList.Count == 0)
            yield break;
            
        isPlayingTTS = true;
        
        for (int i = 0; i < ttsList.Count; i++)
        {
            AudioClip clip = ttsList[i];
            if (clip != null && audioSource != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                
                // 해당하는 애니메이션 재생
                if (heroineAnimator != null && animationNames != null && i < animationNames.Length)
                {
                    heroineAnimator.Play(animationNames[i]);
                    Debug.Log($"Playing animation: {animationNames[i]}");
                }
                
                // 오디오가 끝날 때까지 대기
                yield return new WaitUntil(() => !audioSource.isPlaying);
                
                // 다음 오디오 재생 전 딜레이 (마지막이 아닌 경우)
                if (i < ttsList.Count - 1)
                {
                    yield return new WaitForSeconds(delayBetweenTTS);
                }
            }
        }
        
        isPlayingTTS = false;
    }
    
    /// <summary>
    /// 게임 점수 설정 (BoxingInitializer에서 호출)
    /// </summary>
    public void SetGameScore(int score)
    {
        gameScore = score;
        gameSuccessful = gameScore >= minScoreForSuccess;
        Debug.Log($"Game score set to: {gameScore}, Success: {gameSuccessful}");
    }
    
    // ===== 버튼 이벤트 함수들 =====
    
    /// <summary>
    /// 첫 번째 선택지 1 버튼 클릭
    /// </summary>
    public void OnFirstChoice1Selected()
    {
        firstSelectedChoice = 1;
        Debug.Log("First choice 1 selected");
    }
    
    /// <summary>
    /// 첫 번째 선택지 2 버튼 클릭
    /// </summary>
    public void OnFirstChoice2Selected()
    {
        firstSelectedChoice = 2;
        Debug.Log("First choice 2 selected");
    }
    
    /// <summary>
    /// 첫 번째 선택지 3 버튼 클릭
    /// </summary>
    public void OnFirstChoice3Selected()
    {
        firstSelectedChoice = 3;
        Debug.Log("First choice 3 selected");
    }
    
    /// <summary>
    /// 두 번째 선택지 1 버튼 클릭
    /// </summary>
    public void OnSecondChoice1Selected()
    {
        secondSelectedChoice = 1;
        Debug.Log("Second choice 1 selected");
    }
    
    /// <summary>
    /// 두 번째 선택지 2 버튼 클릭
    /// </summary>
    public void OnSecondChoice2Selected()
    {
        secondSelectedChoice = 2;
        Debug.Log("Second choice 2 selected");
    }
    
    /// <summary>
    /// 두 번째 선택지 3 버튼 클릭
    /// </summary>
    public void OnSecondChoice3Selected()
    {
        secondSelectedChoice = 3;
        Debug.Log("Second choice 3 selected");
    }
} 
