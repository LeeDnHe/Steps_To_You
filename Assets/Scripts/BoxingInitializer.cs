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
    
    [Header("Countdown Settings")]
    public GameObject countdownPanel;
    public TextMeshProUGUI countdownText;
    
    [Header("Game Flow Settings")]
    public float delayAfterPreGameTTS = 0.5f;
    
    [Header("Game End Settings")]
    public int minScoreToWin = 100;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public GameObject successPanel;
    public Button restartButton;
    public Button exitButton;
    public string exitSceneName = "MainRoom"; // 기본방 씬 이름
    
    [Header("Success Dialogue Settings")]
    public GameObject heroineCharacter; // 여주인공 캐릭터
    public DialogueFlowControllerAfterGame dialogueController; // 대화 컨트롤러
    
    [Header("Background Music Settings")]
    public AudioSource backgroundMusicSource; // 배경음악용 오디오소스
    public AudioClip backgroundMusicClip; // 배경음악 클립
    
    // Game state
    private bool gameInitialized = false;
    private BoxingManager.GamePhase currentPhase = BoxingManager.GamePhase.Easy;
    
    void Start()
    {
        // XR Origin 위치 초기화
        if (xrOrigin != null)
        {
            xrOrigin.position = transform.position;
            xrOrigin.rotation = transform.rotation;
        }
        
        // 배경음악 AudioSource 초기화
        if (backgroundMusicSource == null)
        {
            backgroundMusicSource = GetComponent<AudioSource>();
            if (backgroundMusicSource == null)
            {
                backgroundMusicSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // 배경음악 설정
        if (backgroundMusicSource != null && backgroundMusicClip != null)
        {
            backgroundMusicSource.clip = backgroundMusicClip;
            backgroundMusicSource.loop = true; // 반복 재생
            backgroundMusicSource.playOnAwake = false; // 자동 재생 비활성화
            Debug.Log("Background music initialized");
        }
        
        // 결과 패널 초기 비활성화
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (successPanel != null)
            successPanel.SetActive(false);
            
        // 여주인공 캐릭터와 대화 컨트롤러 초기 비활성화
        if (heroineCharacter != null)
            heroineCharacter.SetActive(false);
        if (dialogueController != null)
            dialogueController.gameObject.SetActive(false);
            
        // 버튼 이벤트 등록
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitToMainRoom);
            
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
    /// 게임 시작 전 시퀀스 (TTS 재생)
    /// </summary>
    void StartPreGameSequence()
    {
        Debug.Log("Starting Pre-Game Sequence");
        
        if (ttsManager != null)
        {
            // TTS Manager에게 시작 전 TTS 재생 요청
            ttsManager.StartPreGameTTS(OnPreGameTTSComplete);
        }
        else
        {
            Debug.LogWarning("TTS Manager not found, starting countdown directly");
            OnPreGameTTSComplete();
        }
    }
    
    /// <summary>
    /// 시작 전 TTS 완료 콜백
    /// </summary>
    void OnPreGameTTSComplete()
    {
        Debug.Log("Pre-Game TTS completed, starting countdown");
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
        
        // 배경음악 시작 (5초 카운트다운과 함께)
        StartBackgroundMusic();
        
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
        
        // 카운트다운 패널은 비활성화하지 않고 계속 유지
        // 게임 시작 후 남은 시간 표시용으로 사용
        
        // 복싱 게임 시작
        StartBoxingGame();
    }
    
    /// <summary>
    /// 복싱 게임 실제 시작
    /// </summary>
    void StartBoxingGame()
    {
        Debug.Log("=== Starting Boxing Game ===");
        
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
    /// 각 페이즈의 남은 시간을 실시간으로 업데이트
    /// </summary>
    IEnumerator UpdatePhaseTimer()
    {
        while (boxingManager != null && boxingManager.currentPhase != BoxingManager.GamePhase.Finished)
        {
            if (countdownText != null && boxingManager != null)
            {
                float remainingTime = boxingManager.GetRemainingPhaseTime();
                string phaseName = GetPhaseDisplayName(boxingManager.currentPhase);
                
                // 남은 시간을 분:초 형식으로 표시
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                
                countdownText.text = $"{phaseName}\n{minutes:00}:{seconds:00}";
            }
            
            yield return new WaitForSeconds(0.1f); // 0.1초마다 업데이트
        }
        
        // 게임 종료 시 countdown 패널은 유지 (결과 표시용)
        Debug.Log("Game finished - keeping countdown panel for results");
    }
    
    /// <summary>
    /// 페이즈 이름을 표시용으로 변환
    /// </summary>
    string GetPhaseDisplayName(BoxingManager.GamePhase phase)
    {
        switch (phase)
        {
            case BoxingManager.GamePhase.Easy:
                return "EASY";
            case BoxingManager.GamePhase.Normal:
                return "NORMAL";
            case BoxingManager.GamePhase.Hard:
                return "HARD";
            default:
                return "GAME";
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
        
        // 배경음악 정지 (성공/실패 관계없이)
        StopBackgroundMusic();
        
        // 최소 점수 달성 여부에 따라 성공/실패 처리
        bool isSuccess = finalScore >= minScoreToWin;
        
        if (isSuccess)
        {
            // 성공 시
            if (successPanel != null)
            {
                successPanel.SetActive(true);
            }
     
            // countdown 패널에 성공 결과 표시
            if (countdownText != null)
            {
                countdownText.text = $"게임 성공!\n최종 점수: {finalScore}";
            }
            
            // 4초 후 여주인공 캐릭터 배치 및 대화 시작
            StartCoroutine(StartSuccessDialogueAfterDelay(4.0f));
        }
        else
        {
            // 실패 시
            if (gameOverPanel != null)
            {
                // 5초 후 게임오버 패널 활성화
                StartCoroutine(ShowGameOverPanelAfterDelay(5.0f));
                
                // 3초 후 실패 TTS 재생
                StartCoroutine(PlayFailureTTSAfterDelay(3.0f));
            }
           
            
            // countdown 패널에 실패 결과 표시
            if (countdownText != null)
            {
                countdownText.text = $"게임 실패\n최종 점수: {finalScore}\n목표 점수: {minScoreToWin}";
            }
        }
    }
    
    /// <summary>
    /// 지정된 시간 후 실패 TTS 재생
    /// </summary>
    /// <param name="delay">지연 시간 (초)</param>
    /// <returns></returns>
    private IEnumerator PlayFailureTTSAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (ttsManager != null)
        {
            ttsManager.StartFailureTTS();
        }
    }
    
    /// <summary>
    /// 지정된 시간 후 게임오버 패널 활성화
    /// </summary>
    /// <param name="delay">지연 시간 (초)</param>
    /// <returns></returns>
    private IEnumerator ShowGameOverPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// 성공 후 여주인공 캐릭터 배치 및 대화 시작
    /// </summary>
    /// <param name="delay">지연 시간 (초)</param>
    /// <returns></returns>
    private IEnumerator StartSuccessDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 성공 패널 비활성화
        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }
        
        // 여주인공 캐릭터 위치 설정 (플레이어 기준 x: -2, z: +2)
        if (heroineCharacter != null && xrOrigin != null)
        {
            Vector3 playerPosition = xrOrigin.position;
            Vector3 heroinePosition = new Vector3(
                playerPosition.x - 2f,  // x축으로 -2
                playerPosition.y,       // y축은 동일
                playerPosition.z + 2f   // z축으로 +2
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
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("Restarting Boxing Game");
        
        // 배경음악 정지
        StopBackgroundMusic();
        
        // XR Origin 위치 재설정
        if (xrOrigin != null)
        {
            xrOrigin.position = transform.position;
            xrOrigin.rotation = transform.rotation;
        }
        
        // 현재 진행 중인 모든 코루틴 중단
        StopAllCoroutines();
        
        // TTS 중단
        if (ttsManager != null)
        {
            ttsManager.StopCurrentTTS();
        }
        
        // 결과 패널 비활성화
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (successPanel != null)
            successPanel.SetActive(false);
        
        // 상태 초기화
        gameInitialized = false;
        currentPhase = BoxingManager.GamePhase.Easy;
        
        // 게임 스코어 리셋
        if (boxingManager != null)
        {
            boxingManager.Score = 0;
            boxingManager.Combo = 0;
            boxingManager.SpawnedCount = 0;
        }
        
        // 게임 재초기화
        InitializeBoxingGame();
    }
    
    /// <summary>
    /// 게임 중단
    /// </summary>
    public void StopGame()
    {
        Debug.Log("Stopping Boxing Game");
        
        // 배경음악 정지
        StopBackgroundMusic();
        
        StopAllCoroutines();
        
        if (ttsManager != null)
        {
            ttsManager.StopCurrentTTS();
        }
        
        if (boxingManager != null)
        {
            // BoxingManager 중단 (필요시 BoxingManager에 StopGame 메서드 추가)
        }
        
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 현재 게임 상태 확인
    /// </summary>
    public bool IsGameInitialized()
    {
        return gameInitialized;
    }
    
    /// <summary>
    /// 현재 페이즈 확인
    /// </summary>
    public BoxingManager.GamePhase GetCurrentPhase()
    {
        return currentPhase;
    }
    
    /// <summary>
    /// 메인 룸으로 돌아가기
    /// </summary>
    public void ExitToMainRoom()
    {
        Debug.Log($"Exiting to scene: {exitSceneName}");
        SceneManager.LoadScene(exitSceneName);
    }
    
    /// <summary>
    /// 게임 종료 처리
    /// </summary>
    public void EndGame()
    {
        Debug.Log("Game ended");
        
        // 콤보 UI 비활성화
        if (boxingManager != null && boxingManager.ComboUI != null)
        {
            boxingManager.ComboUI.SetActive(false);
            Debug.Log("Combo UI deactivated on game end");
        }
        
        // 게임 종료 후 5초 대기 후 게임 오버 패널 표시
        StartCoroutine(ShowGameOverPanelAfterDelay(5f));
    }

    /// <summary>
    /// 배경음악 시작
    /// </summary>
    void StartBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicClip != null)
        {
            backgroundMusicSource.Play();
            Debug.Log("Background music started");
        }
        else
        {
            Debug.LogWarning("Background music source or clip is missing");
        }
    }
    
    /// <summary>
    /// 배경음악 정지
    /// </summary>
    void StopBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
            Debug.Log("Background music stopped");
        }
    }
} 