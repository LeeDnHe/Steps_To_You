using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class DialogueFlowControllerAfterGame : MonoBehaviour
{
    [Header("TTS Audio Lists")]
    public List<AudioClip> preChoiceTTSList = new List<AudioClip>(); // 선택지 전
    public List<AudioClip> choice1TTSList = new List<AudioClip>();   // 선택지 1
    public List<AudioClip> choice2TTSList = new List<AudioClip>();   // 선택지 2
    public List<AudioClip> choice3TTSList = new List<AudioClip>();   // 선택지 3
    public List<AudioClip> postChoiceTTSList = new List<AudioClip>(); // 선택지 후
    
    [Header("Audio Source")]
    public AudioSource audioSource;
    
    [Header("Various Audio Controller")]
    public VariousAudioController variousAudioController; // 오디오 관리 컨트롤러
    
    [Header("UI Panels")]
    public GameObject choicePanel;        // 선택지 패널 (버튼 1,2,3)
    public GameObject popup1Panel;        // 선택지 1 팝업 패널 (2초간 표시)
    public GameObject popup2Panel;        // 선택지 2 팝업 패널 (2초간 표시)
    public GameObject popup3Panel;        // 선택지 3 팝업 패널 (2초간 표시)
    public GameObject intermediatePanel;  // 중간 패널 (선택지후 2번 후)
    public GameObject finalPanel;         // 마지막 패널 (다음챕터/방돌아가기)
    
    [Header("Scene Names")]
    public string nextChapterSceneName = "NextChapter";
    public string mainRoomSceneName = "MainRoom";
    
    [Header("Settings")]
    public float delayBetweenTTS = 0.5f;
    public float popupDuration = 2.0f;
    
    // Internal variables
    private bool isPlayingTTS = false;
    private int selectedChoice = -1;
    private bool intermediateCompleted = false;
    
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
        
        // 모든 패널 비활성화
        SetAllPanelsInactive();
        
        // 대화 시작
        StartCoroutine(StartDialogueSequence());
    }
    
    void SetAllPanelsInactive()
    {
        if (choicePanel != null) choicePanel.SetActive(false);
        if (popup1Panel != null) popup1Panel.SetActive(false);
        if (popup2Panel != null) popup2Panel.SetActive(false);
        if (popup3Panel != null) popup3Panel.SetActive(false);
        if (intermediatePanel != null) intermediatePanel.SetActive(false);
        if (finalPanel != null) finalPanel.SetActive(false);
    }
    
    /// <summary>
    /// 전체 대화 시퀀스 시작
    /// </summary>
    IEnumerator StartDialogueSequence()
    {
        // 1. 선택지 전 TTS 재생
        yield return StartCoroutine(PlayTTSList(preChoiceTTSList));
        
        // 선택지 등장 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceAppear();
        }
        
        // 2. 선택지 패널 활성화
        if (choicePanel != null)
        {
            choicePanel.SetActive(true);
        }
        
        // 3. 선택지 선택 대기
        yield return new WaitUntil(() => selectedChoice != -1);
        
        // 4. 선택지 패널 비활성화
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
        
        // 5. 선택된 선택지에 따른 팝업 패널 2초간 표시
        yield return StartCoroutine(ShowPopupPanel());
        
        // 6. 선택된 선택지에 따른 TTS 재생
        yield return StartCoroutine(PlaySelectedChoiceTTS());
        
        // 7. 선택지 후 TTS 재생 (1~2번)
        yield return StartCoroutine(PlayPostChoiceTTS_Part1());
        
        // 8. 중간 패널 표시
        yield return StartCoroutine(ShowIntermediatePanel());
        
        // 9. 선택지 후 TTS 재생 (3번)
        yield return StartCoroutine(PlayPostChoiceTTS_Part2());
        
        // 파이널패널 등장 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceAppear();
        }
        
        // 10. 마지막 패널 표시
        if (finalPanel != null)
        {
            finalPanel.SetActive(true);
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
                
                // 다음 오디오 재생 전 0.5초 대기 (마지막이 아닌 경우)
                if (i < ttsList.Count - 1)
                {
                    yield return new WaitForSeconds(delayBetweenTTS);
                }
            }
        }
        
        isPlayingTTS = false;
    }
    
    /// <summary>
    /// 선택된 선택지에 따른 팝업 패널 2초간 표시
    /// </summary>
    IEnumerator ShowPopupPanel()
    {
        GameObject selectedPopup = null;
        
        // 선택된 선택지에 따라 해당 팝업 선택
        switch (selectedChoice)
        {
            case 1:
                selectedPopup = popup1Panel;
                break;
            case 2:
                selectedPopup = popup2Panel;
                break;
            case 3:
                selectedPopup = popup3Panel;
                break;
        }
        
        // 선택된 팝업 표시
        if (selectedPopup != null)
        {
            selectedPopup.SetActive(true);
            yield return new WaitForSeconds(popupDuration);
            selectedPopup.SetActive(false);
        }
    }
    
    /// <summary>
    /// 선택된 선택지에 따른 TTS 재생
    /// </summary>
    IEnumerator PlaySelectedChoiceTTS()
    {
        List<AudioClip> selectedTTSList = null;
        
        switch (selectedChoice)
        {
            case 1:
                selectedTTSList = choice1TTSList;
                break;
            case 2:
                selectedTTSList = choice2TTSList;
                break;
            case 3:
                selectedTTSList = choice3TTSList;
                break;
        }
        
        if (selectedTTSList != null)
        {
            yield return StartCoroutine(PlayTTSList(selectedTTSList));
        }
    }
    
    /// <summary>
    /// 선택지 후 TTS 1~2번 재생
    /// </summary>
    IEnumerator PlayPostChoiceTTS_Part1()
    {
        if (postChoiceTTSList != null && postChoiceTTSList.Count >= 2)
        {
            List<AudioClip> part1List = new List<AudioClip>();
            part1List.Add(postChoiceTTSList[0]);
            part1List.Add(postChoiceTTSList[1]);
            
            yield return StartCoroutine(PlayTTSList(part1List));
        }
    }
    
    /// <summary>
    /// 중간 패널 표시 및 대기
    /// </summary>
    IEnumerator ShowIntermediatePanel()
    {
        if (intermediatePanel != null)
        {
            // 중간패널 등장 사운드 재생
            if (variousAudioController != null)
            {
                variousAudioController.PlayChoiceAppear();
            }
            
            intermediatePanel.SetActive(true);
            
            // 패널이 꺼질 때까지 대기
            yield return new WaitUntil(() => intermediateCompleted);
            
            intermediatePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 선택지 후 TTS 3번 재생
    /// </summary>
    IEnumerator PlayPostChoiceTTS_Part2()
    {
        if (postChoiceTTSList != null && postChoiceTTSList.Count >= 3)
        {
            List<AudioClip> part2List = new List<AudioClip>();
            part2List.Add(postChoiceTTSList[2]);
            
            yield return StartCoroutine(PlayTTSList(part2List));
        }
    }
    
    // ===== 버튼 이벤트 함수들 =====
    
    /// <summary>
    /// 선택지 1 버튼 클릭
    /// </summary>
    public void OnChoice1Selected()
    {
        selectedChoice = 1;
    }
    
    /// <summary>
    /// 선택지 2 버튼 클릭
    /// </summary>
    public void OnChoice2Selected()
    {
        selectedChoice = 2;
    }
    
    /// <summary>
    /// 선택지 3 버튼 클릭
    /// </summary>
    public void OnChoice3Selected()
    {
        selectedChoice = 3;
    }
    
    /// <summary>
    /// 중간 패널 닫기 버튼 클릭
    /// </summary>
    public void OnIntermediatePanelClosed()
    {
        intermediateCompleted = true;
    }
    
    /// <summary>
    /// 다음 챕터 버튼 클릭
    /// </summary>
    public void OnNextChapterClicked()
    {
        SceneManager.LoadScene(nextChapterSceneName);
    }
    
    /// <summary>
    /// 방으로 돌아가기 버튼 클릭
    /// </summary>
    public void OnReturnToRoomClicked()
    {
        SceneManager.LoadScene(mainRoomSceneName);
    }
} 
