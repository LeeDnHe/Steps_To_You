using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DialogueFlowController : MonoBehaviour
{
    private AudioSource npcAudio;

    public AudioClip[] npcLines; // 0: wav1 ~ 7, 7: wav8

    public GameObject dialogueDesk;  // 🔥 주인공 대화 UI로 통일

    public GameObject NPCPanel;     // 선택지 3개
    public GameObject NPCPanel2;    // 선택지 2개

    public GameObject DialogueFlowController2;
    
    public VariousAudioController variousAudioController; // 오디오 관리 컨트롤러
    public BackgroundSoundManager backgroundSoundManager; // 배경음악 매니저
    
    [Header("Character Animation")]
    public Animator heroineAnimator; // 여주인공 애니메이터

    private bool playerClosedDialogue = false;
    private int choiceFromPanel = -1;
    private int choiceFromPanel2 = -1;

    private bool isRunning = false; // 코루틴 중복 실행 방지

    void Start()
    {
        // 같은 오브젝트의 AudioSource 컴포넌트 가져오기
        npcAudio = GetComponent<AudioSource>();
        
        // BackgroundSoundManager 초기화
        if (backgroundSoundManager == null)
        {
            backgroundSoundManager = BackgroundSoundManager.Instance;
        }
        
        if (!isRunning)
        {
            StartCoroutine(RunDialogue());
        }
    }
    
    void OnDisable()
    {
        // GameObject가 비활성화될 때 오디오 정지
        if (npcAudio != null && npcAudio.isPlaying)
        {
            npcAudio.Stop();
            Debug.Log("DialogueFlowController audio stopped on disable");
        }
        
        // 실행 중인 코루틴도 정리
        StopAllCoroutines();
        isRunning = false;
    }

    IEnumerator RunDialogue()
    {
        isRunning = true;
        yield return new WaitForSecondsRealtime(1f);

        npcAudio.clip = npcLines[0]; // wav1
        npcAudio.Play();
        
        // wav1 -> take_01 애니메이션 재생
        if (heroineAnimator != null)
        {
            heroineAnimator.Play("take_01");
        }
        
        yield return new WaitUntil(() => !npcAudio.isPlaying);
        yield return new WaitForSecondsRealtime(0.5f);

        npcAudio.clip = npcLines[1]; // wav2
        npcAudio.Play();
        
        // wav2 -> take_02 애니메이션 재생
        if (heroineAnimator != null)
        {
            heroineAnimator.Play("take_02");
        }
        
        yield return new WaitUntil(() => !npcAudio.isPlaying);
        yield return new WaitForSecondsRealtime(0.5f);

        // 🔥 dialogueDesk UI 활성화
        dialogueDesk.SetActive(true);

        // 대화 시작 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayMessagePopupOpenSequence();
        }

        // 🔥 닫기 버튼 누를 때까지 기다림
        yield return new WaitUntil(() => playerClosedDialogue);

        // 🔥 UI 비활성화
        dialogueDesk.SetActive(false);
        yield return new WaitForSecondsRealtime(0.5f);

        npcAudio.clip = npcLines[2]; // wav3
        npcAudio.Play();
        
        // wav3 -> take_03 애니메이션 재생
        if (heroineAnimator != null)
        {
            heroineAnimator.Play("take_03");
        }
        
        yield return new WaitUntil(() => !npcAudio.isPlaying);
        yield return new WaitForSecondsRealtime(0.5f);

        // 선택지 등장 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceAppear();
        }

        NPCPanel.SetActive(true);
        yield return new WaitUntil(() => choiceFromPanel != -1); // 선택지 1 클릭 대기

        NPCPanel.SetActive(false);

        switch (choiceFromPanel)
        {
            case 0: // 1번 선택 → wav4
                npcAudio.clip = npcLines[3];
                npcAudio.Play();
                
                // wav4 -> take_04 애니메이션 재생
                if (heroineAnimator != null)
                {
                    heroineAnimator.Play("take_04");
                }
                
                yield return new WaitUntil(() => !npcAudio.isPlaying);
                break;

            case 1: // 2번 선택 → wav5 + 추가 선택
                npcAudio.clip = npcLines[4];
                npcAudio.Play();
                
                // wav5 -> take_05 애니메이션 재생
                if (heroineAnimator != null)
                {
                    heroineAnimator.Play("take_05");
                }
                
                yield return new WaitUntil(() => !npcAudio.isPlaying);

                // 두 번째 선택지 등장 사운드 재생
                if (variousAudioController != null)
                {
                    variousAudioController.PlayChoiceAppear();
                }

                NPCPanel2.SetActive(true);
                yield return new WaitUntil(() => choiceFromPanel2 != -1);
                NPCPanel2.SetActive(false);

                if (choiceFromPanel2 == 0)
                {
                    npcAudio.clip = npcLines[6]; // wav7
                    npcAudio.Play();
                    
                    // wav7 -> take_04 애니메이션 재생
                    if (heroineAnimator != null)
                    {
                        heroineAnimator.Play("take_04");
                    }
                }
                else
                {
                    npcAudio.clip = npcLines[7]; // wav8
                npcAudio.Play();
                    
                    // wav8 -> take_06 애니메이션 재생
                    if (heroineAnimator != null)
                    {
                        heroineAnimator.Play("take_06");
                    }
                }

                yield return new WaitUntil(() => !npcAudio.isPlaying);
                break;

            case 2: // 3번 선택 → wav6
                npcAudio.clip = npcLines[5];
                npcAudio.Play();
                
                // wav6 -> take_06 애니메이션 재생
                if (heroineAnimator != null)
                {
                    heroineAnimator.Play("take_06");
                }
                
                yield return new WaitUntil(() => !npcAudio.isPlaying);
                break;
        }

        yield return new WaitUntil(() => !npcAudio.isPlaying);
        Debug.Log("대사 시퀀스 완료");

        // 🔥 1초 기다림
        yield return new WaitForSecondsRealtime(1f);

        // 🔥 두번째 씬 오브젝트 활성화 → OnEnable() → 대사 자동 시작
        if (DialogueFlowController2 != null)
        {
            DialogueFlowController2.SetActive(true);
        }

        // 🔥 현재 오브젝트 비활성화 (다음 파트로 넘어가므로)
        gameObject.SetActive(false);

        isRunning = false;
    }

    /// <summary>
    /// TTS 재생 with 배경음악 볼륨 조절
    /// </summary>
    IEnumerator PlayTTSWithVolumeControl(AudioClip clip, string animationName = "")
    {
        if (clip == null || npcAudio == null) yield break;
        
        // TTS 시작 시 배경음악 볼륨 낮춤
        if (backgroundSoundManager != null)
        {
            backgroundSoundManager.DuckVolumeForTTS();
        }
        
        npcAudio.clip = clip;
        npcAudio.Play();
        
        // 애니메이션 재생
        if (heroineAnimator != null && !string.IsNullOrEmpty(animationName))
        {
            heroineAnimator.Play(animationName);
        }
        
        yield return new WaitUntil(() => !npcAudio.isPlaying);
        
        // TTS 종료 시 배경음악 볼륨 복원
        if (backgroundSoundManager != null)
        {
            backgroundSoundManager.RestoreVolumeAfterTTS();
        }
    }

    // 🔻 UI 버튼 또는 Ray로 호출할 함수들
    public void OnPlayerDialogueClosed()
    {
        playerClosedDialogue = true;
    }

    public void OnPanelChoice(int index)
    {
        choiceFromPanel = index;
    }

    public void OnPanel2Choice(int index)
    {
        choiceFromPanel2 = index;
    }
}
