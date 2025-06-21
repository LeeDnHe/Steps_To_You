using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BoxingInitializer : MonoBehaviour
{
    [Header("Manager References")]
    public BoxingTTSManager ttsManager;
    public BoxingManager boxingManager;
    public Transform xrOrigin; // XR Origin 참조 추가
    
    [Header("Position Settings")]
    public Transform preGamePlayerPosition; // Pre-Game 시 플레이어 위치
    public Transform preGameHeroinePosition; // Pre-Game 시 여주인공 위치
    public Transform boxingPlayerPosition; // 복싱 게임 시 플레이어 위치
    public Transform cheeringHeroinePosition; // 응원 시 여주인공 위치 (wav8 때)
    
    [Header("Character Animation")]
    public Animator boxingHeroineAnimator; // 복싱 전용 여주인공 애니메이터
    public Animator heroineAnimator; // 원래 여주인공 애니메이터
    
    [Header("Demo Cube Settings")]
    public Transform demoCubeSpawnPoint; // 데모 큐브 소환 위치
    public float demoCubeMoveSpeed = 5f; // 데모 큐브 이동 속도 (노말페이즈 속도)
    public float demoCubeLifeTime = 0.6f; // 데모 큐브 생존 시간 (초)
    
    [Header("Countdown Settings")]
    public GameObject countdownPanel;
    public TextMeshProUGUI countdownText;
    
    [Header("Game Flow Settings")]
    public float delayAfterPreGameTTS = 0.5f;
    
    [Header("Game End Settings")]
    public int minScoreToWin = 100;
    public GameObject successPanel; // 성공 시 표시할 패널
    public GameObject failurePanel; // 실패 시 표시할 패널
    public TextMeshProUGUI finalScoreText; // 최종 점수 텍스트
    
    [Header("Success Dialogue Settings")]
    public GameObject heroineCharacter; // 여주인공 캐릭터 (대화용)
    public GameObject boxingHeroineCharacter; // 복싱 전용 여주인공 캐릭터
    public DialogueFlowControllerAfterGame dialogueController; // 대화 컨트롤러
    
    [Header("White Background Transition")]
    public GameObject whiteBackgroundObject; // 흰색 배경 GameObject
    public GameObject[] backgroundAssets; // 배경 에셋들 (게임 시작 시 비활성화, 종료 시 다시 활성화)
    public Material boxingSkybox; // 복싱용 스카이박스
    
    [Header("Background Sound Manager")]
    public BackgroundSoundManager backgroundSoundManager; // 배경음악 매니저
    
    [Header("Various Audio Controller")]
    public VariousAudioController variousAudioController; // 오디오 관리 컨트롤러
    
    [Header("VFX Settings")]
    public GameObject portalVFX; // 복싱 게임 시작/종료 시 사용할 포탈 VFX
    
    // Game state
    private bool gameInitialized = false;
    private BoxingManager.GamePhase currentPhase = BoxingManager.GamePhase.Easy;
    private Material originalSkybox; // 원래 스카이박스 저장용
    private float originalAmbientIntensity; // 원래 감마값 저장용
    
    // Demo cube tracking
    private List<GameObject> activeDemoCubes = new List<GameObject>();
    
    void Start()
    {
        // 원래 스카이박스 및 감마값 저장
        originalSkybox = RenderSettings.skybox;
        originalAmbientIntensity = RenderSettings.ambientIntensity;
        
        // 흰색 배경 초기화 (비활성화)
        if (whiteBackgroundObject != null)
        {
            whiteBackgroundObject.SetActive(false);
        }
        
        // 포탈 VFX 초기화 (비활성화)
        if (portalVFX != null)
        {
            portalVFX.SetActive(false);
        }
        
        // XR Origin 위치 초기화
        if (xrOrigin != null)
        {
            xrOrigin.position = transform.position;
            xrOrigin.rotation = transform.rotation;
        }
        
        // 결과 패널들 초기 비활성화
        if (successPanel != null)
            successPanel.SetActive(false);
        if (failurePanel != null)
            failurePanel.SetActive(false);
            
        // 대화 컨트롤러 초기 비활성화
        if (dialogueController != null)
            dialogueController.gameObject.SetActive(false);
            
        // 복싱 전용 여주인공 초기 비활성화
        if (boxingHeroineCharacter != null)
            boxingHeroineCharacter.SetActive(false);
            
        // 기존 여주인공 비활성화
        if (heroineCharacter != null)
        {
            heroineCharacter.SetActive(false);
            Debug.Log("Original heroine deactivated for boxing sequence");
        }
            
        InitializeBoxingGame();
    }
    
    /// <summary>
    /// 복싱 게임 전체 초기화
    /// </summary>
    void InitializeBoxingGame()
    {
        Debug.Log("=== Boxing Game Initializer Started ===");
        
        // 카운트다운 패널 초기 비활성화
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }
        
        // BoxingManager 초기화 (게임 시작은 하지 않음)
        if (boxingManager != null)
        {
            boxingManager.gameObject.SetActive(true);
            // BoxingManager의 자동 시작을 막기 위해 필요시 추가 설정
        }
        
        // TTS Manager 초기화
        if (ttsManager != null)
        {
            // TTS Manager에서 자동 시작하지 않도록 설정
            ttsManager.enabled = true;
        }
        
        // 시작 전 TTS 재생
        StartPreGameSequence();
    }
    
    /// <summary>
    /// 게임 시작 전 시퀀스 (화면 전환 후 TTS 재생)
    /// </summary>
    void StartPreGameSequence()
    {
        Debug.Log("Starting Pre-Game Sequence with immediate transition");
        
        // 즉시 화면 전환 시작
        StartCoroutine(TransitionToBoxingAndStartTTS());
    }
    
    /// <summary>
    /// 복싱으로 전환하고 TTS 시작
    /// </summary>
    IEnumerator TransitionToBoxingAndStartTTS()
    {
        // 1. 흰색 배경 페이드인 (1초) - 플레이어와 여주인공 텔레포트 전
        yield return StartCoroutine(FadeWhiteBackground(0f, 1f, 1f));
        
        // 2. 플레이어와 복싱 전용 여주인공 Pre-Game 위치로 텔레포트
        TeleportToPreGamePositions();
        
        // 3. 흰색 배경 페이드아웃 (1초)
        yield return StartCoroutine(FadeWhiteBackground(1f, 0f, 1f));
        
        // 4. 설명 및 이지페이즈 음악 시작
        if (backgroundSoundManager != null)
        {
            backgroundSoundManager.PlayExplanationPhaseMusic();
        }
        else
        {
            // 백업: BackgroundSoundManager 인스턴스 찾기
            backgroundSoundManager = BackgroundSoundManager.Instance;
            if (backgroundSoundManager != null)
            {
                backgroundSoundManager.PlayExplanationPhaseMusic();
            }
        }
        
        // 5. TTS 시작
        if (ttsManager != null)
        {
            // TTS Manager에게 시작 전 TTS 재생 요청 (애니메이션 콜백 포함)
            ttsManager.StartPreGameTTS(OnPreGameTTSComplete, OnPreGameTTSClipStart);
        }
        else
        {
            Debug.LogWarning("TTS Manager not found, starting countdown directly");
            OnPreGameTTSComplete();
        }
    }
    
    /// <summary>
    /// Pre-Game 위치로 텔레포트
    /// </summary>
    void TeleportToPreGamePositions()
    {
        // 플레이어 텔레포트
        if (xrOrigin != null && preGamePlayerPosition != null)
        {
            xrOrigin.position = preGamePlayerPosition.position;
            xrOrigin.rotation = preGamePlayerPosition.rotation;
            Debug.Log("Player teleported to Pre-Game position");
        }
        
        // 복싱 전용 여주인공 활성화 및 텔레포트
        if (boxingHeroineCharacter != null && preGameHeroinePosition != null)
        {
            boxingHeroineCharacter.SetActive(true);
            boxingHeroineCharacter.transform.position = preGameHeroinePosition.position;
            boxingHeroineCharacter.transform.rotation = preGameHeroinePosition.rotation;
            Debug.Log("Boxing heroine activated and teleported to Pre-Game position");
        }
    }
    
    /// <summary>
    /// Pre-Game TTS 클립 시작 시 호출 (애니메이션 재생용)
    /// </summary>
    /// <param name="clipIndex">클립 인덱스 (0부터 시작)</param>
    void OnPreGameTTSClipStart(int clipIndex)
    {
        // wav3(index 2) -> take_18, wav4(index 3) -> take_19, wav5(index 4) -> take_20, wav6(index 5) -> take_21, wav7(index 6) -> take_22
        switch (clipIndex)
        {
            case 2: // wav3 - 오른손 큐브 예시
                if (boxingHeroineAnimator != null)
                {
                    boxingHeroineAnimator.Play("take18");
                    Debug.Log("Playing boxing heroine animation: take18 for wav3");
                }
                // 오른손 큐브 0.2초 후 소환
                StartCoroutine(SpawnDemoCubeDelayed(CubeType.Right, 0.6f));
                break;
            case 3: // wav4 - 왼손 큐브 예시
                if (boxingHeroineAnimator != null)
                {
                    boxingHeroineAnimator.Play("take_19");
                    Debug.Log("Playing boxing heroine animation: take_19 for wav4");
                }
                // 왼손 큐브 0.05초 후 소환
                StartCoroutine(SpawnDemoCubeDelayed(CubeType.Left, 0.05f));
                break;
            case 4: // wav5
                if (boxingHeroineAnimator != null)
                {
                    boxingHeroineAnimator.Play("take_20");
                    Debug.Log("Playing boxing heroine animation: take_20 for wav5");
                }
                break;
            case 5: // wav6 - 금지 큐브 예시
                if (boxingHeroineAnimator != null)
                {
                    boxingHeroineAnimator.Play("take_21");
                    Debug.Log("Playing boxing heroine animation: take_21 for wav6");
                }
                // 금지 큐브 0.5초 후 소환
                StartCoroutine(SpawnDemoCubeDelayed(CubeType.Forbidden,1.45f));
                break;
            case 6: // wav7
                if (boxingHeroineAnimator != null)
                {
                    boxingHeroineAnimator.Play("take_22");
                    Debug.Log("Playing boxing heroine animation: take_22 for wav7");
                }
                // 왼손 큐브 3번 연속 소환 (0.2초 간격)
                StartCoroutine(SpawnMultipleDemoCubes(CubeType.Left, 3, 0.95f));
                break;
            case 7: // wav8 (마지막 TTS)
                // wav8 시작 시 복싱 여주인공 비활성화 및 원래 여주인공 응원 위치로 텔레포트
                StartCoroutine(SwitchToCheeringHeroine());
                break;
        }
    }
    
    /// <summary>
    /// 응원하는 여주인공으로 전환 (wav8 시작 시)
    /// </summary>
    IEnumerator SwitchToCheeringHeroine()
    {
        // 1. 복싱 전용 여주인공 비활성화
        if (boxingHeroineCharacter != null)
        {
            boxingHeroineCharacter.SetActive(false);
            Debug.Log("Boxing heroine deactivated for wav8");
        }
        
        // 2. 원래 여주인공을 응원 위치로 텔레포트 및 활성화
        if (heroineCharacter != null && cheeringHeroinePosition != null)
        {
            heroineCharacter.transform.position = cheeringHeroinePosition.position;
            heroineCharacter.transform.rotation = cheeringHeroinePosition.rotation;
            heroineCharacter.SetActive(true);
            Debug.Log("Original heroine teleported to cheering position and activated");
            
            // 3. 응원 애니메이션 재생
            if (heroineAnimator != null)
            {
                heroineAnimator.Play("take_23");
                Debug.Log("Playing cheering animation: take_23 for wav8");
            }
        }
        
        yield return null;
    }
    
    /// <summary>
    /// 시작 전 TTS 완료 콜백
    /// </summary>
    void OnPreGameTTSComplete()
    {
        Debug.Log("Pre-Game TTS completed, deactivating cheering heroine and transitioning to boxing game");
        
        // wav8(마지막 TTS) 완료 후 원래 여주인공 비활성화
        if (heroineCharacter != null)
        {
            heroineCharacter.SetActive(false);
            Debug.Log("Original heroine deactivated after wav8 completion");
        }
        
        // 남은 데모 큐브들 정리
        CleanupDemoCubes();
        
        StartCoroutine(TransitionToBoxingGame());
    }
    
    /// <summary>
    /// 복싱 게임으로 전환 (배경 제거 및 플레이어 텔레포트)
    /// </summary>
    IEnumerator TransitionToBoxingGame()
    {
        // 1. 흰색 배경 페이드인 (1초)
        yield return StartCoroutine(FadeWhiteBackground(0f, 1f, 1f));
        
        // 2. 배경 에셋들 비활성화 및 스카이박스 변경
        DisableBackgroundAssets();
        ChangeSkybox(boxingSkybox);
        
        // 3. 플레이어를 복싱 시작 위치로 텔레포트
        if (xrOrigin != null && boxingPlayerPosition != null)
        {
            xrOrigin.position = boxingPlayerPosition.position;
            xrOrigin.rotation = boxingPlayerPosition.rotation;
            Debug.Log("Player teleported to Boxing position");
        }
        
        // 4. 복싱 전용 여주인공 비활성화
        if (boxingHeroineCharacter != null)
        {
            boxingHeroineCharacter.SetActive(false);
            Debug.Log("Boxing heroine character deactivated for boxing game");
        }
        
        // 5. 흰색 배경 페이드아웃 (1초)
        yield return StartCoroutine(FadeWhiteBackground(1f, 0f, 1f));
        
        // 6. 카운트다운 및 게임 시작
        StartCoroutine(ShowCountdownAndStartGame());
    }
    
    /// <summary>
    /// 카운트다운 표시 후 게임 시작
    /// </summary>
    IEnumerator ShowCountdownAndStartGame()
    {
        // 잠시 대기
        yield return new WaitForSeconds(delayAfterPreGameTTS);
        
        // 카운트다운 패널 활성화
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
        }
        
        // 콤보 UI 활성화 (5초 카운트다운 직전)
        if (boxingManager != null && boxingManager.ComboUI != null)
        {
            boxingManager.ComboUI.SetActive(true);
            Debug.Log("Combo UI activated before countdown");
        }
        
        // 본게임 음악 요청
        if (backgroundSoundManager != null)
        {
            backgroundSoundManager.PlayMainGameMusic();
            Debug.Log("Main game music requested");
        }
        
        // 5부터 1까지 카운트다운
        for (int count = 5; count >= 1; count--)
        {
            if (countdownText != null)
            {
                countdownText.text = count.ToString();
            }
            
            Debug.Log($"Countdown: {count}");
            yield return new WaitForSeconds(1f);
        }
        
        // 복싱 게임 시작
        StartBoxingGame();
    }
    
    /// <summary>
    /// 복싱 게임 실제 시작
    /// </summary>
    void StartBoxingGame()
    {
        Debug.Log("=== Starting Boxing Game ===");
        
        // 포탈 VFX 활성화
        if (portalVFX != null)
        {
            portalVFX.SetActive(true);
            Debug.Log("Portal VFX activated for boxing game start");
        }
        
        // BoxingManager 게임 시작
        if (boxingManager != null)
        {
            boxingManager.StartGame();
            
            // BoxingManager의 페이즈 변경 이벤트 구독 및 시간 표시 시작
            StartCoroutine(MonitorGamePhases());
            StartCoroutine(UpdatePhaseTimer());
        }
        
        // Easy Phase TTS 시작
        if (ttsManager != null)
        {
            ttsManager.StartEasyPhaseTTS();
        }
        
        gameInitialized = true;
    }
    
    /// <summary>
    /// 게임 중에는 카운트다운 패널 숨김 (결과 표시 시에만 사용)
    /// </summary>
    IEnumerator UpdatePhaseTimer()
    {
        // 게임 시작 후 카운트다운 패널 숨김
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }
        
        // 게임 종료까지 대기
        while (boxingManager != null && boxingManager.currentPhase != BoxingManager.GamePhase.Finished)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("Game finished - preparing to show results");
    }
    
    /// <summary>
    /// 게임 페이즈 모니터링
    /// </summary>
    IEnumerator MonitorGamePhases()
    {
        while (boxingManager != null && boxingManager.currentPhase != BoxingManager.GamePhase.Finished)
        {
            // 현재 페이즈 확인
            BoxingManager.GamePhase newPhase = boxingManager.currentPhase;
            
            // 페이즈가 변경되었을 때 TTS 재생
            if (newPhase != currentPhase)
            {
                OnPhaseChanged(newPhase);
                currentPhase = newPhase;
            }
            
            yield return new WaitForSeconds(0.1f); // 0.1초마다 체크
        }
        
        Debug.Log("=== Boxing Game Finished ===");
        
        // 4초 후 게임 종료 결과 처리
        StartCoroutine(HandleGameEndAfterDelay(4.0f));
    }
    
    /// <summary>
    /// 지연 시간 후 게임 종료 처리
    /// </summary>
    /// <param name="delay">지연 시간 (초)</param>
    /// <returns></returns>
    private IEnumerator HandleGameEndAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (boxingManager != null)
        {
            HandleGameEnd(boxingManager.Score);
        }
    }
    
    /// <summary>
    /// 게임 종료 시 결과 처리
    /// </summary>
    private void HandleGameEnd(int finalScore)
    {
        Debug.Log($"Game Over! \nFinal Score: {finalScore}");
        
        // 포탈 VFX 비활성화 (결과창 표시 전)
        if (portalVFX != null)
        {
            portalVFX.SetActive(false);
            Debug.Log("Portal VFX deactivated before showing results");
        }
        
        // 배경음악 정지
        if (backgroundSoundManager != null)
        {
            backgroundSoundManager.StopMusic();
            Debug.Log("Background music stopped");
        }
        
        // 콤보 UI 비활성화
        if (boxingManager != null && boxingManager.ComboUI != null)
        {
            boxingManager.ComboUI.SetActive(false);
            Debug.Log("Combo UI deactivated on game end");
        }
        
        // 최소 점수 달성 여부 확인
        bool isSuccess = finalScore >= minScoreToWin;
        
        // 카운트다운 패널 다시 활성화하고 점수 표시
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
        }
        
        // 최종 점수만 표시 (성공/실패 텍스트 제거)
        if (countdownText != null)
        {
            countdownText.text = $"최종 점수: {finalScore}";
        }
        
        // 성공/실패에 따른 패널 표시
        if (isSuccess)
        {
            if (successPanel != null)
            {
                successPanel.SetActive(true);
            }
        }
        else
        {
            if (failurePanel != null)
            {
                failurePanel.SetActive(true);
            }
        }
            
        // 결과창 열림 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayResultPanelOpen();
        }
        
        // 결과 표시 및 전환 시퀀스 시작
        StartCoroutine(ShowResultAndTransition(finalScore));
    }
    
    /// <summary>
    /// 결과 표시 후 흰색 전환 및 원복
    /// </summary>
    /// <param name="finalScore">최종 점수</param>
    /// <returns></returns>
    private IEnumerator ShowResultAndTransition(int finalScore)
    {
        // 1. 결과 화면 5초간 표시
        yield return new WaitForSeconds(5.0f);
        
        // 결과창 닫힘 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayResultPanelClose();
        }
        
        // 2. 흰색 배경 2초간 표시
        yield return StartCoroutine(FadeWhiteBackground(0f, 1f, 0.5f)); // 0.5초로 빠르게 페이드인
        yield return new WaitForSeconds(1.0f); // 1초 유지
        yield return StartCoroutine(FadeWhiteBackground(1f, 0f, 0.5f)); // 0.5초로 빠르게 페이드아웃
        
        // 3. 원래 상태로 복원
        RestoreOriginalState();
        
        // 4. 기본 배경음악으로 복원
        if (backgroundSoundManager != null)
        {
            backgroundSoundManager.PlayDefaultMusic();
            Debug.Log("Default music restored");
        }
        
        // 5. 대화 시작
        StartCoroutine(StartDialogueAfterDelay(1.0f));
    }
    
    /// <summary>
    /// 게임 페이즈 변경 시 호출
    /// </summary>
    void OnPhaseChanged(BoxingManager.GamePhase newPhase)
    {
        Debug.Log($"Phase changed to: {newPhase}");
        
        if (ttsManager != null)
        {
            switch (newPhase)
            {
                case BoxingManager.GamePhase.Normal:
                    ttsManager.StartNormalPhaseTTS();
                    break;
                case BoxingManager.GamePhase.Hard:
                    ttsManager.StartHardPhaseTTS();
                    break;
                case BoxingManager.GamePhase.Finished:
                    // 게임 완료 시 필요한 처리
                    break;
            }
        }
    }
    
    /// <summary>
    /// 게임 종료 처리
    /// </summary>
    public void EndGame()
    {
        Debug.Log("Game ended");
        
        // 콤보 UI 비활성화는 HandleGameEnd에서 처리됨
    }
    
    /// <summary>
    /// 흰색 배경 표시/숨김 효과 (CanvasGroup 알파값 + 감마값 페이드)
    /// </summary>
    /// <param name="startAlpha">시작 상태 (0: 숨김, 1: 표시)</param>
    /// <param name="endAlpha">끝 상태 (0: 숨김, 1: 표시)</param>
    /// <param name="duration">지속 시간</param>
    /// <returns></returns>
    private IEnumerator FadeWhiteBackground(float startAlpha, float endAlpha, float duration)
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
    /// 스카이박스 변경
    /// </summary>
    /// <param name="newSkybox">새로운 스카이박스</param>
    private void ChangeSkybox(Material newSkybox)
    {
        if (newSkybox != null)
        {
            RenderSettings.skybox = newSkybox;
            DynamicGI.UpdateEnvironment();
            Debug.Log($"Skybox changed to: {newSkybox.name}");
        }
    }
    
    /// <summary>
    /// 배경 에셋들 비활성화
    /// </summary>
    private void DisableBackgroundAssets()
    {
        if (backgroundAssets != null)
        {
            foreach (GameObject obj in backgroundAssets)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    Debug.Log($"Disabled background asset: {obj.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 배경 에셋들 다시 활성화
    /// </summary>
    private void EnableBackgroundAssets()
    {
        if (backgroundAssets != null)
        {
            foreach (GameObject obj in backgroundAssets)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    Debug.Log($"Enabled background asset: {obj.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 원래 상태로 복원
    /// </summary>
    private void RestoreOriginalState()
    {
        // 원래 스카이박스로 복원
        ChangeSkybox(originalSkybox);
        
        // 배경 에셋들 다시 활성화
        EnableBackgroundAssets();
        
        Debug.Log("Original state restored - skybox and background assets");
    }

    /// <summary>
    /// 결과 표시 후 여주인공 캐릭터 배치 및 대화 시작
    /// </summary>
    /// <param name="delay">지연 시간 (초)</param>
    /// <returns></returns>
    private IEnumerator StartDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 결과 패널들 비활성화
        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }
        if (failurePanel != null)
        {
            failurePanel.SetActive(false);
        }
        
        // 대화 컨트롤러에 게임 점수 전달
        if (dialogueController != null)
        {
            var afterGameController = dialogueController.GetComponent<DialogueFlowControllerAfterGame>();
            if (afterGameController != null && boxingManager != null)
            {
                afterGameController.SetGameScore(boxingManager.Score);
                Debug.Log($"Game score {boxingManager.Score} passed to DialogueFlowControllerAfterGame");
            }
        }
        
        // 여주인공 캐릭터 위치 설정 (플레이어 기준 x: +1,z: 0)
        if (heroineCharacter != null && xrOrigin != null)
        {
            Vector3 playerPosition = xrOrigin.position;
            Vector3 heroinePosition = new Vector3(
                playerPosition.x + 1f,  // x축으로 +1
                playerPosition.y,       // y축은
                playerPosition.z + 1f   // z축으로 1
            );
            
            heroineCharacter.transform.position = heroinePosition;
            
            // 여주인공이 플레이어를 바라보도록 회전 설정
            Vector3 lookDirection = (playerPosition - heroinePosition).normalized;
            lookDirection.y = 0; // Y축 회전만 적용
            if (lookDirection != Vector3.zero)
            {
                heroineCharacter.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
            
            // 여주인공 캐릭터 활성화
            heroineCharacter.SetActive(true);
        }
        
        // 대화 컨트롤러 활성화 및 시작
        if (dialogueController != null)
        {
            dialogueController.gameObject.SetActive(true);
        }
        
        // 현재 복싱 오브젝트 비활성화 (다음 파트로 넘어가므로)
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 데모 큐브 타입 열거형
    /// </summary>
    public enum CubeType
    {
        Left,
        Right,
        Forbidden
    }
    
    /// <summary>
    /// 지연 시간 후 데모 큐브 소환
    /// </summary>
    /// <param name="cubeType">소환할 큐브 타입</param>
    /// <param name="delay">지연 시간 (초)</param>
    /// <returns></returns>
    IEnumerator SpawnDemoCubeDelayed(CubeType cubeType, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnDemoCube(cubeType);
        Debug.Log($"Demo {cubeType} cube spawned after {delay} seconds delay");
    }
    
    /// <summary>
    /// 여러 개의 데모 큐브를 일정 간격으로 연속 소환
    /// </summary>
    /// <param name="cubeType">소환할 큐브 타입</param>
    /// <param name="count">소환할 큐브 개수</param>
    /// <param name="interval">소환 간격 (초)</param>
    /// <returns></returns>
    IEnumerator SpawnMultipleDemoCubes(CubeType cubeType, int count, float interval)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnDemoCube(cubeType);
            Debug.Log($"Demo {cubeType} cube #{i + 1} spawned (total: {count})");
            
            // 마지막 큐브가 아니면 간격만큼 대기
            if (i < count - 1)
            {
                yield return new WaitForSeconds(interval);
            }
        }
        
        Debug.Log($"All {count} demo {cubeType} cubes spawned with {interval} seconds interval");
    }
    
    /// <summary>
    /// 데모 큐브 소환
    /// </summary>
    /// <param name="cubeType">소환할 큐브 타입</param>
    void SpawnDemoCube(CubeType cubeType)
    {
        if (boxingManager == null || demoCubeSpawnPoint == null)
        {
            Debug.LogWarning("BoxingManager or demo spawn point not set!");
            return;
        }
        
        GameObject cubeToSpawn = null;
        string cubeTypeName = "";
        
        // 큐브 타입에 따라 프리팹 선택
        switch (cubeType)
        {
            case CubeType.Left:
                cubeToSpawn = boxingManager.LeftCube;
                cubeTypeName = "Left";
                break;
            case CubeType.Right:
                cubeToSpawn = boxingManager.RightCube;
                cubeTypeName = "Right";
                break;
            case CubeType.Forbidden:
                cubeToSpawn = boxingManager.ForbiddenCube;
                cubeTypeName = "Forbidden";
                break;
        }
        
        if (cubeToSpawn == null)
        {
            Debug.LogWarning($"Demo {cubeTypeName} cube prefab not found!");
            return;
        }
        
        // 큐브 생성
        GameObject demoCube = Instantiate(cubeToSpawn, demoCubeSpawnPoint.position, Quaternion.identity);
        activeDemoCubes.Add(demoCube);
        
        Debug.Log($"Demo {cubeTypeName} cube spawned at {demoCubeSpawnPoint.position}");
        
        // 데모 큐브 이동 및 파괴 코루틴 시작
        StartCoroutine(MoveDemoCube(demoCube, cubeType));
    }
    
    /// <summary>
    /// 데모 큐브 이동 및 파괴 처리
    /// </summary>
    /// <param name="demoCube">이동시킬 데모 큐브</param>
    /// <param name="cubeType">큐브 타입 (VFX 재생용)</param>
    /// <returns></returns>
    IEnumerator MoveDemoCube(GameObject demoCube, CubeType cubeType)
    {
        if (demoCube == null) yield break;
        
        float elapsedTime = 0f;
        Vector3 startPosition = demoCube.transform.position;
        
        // 금지 큐브와 일반 큐브의 다른 처리
        if (cubeType == CubeType.Forbidden)
        {
            // 금지 큐브: 1초 동안 계속 이동하다가 사라짐 (VFX 없음)
            while (elapsedTime < 1f && demoCube != null)
            {
                // -Z 방향으로 노말페이즈 속도로 이동
                demoCube.transform.Translate(Vector3.back * demoCubeMoveSpeed * Time.deltaTime);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 1초 후 금지 큐브 조용히 사라짐 (VFX 없음)
            if (demoCube != null)
            {
                activeDemoCubes.Remove(demoCube);
                Destroy(demoCube);
                Debug.Log($"Demo Forbidden cube disappeared after 1 second without VFX");
            }
        }
        else
        {
            // 일반 큐브 (Left, Right): 0.5초 후 터지면서 VFX 재생
            while (elapsedTime < demoCubeLifeTime && demoCube != null)
            {
                // -Z 방향으로 노말페이즈 속도로 이동
                demoCube.transform.Translate(Vector3.back * demoCubeMoveSpeed * Time.deltaTime);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 0.5초 후 큐브 파괴 및 이펙트 재생
            if (demoCube != null)
            {
                Vector3 explosionPosition = demoCube.transform.position;
                
                // 큐브 타입에 따른 VFX 재생
                PlayDemoCubeVFX(cubeType, explosionPosition);
                
                // 큐브 파괴
                activeDemoCubes.Remove(demoCube);
                Destroy(demoCube);
                
                Debug.Log($"Demo {cubeType} cube destroyed at {explosionPosition} after {demoCubeLifeTime} seconds");
            }
        }
    }
    
    /// <summary>
    /// 데모 큐브 VFX 재생
    /// </summary>
    /// <param name="cubeType">큐브 타입</param>
    /// <param name="position">VFX 재생 위치</param>
    void PlayDemoCubeVFX(CubeType cubeType, Vector3 position)
    {
        if (boxingManager == null) return;
        
        switch (cubeType)
        {
            case CubeType.Left:
                boxingManager.PlayLeftCubeHitVFX(position);
                Debug.Log($"Demo Left cube VFX played at {position}");
                break;
            case CubeType.Right:
                boxingManager.PlayRightCubeHitVFX(position);
                Debug.Log($"Demo Right cube VFX played at {position}");
                break;
            case CubeType.Forbidden:
                boxingManager.PlayForbiddenCubeHitVFX(position);
                Debug.Log($"Demo Forbidden cube VFX played at {position}");
                break;
        }
    }
    
    /// <summary>
    /// 모든 활성 데모 큐브 정리
    /// </summary>
    void CleanupDemoCubes()
    {
        foreach (GameObject demoCube in activeDemoCubes)
        {
            if (demoCube != null)
            {
                Destroy(demoCube);
            }
        }
        activeDemoCubes.Clear();
        Debug.Log("All demo cubes cleaned up");
    }
} 