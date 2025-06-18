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
    public Transform heroineStartPosition; // 여주인공 시작 위치
    public Transform heroineEndPosition;   // 여주인공 도착 위치
    public float heroineWalkDuration = 1.0f; // 여주인공 이동 시간
    
    [Header("Managers")]
    public AffectionManager affectionManager; // 호감도 매니저
    
    [Header("Next Scene Settings")]
    public GameObject nextDialogueController; // 다음 대화 컨트롤러
    
    [Header("Settings")]
    public float delayBetweenTTS = 0.5f;
    
    // Internal variables
    private bool isPlayingTTS = false;
    private int firstSelectedChoice = -1;
    private int secondSelectedChoice = -1;
    private bool gameSuccessful = false;
    
    // Animation support (확장성을 위해)
    private Animator heroineAnimator;
    
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
        
        // 호감도 매니저 초기화
        if (affectionManager == null)
        {
            affectionManager = AffectionManager.Instance;
        }
        
        // 여주인공 애니메이터 가져오기 (있다면)
        if (heroineCharacter != null)
        {
            heroineAnimator = heroineCharacter.GetComponent<Animator>();
        }
        
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
        yield return StartCoroutine(PlayTTSList(intermediateTTSList));
        
        // 4. 첫 번째 선택지 등장
        yield return StartCoroutine(ShowFirstChoicePanel());
        
        // 5. 첫 번째 선택에 따른 TTS 재생
        yield return StartCoroutine(PlayFirstChoiceTTS());
        
        // 6. 마지막 TTS 재생
        yield return StartCoroutine(PlayTTSList(lastTTSList));
        
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
        
        if (initialTTSList.Count > 0)
        {
            Debug.Log($"Playing initial TTS - Game {(gameSuccessful ? "Success" : "Failure")}");
            yield return StartCoroutine(PlayTTSList(initialTTSList));
        }
    }
    
    /// <summary>
    /// 여주인공 캐릭터 이동 (애니메이션 지원)
    /// </summary>
    IEnumerator MoveHeroineCharacter()
    {
        if (heroineCharacter != null && heroineStartPosition != null && heroineEndPosition != null)
        {
            Debug.Log("Moving heroine character");
            
            // 시작 위치 설정
            heroineCharacter.transform.position = heroineStartPosition.position;
            heroineCharacter.transform.rotation = heroineStartPosition.rotation;
            
            // 걷기 애니메이션 시작 (애니메이터가 있다면)
            if (heroineAnimator != null)
            {
                heroineAnimator.SetBool("IsWalking", true);
            }
            
            // 이동
            float elapsedTime = 0;
            Vector3 startPos = heroineStartPosition.position;
            Vector3 endPos = heroineEndPosition.position;
            Quaternion startRot = heroineStartPosition.rotation;
            Quaternion endRot = heroineEndPosition.rotation;
            
            while (elapsedTime < heroineWalkDuration)
            {
                float t = elapsedTime / heroineWalkDuration;
                heroineCharacter.transform.position = Vector3.Lerp(startPos, endPos, t);
                heroineCharacter.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 최종 위치 설정
            heroineCharacter.transform.position = endPos;
            heroineCharacter.transform.rotation = endRot;
            
            // 걷기 애니메이션 종료 (애니메이터가 있다면)
            if (heroineAnimator != null)
            {
                heroineAnimator.SetBool("IsWalking", false);
            }
            
            Debug.Log("Heroine character movement completed");
        }
        else
        {
            Debug.LogWarning("Heroine character movement setup incomplete");
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
        
        switch (firstSelectedChoice)
        {
            case 1:
                selectedTTSList = firstChoice1TTSList;
                break;
            case 2:
                selectedTTSList = firstChoice2TTSList;
                break;
            case 3:
                selectedTTSList = firstChoice3TTSList;
                break;
        }
        
        if (selectedTTSList != null && selectedTTSList.Count > 0)
        {
            Debug.Log($"Playing first choice {firstSelectedChoice} TTS");
            yield return StartCoroutine(PlayTTSList(selectedTTSList));
        }
    }
    
    /// <summary>
    /// 두 번째 선택에 따른 TTS 재생
    /// </summary>
    IEnumerator PlaySecondChoiceTTS()
    {
        List<AudioClip> selectedTTSList = null;
        
        switch (secondSelectedChoice)
        {
            case 1:
                selectedTTSList = secondChoice1TTSList;
                break;
            case 2:
                selectedTTSList = secondChoice2TTSList;
                break;
            case 3:
                selectedTTSList = secondChoice3TTSList;
                break;
        }
        
        if (selectedTTSList != null && selectedTTSList.Count > 0)
        {
            Debug.Log($"Playing second choice {secondSelectedChoice} TTS");
            yield return StartCoroutine(PlayTTSList(selectedTTSList));
        }
    }
    
    /// <summary>
    /// 최종 점수 계산 및 다음 단계 진행
    /// </summary>
    IEnumerator CalculateFinalScoreAndProceed()
    {
        // 호감도 점수 가져오기
        int affectionScore = 0;
        if (affectionManager != null)
        {
            affectionScore = affectionManager.GetCurrentAffection();
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
        
        // 다음 대화 컨트롤러 활성화
        if (nextDialogueController != null)
        {
            // 다음 컨트롤러에 점수 정보 전달 (필요시 구현)
            // var nextController = nextDialogueController.GetComponent<NextDialogueController>();
            // if (nextController != null)
            // {
            //     nextController.SetScores(gameScore, affectionScore, finalScore, firstSelectedChoice, secondSelectedChoice);
            // }
            
            nextDialogueController.SetActive(true);
        }
        
        Debug.Log("DialogueFlowControllerAfterGame completed");
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
        
        // 호감도 조정 (필요시)
        if (affectionManager != null)
        {
            affectionManager.AddAffection(5); // 예시
        }
    }
    
    /// <summary>
    /// 첫 번째 선택지 2 버튼 클릭
    /// </summary>
    public void OnFirstChoice2Selected()
    {
        firstSelectedChoice = 2;
        Debug.Log("First choice 2 selected");
        
        // 호감도 조정 (필요시)
        if (affectionManager != null)
        {
            affectionManager.AddAffection(3); // 예시
        }
    }
    
    /// <summary>
    /// 첫 번째 선택지 3 버튼 클릭
    /// </summary>
    public void OnFirstChoice3Selected()
    {
        firstSelectedChoice = 3;
        Debug.Log("First choice 3 selected");
        
        // 호감도 조정 (필요시)
        if (affectionManager != null)
        {
            affectionManager.AddAffection(1); // 예시
        }
    }
    
    /// <summary>
    /// 두 번째 선택지 1 버튼 클릭
    /// </summary>
    public void OnSecondChoice1Selected()
    {
        secondSelectedChoice = 1;
        Debug.Log("Second choice 1 selected");
        
        // 호감도 조정 (필요시)
        if (affectionManager != null)
        {
            affectionManager.AddAffection(5); // 예시
        }
    }
    
    /// <summary>
    /// 두 번째 선택지 2 버튼 클릭
    /// </summary>
    public void OnSecondChoice2Selected()
    {
        secondSelectedChoice = 2;
        Debug.Log("Second choice 2 selected");
        
        // 호감도 조정 (필요시)
        if (affectionManager != null)
        {
            affectionManager.AddAffection(3); // 예시
        }
    }
    
    /// <summary>
    /// 두 번째 선택지 3 버튼 클릭
    /// </summary>
    public void OnSecondChoice3Selected()
    {
        secondSelectedChoice = 3;
        Debug.Log("Second choice 3 selected");
        
        // 호감도 조정 (필요시)
        if (affectionManager != null)
        {
            affectionManager.AddAffection(1); // 예시
        }
    }
} 
