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
    public Transform playerTransform; // 플레이어 Transform (직접 할당)
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
        
        // 흰색 배경 초기화 (항상 활성화, 알파값 0)
        if (whiteBackgroundObject != null)
        {
            whiteBackgroundObject.SetActive(true);
            
            // 하얀색 이미지 컴포넌트 찾아서 알파값 0으로 설정
            UnityEngine.UI.Image whiteImage = whiteBackgroundObject.GetComponent<UnityEngine.UI.Image>();
            if (whiteImage == null)
            {
                whiteImage = whiteBackgroundObject.GetComponentInChildren<UnityEngine.UI.Image>();
            }
            
            if (whiteImage != null)
            {
                Color color = whiteImage.color;
                color.a = 0f;
                whiteImage.color = color;
                Debug.Log("White background initialized - always active with alpha 0");
            }
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
    /// 여주인공 캐릭터 이동 (현재 위치에서 플레이어 기준 상대 위치로, 이동 중 플레이어를 바라보며)
    /// </summary>
    IEnumerator MoveHeroineCharacter()
    {
        if (heroineCharacter != null)
        {
            Debug.Log("Moving heroine character from current position to player-relative position");
            
            // 현재 위치를 시작점으로 사용
            Vector3 startPos = heroineCharacter.transform.position;
            
            // 플레이어 위치 가져오기 (직접 할당된 Transform 사용)
            Vector3 playerPos = playerTransform != null ? playerTransform.position : Vector3.zero;
            
            // 플레이어 기준 x축 + 0.7f 위치를 목적지로 설정
            Vector3 endPos = new Vector3(
                playerPos.x + 0.7f,  // x축으로 +0.7f
                playerPos.y,         // y축은 플레이어와 동일
                playerPos.z          // z축은 플레이어와 동일
            );
            
            // 걷기 애니메이션 시작 (애니메이터가 있다면)
            if (heroineAnimator != null)
            {
                heroineAnimator.SetBool("IsWalking", true);
            }
            
            // 이동하면서 지속적으로 플레이어를 바라보기
            float elapsedTime = 0;
            
            while (elapsedTime < heroineWalkDuration)
            {
                float t = elapsedTime / heroineWalkDuration;
                
                // 위치 보간
                heroineCharacter.transform.position = Vector3.Lerp(startPos, endPos, t);
                
                // 실시간으로 플레이어 방향으로 회전 (이동 중에도 계속 플레이어를 바라봄)
                Vector3 currentPlayerPos = playerTransform != null ? playerTransform.position : Vector3.zero;
                
                // 카메라가 있다면 카메라 위치 사용, 없다면 playerTransform 위치 사용
                Vector3 targetLookPos = Camera.main != null ? Camera.main.transform.position : currentPlayerPos;
                
                Vector3 directionToPlayer = (targetLookPos - heroineCharacter.transform.position).normalized;
                directionToPlayer.y = 0; // Y축 회전만 적용 (수평 회전)
                
                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion lookAtPlayer = Quaternion.LookRotation(directionToPlayer);
                    // 부드러운 회전을 위해 Slerp 사용
                    heroineCharacter.transform.rotation = Quaternion.Slerp(
                        heroineCharacter.transform.rotation, 
                        lookAtPlayer, 
                        Time.deltaTime * 5f // 회전 속도 조절
                    );
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 최종 위치 설정
            heroineCharacter.transform.position = endPos;
            
            // 최종적으로 플레이어를 정확히 바라보도록 설정
            Vector3 finalPlayerPos = playerTransform != null ? playerTransform.position : Vector3.zero;
            Vector3 finalTargetLookPos = Camera.main != null ? Camera.main.transform.position : finalPlayerPos;
            
            Vector3 finalDirectionToPlayer = (finalTargetLookPos - heroineCharacter.transform.position).normalized;
            finalDirectionToPlayer.y = 0; // Y축 회전만 적용 (수평 회전)
            
            if (finalDirectionToPlayer != Vector3.zero)
            {
                Quaternion finalLookAtPlayer = Quaternion.LookRotation(finalDirectionToPlayer);
                heroineCharacter.transform.rotation = finalLookAtPlayer;
                Debug.Log("Heroine final rotation set to face player");
            }
            
            // 걷기 애니메이션 종료 (애니메이터가 있다면)
            if (heroineAnimator != null)
            {
                heroineAnimator.SetBool("IsWalking", false);
            }
            
            Debug.Log("Heroine character movement completed - moved while looking at player");
        }
        else
        {
            Debug.LogWarning("Heroine character or player transform not set");
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
        // 1. 흰색 배경 효과 (4초: 2초 페이드인 + 2초 페이드아웃)
        // 배경음악 변경과 컨트롤러 설정은 중간(2초 지점)에 실행
        StartCoroutine(SetupNextDialogueAtMidpoint());
        yield return StartCoroutine(FadeWhiteBackground(4f));
        
        // 2. 흰색 화면 전환 완료 후 TTS 시작
        if (nextDialogueController != null)
        {
            var finalController = nextDialogueController.GetComponent<DialogueFlowControllerLast>();
            if (finalController != null)
            {
                finalController.StartFinalDialogueFlow();
                Debug.Log("Final dialogue flow started after white background transition completed");
            }
        }
        
        // 3. 현재 오브젝트 비활성화 (다음 파트로 넘어가므로)
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 흰색 배경 중간 지점에서 다음 대화 설정
    /// </summary>
    IEnumerator SetupNextDialogueAtMidpoint()
    {
        // 2초 대기 (흰색 배경이 최대가 되는 시점)
        yield return new WaitForSeconds(2f);
        
        // 배경음악 변경 (LastDialogue용 음악으로 변경)
        if (backgroundSoundManager != null)
        {
            // 기본 음악으로 변경 (필요시 다른 음악으로 변경 가능)
            backgroundSoundManager.PlayDefaultMusic();
            Debug.Log("Background music changed for last dialogue");
        }
        
        // 다음 대화 컨트롤러 설정 (활성화만 하고 TTS는 아직 시작하지 않음)
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
    }
    
    /// <summary>
    /// 흰색 배경 효과 (0 → 1 → 0 패턴으로 알파값 조절)
    /// </summary>
    /// <param name="duration">전체 지속 시간 (절반씩 페이드인/아웃)</param>
    /// <returns></returns>
    IEnumerator FadeWhiteBackground(float duration)
    {
        if (whiteBackgroundObject == null) yield break;
        
        // 하얀색 이미지 컴포넌트 찾기
        UnityEngine.UI.Image whiteImage = whiteBackgroundObject.GetComponent<UnityEngine.UI.Image>();
        
        if (whiteImage == null)
        {
            // 하위 오브젝트에서 Image 컴포넌트 찾기
            whiteImage = whiteBackgroundObject.GetComponentInChildren<UnityEngine.UI.Image>();
        }
        
        if (whiteImage == null)
        {
            Debug.LogError("White background object has no Image component!");
            yield break;
        }
        
        float halfDuration = duration / 2f;
        
        Debug.Log($"White background fade: 0 → 1 → 0 over {duration} seconds (half: {halfDuration}s each)");
        
        // 1단계: 알파값 0 → 1 (절반 시간)
        float elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            float smoothT = t * t * (3f - 2f * t); // 부드러운 곡선 보간
            float currentAlpha = Mathf.Lerp(0f, 1f, smoothT);
            
            Color color = whiteImage.color;
            color.a = currentAlpha;
            whiteImage.color = color;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 중간값 확실히 설정 (알파값 1)
        Color midColor = whiteImage.color;
        midColor.a = 1f;
        whiteImage.color = midColor;
        
        // 2단계: 알파값 1 → 0 (나머지 절반 시간)
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            float smoothT = t * t * (3f - 2f * t); // 부드러운 곡선 보간
            float currentAlpha = Mathf.Lerp(1f, 0f, smoothT);
            
            Color color = whiteImage.color;
            color.a = currentAlpha;
            whiteImage.color = color;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 최종 알파값 설정 (0)
        Color finalColor = whiteImage.color;
        finalColor.a = 0f;
        whiteImage.color = finalColor;
        
        Debug.Log($"White background fade completed - Alpha returned to 0");
    }
    
    /// <summary>
    /// TTS 리스트를 순차적으로 재생
    /// </summary>
    IEnumerator PlayTTSList(List<AudioClip> ttsList)
    {
        if (ttsList == null || ttsList.Count == 0)
            yield break;
            
        isPlayingTTS = true;
        
        // TTS 시작 시 배경음악 볼륨 낮춤
        if (backgroundSoundManager != null)
        {
            backgroundSoundManager.DuckVolumeForTTS();
        }
        
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
        
        // TTS 종료 시 배경음악 볼륨 복원
        if (backgroundSoundManager != null)
        {
            backgroundSoundManager.RestoreVolumeAfterTTS();
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
        
        // TTS 시작 시 배경음악 볼륨 낮춤
        if (backgroundSoundManager != null)
        {
            backgroundSoundManager.DuckVolumeForTTS();
        }
        
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
        
        // TTS 종료 시 배경음악 볼륨 복원
        if (backgroundSoundManager != null)
        {
            backgroundSoundManager.RestoreVolumeAfterTTS();
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
