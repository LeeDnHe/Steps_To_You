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
    
    [Header("Game UI Settings")]
    public GameObject scorePanel;
    public TextMeshProUGUI scoreText;
    public GameObject comboPanel;
    public TextMeshProUGUI comboText;
    public GameObject timePanel;
    public TextMeshProUGUI timeText;
    public GameObject stage1Panel; // Easy Phase 표시
    public GameObject stage2Panel; // Normal Phase 표시
    public GameObject stage3Panel; // Hard Phase 표시
    
    [Header("Game Flow Settings")]
    public float delayAfterPreGameTTS = 0.5f;
    
    [Header("Game End Settings")]
    public int minScoreToWin = 100;
    public GameObject successPanel; // 성공 시 표시할 패널
    public GameObject failurePanel; // 실패 시 표시할 패널
    
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
            
        // 게임 UI 패널들 초기 비활성화
        if (scorePanel != null)
            scorePanel.SetActive(false);
        if (comboPanel != null)
            comboPanel.SetActive(false);
        if (timePanel != null)
            timePanel.SetActive(false);
        if (stage1Panel != null)
            stage1Panel.SetActive(false);
        if (stage2Panel != null)
            stage2Panel.SetActive(false);
        if (stage3Panel != null)
            stage3Panel.SetActive(false);
            
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
        // 1. 흰색 배경 효과 (2초: 1초 페이드인 + 1초 페이드아웃)
        // 텔레포트는 중간(1초 지점)에 실행
        StartCoroutine(TeleportAtMidpoint());
        yield return StartCoroutine(FadeWhiteBackground(2f));
        
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
    /// 흰색 배경 중간 지점(1초 후)에 텔레포트 실행
    /// </summary>
    IEnumerator TeleportAtMidpoint()
    {
        // 1초 대기 (흰색 배경이 최대가 되는 시점)
        yield return new WaitForSeconds(1f);
        
        // 텔레포트 실행
        TeleportToPreGamePositions();
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
        // 1. 흰색 배경 효과 (2초: 1초 페이드인 + 1초 페이드아웃)
        // 환경 변경과 텔레포트는 중간(1초 지점)에 실행
        StartCoroutine(SetupBoxingEnvironmentAtMidpoint());
        yield return StartCoroutine(FadeWhiteBackground(2f));
        
        // 2. 카운트다운 및 게임 시작
        StartCoroutine(ShowCountdownAndStartGame());
    }
    
    /// <summary>
    /// 흰색 배경 중간 지점에서 복싱 환경 설정
    /// </summary>
    IEnumerator SetupBoxingEnvironmentAtMidpoint()
    {
        // 1초 대기 (흰색 배경이 최대가 되는 시점)
        yield return new WaitForSeconds(1f);
        
        // 배경 에셋들 비활성화 및 스카이박스 변경
        DisableBackgroundAssets();
        ChangeSkybox(boxingSkybox);
        
        // 플레이어를 복싱 시작 위치로 텔레포트
        if (xrOrigin != null && boxingPlayerPosition != null)
        {
            xrOrigin.position = boxingPlayerPosition.position;
            xrOrigin.rotation = boxingPlayerPosition.rotation;
            Debug.Log("Player teleported to Boxing position");
        }
        
        // 복싱 전용 여주인공 비활성화
        if (boxingHeroineCharacter != null)
        {
            boxingHeroineCharacter.SetActive(false);
            Debug.Log("Boxing heroine character deactivated for boxing game");
        }
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
        
        // 게임 UI 활성화 (5초 카운트다운 직전)
        if (scorePanel != null)
        {
            scorePanel.SetActive(true);
            Debug.Log("Score Panel activated before countdown");
        }
        if (comboPanel != null)
        {
            comboPanel.SetActive(true);
            Debug.Log("Combo Panel activated before countdown");
        }
        if (timePanel != null)
        {
            timePanel.SetActive(true);
            Debug.Log("Time Panel activated before countdown");
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
            StartCoroutine(UpdateGameUI()); // 게임 UI 실시간 업데이트
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
    /// 게임 UI 실시간 업데이트
    /// </summary>
    IEnumerator UpdateGameUI()
    {
        while (boxingManager != null && boxingManager.currentPhase != BoxingManager.GamePhase.Finished)
        {
            // 점수 업데이트
            if (scoreText != null && boxingManager != null)
            {
                scoreText.text = boxingManager.Score.ToString();
            }
            
            // 콤보 업데이트
            if (comboText != null && boxingManager != null)
            {
                comboText.text = boxingManager.Combo.ToString();
            }
            
            // 남은 시간 업데이트
            if (timeText != null && boxingManager != null)
            {
                float remainingTime = boxingManager.GetRemainingPhaseTime();
                int minutes = Mathf.FloorToInt(remainingTime / 60);
                int seconds = Mathf.FloorToInt(remainingTime % 60);
                timeText.text = $"{minutes:00}:{seconds:00}";
            }
            
            yield return new WaitForSeconds(0.1f); // 0.1초마다 업데이트
        }
    }
    
    /// <summary>
    /// Stage 패널을 해당 스테이지 내내 표시
    /// </summary>
    void ShowStagePanel(GameObject stagePanel, string stageName)
    {
        if (stagePanel != null)
        {
            Debug.Log($"Showing {stageName} - will remain visible during the stage");
            stagePanel.SetActive(true);
        }
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
        
        // 게임 UI 비활성화 (Score만 제외)
        if (comboPanel != null)
        {
            comboPanel.SetActive(false);
            Debug.Log("Combo Panel deactivated on game end");
        }
        if (timePanel != null)
        {
            timePanel.SetActive(false);
            Debug.Log("Time Panel deactivated on game end");
        }
        // 모든 스테이지 패널 비활성화
        if (stage1Panel != null)
            {
            stage1Panel.SetActive(false);
            Debug.Log("Stage 1 Panel deactivated on game end");
        }
        if (stage2Panel != null)
            {
            stage2Panel.SetActive(false);
            Debug.Log("Stage 2 Panel deactivated on game end");
            }
        if (stage3Panel != null)
            {
            stage3Panel.SetActive(false);
            Debug.Log("Stage 3 Panel deactivated on game end");
            }
            
        // Score Panel은 활성화 유지하여 최종 점수 표시
        
        // 점수 집계 중단 (결과 패널 표시 직전에만)
        if (boxingManager != null)
        {
            boxingManager.StopScoreCounting();
            Debug.Log("Score counting stopped - final score locked");
        }
        
        // 최종 점수 다시 가져오기 (점수 집계 중단 후)
        int lockedFinalScore = boxingManager != null ? boxingManager.Score : finalScore;
        Debug.Log($"Final locked score: {lockedFinalScore}");
                
        // 최소 점수 달성 여부 확인
        bool isSuccess = lockedFinalScore >= minScoreToWin;
        
        // Score Panel에 최종 점수 표시
        if (scoreText != null)
            {
            scoreText.text = lockedFinalScore.ToString();
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
        StartCoroutine(ShowResultAndTransition(lockedFinalScore));
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
        
        // 모든 게임 UI 패널 비활성화 (5초 후)
        if (scorePanel != null)
        {
            scorePanel.SetActive(false);
            Debug.Log("Score Panel deactivated after 5 seconds");
        }
        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }
        if (failurePanel != null)
        {
            failurePanel.SetActive(false);
        }
        
        // 2. 흰색 배경 효과 (2초: 1초 페이드인 + 1초 페이드아웃)
        yield return StartCoroutine(FadeWhiteBackground(2f));
            
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
        
        // Stage 패널 표시 (이전 스테이지 패널 숨기기 + 새 스테이지 패널 표시)
        switch (newPhase)
        {
            case BoxingManager.GamePhase.Easy:
                // 게임 시작 시 Easy Phase
                ShowStagePanel(stage1Panel, "Stage 1 - Easy Phase");
                break;
            case BoxingManager.GamePhase.Normal:
                // Easy Phase에서 Normal Phase로 전환 시
                if (stage1Panel != null)
                {
                    stage1Panel.SetActive(false);
                    Debug.Log("Stage 1 panel hidden as transitioning to Stage 2");
                }
                ShowStagePanel(stage2Panel, "Stage 2 - Normal Phase");
                break;
            case BoxingManager.GamePhase.Hard:
                // Normal Phase에서 Hard Phase로 전환 시
                if (stage2Panel != null)
                {
                    stage2Panel.SetActive(false);
                    Debug.Log("Stage 2 panel hidden as transitioning to Stage 3");
                }
                ShowStagePanel(stage3Panel, "Stage 3 - Hard Phase");
                break;
        }
        
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
    /// 흰색 배경 효과 (0 → 1 → 0 패턴으로 알파값 조절)
    /// </summary>
    /// <param name="duration">전체 지속 시간 (절반씩 페이드인/아웃)</param>
    /// <returns></returns>
    private IEnumerator FadeWhiteBackground(float duration)
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
        
        // 플레이어 Y rotation 90도 회전 (복싱 종료 후 원래 방향으로) - 오브젝트 소환 전에 먼저 실행
        if (xrOrigin != null)
        {
            Vector3 currentRotation = xrOrigin.eulerAngles;
            currentRotation.y += 90f; // Y축으로 90도 회전
            xrOrigin.rotation = Quaternion.Euler(currentRotation);
            Debug.Log($"Player rotated by 90 degrees on Y axis. New rotation: {currentRotation}");
        }
        
        // 여주인공 캐릭터 위치 설정 (플레이어 기준 x: +1,z: 0)
        if (heroineCharacter != null && xrOrigin != null)
        {
            Vector3 playerPosition = xrOrigin.position;
            Vector3 heroinePosition = new Vector3(
                playerPosition.x + 1.5f,  // x축으로 +1
                playerPosition.y,       // y축은
                playerPosition.z   // z축으로 1
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