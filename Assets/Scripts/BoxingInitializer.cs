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
    
    // Game state
    private bool gameInitialized = false;
    private BoxingManager.GamePhase currentPhase = BoxingManager.GamePhase.Easy;
    
    void Start()
    {
        // 결과 패널 초기 비활성화
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (successPanel != null)
            successPanel.SetActive(false);
            
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
    /// 카운트다운 표시 및 게임 시작
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
        
        // 카운트다운 패널 비활성화
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
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
        
        // BoxingManager 게임 시작
        if (boxingManager != null)
        {
            boxingManager.StartGame();
            
            // BoxingManager의 페이즈 변경 이벤트 구독
            StartCoroutine(MonitorGamePhases());
        }
        
        // Easy Phase TTS 시작
        if (ttsManager != null)
        {
            ttsManager.StartEasyPhaseTTS();
        }
        
        gameInitialized = true;
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
        
        // 게임 종료 시 결과 처리
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
        Debug.Log($"Game Over! Final Score: {finalScore}");
        
        // 최소 점수 달성 여부에 따라 성공/실패 처리
        bool isSuccess = finalScore >= minScoreToWin;
        
        if (isSuccess)
        {
            // 성공 시
            if (successPanel != null)
            {
                successPanel.SetActive(true);
            }
            if (finalScoreText != null)
            {
                finalScoreText.text = $"최종 점수: {finalScore}";
            }
        }
        else
        {
            // 실패 시
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                
                // 3초 후 실패 TTS 재생
                StartCoroutine(PlayFailureTTSAfterDelay(3.0f));
            }
            if (finalScoreText != null)
            {
                finalScoreText.text = $"최종 점수: {finalScore}\n목표 점수: {minScoreToWin}";
            }
        }
    }
    
    /// <summary>
    /// 지정된 시간 후 실패 TTS 재생
    /// </summary>
    private IEnumerator PlayFailureTTSAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 실패 TTS 재생
        TriggerFailureTTS();
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
    /// 실패 시 TTS 재생 (외부에서 호출 가능)
    /// </summary>
    public void TriggerFailureTTS()
    {
        if (ttsManager != null)
        {
            ttsManager.StartFailureTTS();
        }
    }
    
    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("Restarting Boxing Game");
        
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
} 