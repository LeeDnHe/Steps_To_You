using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DialogueFlowController2 : MonoBehaviour
{
    private AudioSource npcAudio;
    public AudioClip[] npcLines; // [0] ~ [6] : wav0 ~ wav6

    public GameObject dialogueGiveup1;
    public GameObject dialogueGiveup2;
    public GameObject dialogueJjo;
    
    public BoxingInitializer boxingInitializer; // 복싱 게임 초기화기 참조
    public VariousAudioController variousAudioController; // 오디오 관리 컨트롤러
    
    [Header("Character Animation")]
    public Animator heroineAnimator; // 여주인공 애니메이터

    private bool closedGiveup1 = false;
    private bool closedGiveup2 = false;
    private bool closedJjo = false;

    void OnEnable()
    {
        // AudioSource 초기화
        if (npcAudio == null)
        {
            npcAudio = GetComponent<AudioSource>();
        }
        
        // 복싱 게임 초기 비활성화
        if (boxingInitializer != null)
        {
            boxingInitializer.gameObject.SetActive(false);
        }
        
        StartCoroutine(RunDialogue());
    }
    
    void OnDisable()
    {
        // GameObject가 비활성화될 때 오디오 정지
        if (npcAudio != null && npcAudio.isPlaying)
        {
            npcAudio.Stop();
            Debug.Log("DialogueFlowController2 audio stopped on disable");
        }
        
        // 실행 중인 코루틴도 정리
        StopAllCoroutines();
    }

    IEnumerator RunDialogue()
    {
        npcAudio.clip = npcLines[0]; // wav0
        npcAudio.Play();
        
        // wav0 -> take_07 애니메이션 재생
        if (heroineAnimator != null)
        {
            heroineAnimator.Play("take_07");
        }
        
        yield return new WaitUntil(() => !npcAudio.isPlaying);
        yield return new WaitForSeconds(0.5f);

        npcAudio.clip = npcLines[1]; // wav1
        npcAudio.Play();
        
        // wav1 -> take_08 애니메이션 재생
        if (heroineAnimator != null)
        {
            heroineAnimator.Play("take_08");
        }
        
        yield return new WaitUntil(() => !npcAudio.isPlaying);

        yield return new WaitForSeconds(0.5f);
        
        // dialogueGiveup1 등장 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceAppear();
        }
        
        dialogueGiveup1.SetActive(true);
        yield return new WaitUntil(() => closedGiveup1);
        dialogueGiveup1.SetActive(false);

        // dialogueGiveup2 등장 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceAppear();
        }

        dialogueGiveup2.SetActive(true);
        yield return new WaitUntil(() => closedGiveup2);
        dialogueGiveup2.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        npcAudio.clip = npcLines[2]; // wav2
        npcAudio.Play();
        
        // wav2 -> take_09 애니메이션 재생
        if (heroineAnimator != null)
        {
            heroineAnimator.Play("take_09");
        }
        
        yield return new WaitUntil(() => !npcAudio.isPlaying);

        npcAudio.clip = npcLines[3]; // wav3
        npcAudio.Play();
        
        // wav3 -> take_10 애니메이션 재생
        if (heroineAnimator != null)
        {
            heroineAnimator.Play("take_10");
        }
        
        yield return new WaitUntil(() => !npcAudio.isPlaying);

        npcAudio.clip = npcLines[4]; // wav4
        npcAudio.Play();
        
        // wav4 -> take_11 애니메이션 재생
        if (heroineAnimator != null)
        {
            heroineAnimator.Play("take_11");
        }
        
        yield return new WaitUntil(() => !npcAudio.isPlaying);

        // dialogueJjo 등장 사운드 재생
        if (variousAudioController != null)
        {
            variousAudioController.PlayChoiceAppear();
        }

        dialogueJjo.SetActive(true);
        yield return new WaitUntil(() => closedJjo);
        dialogueJjo.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        npcAudio.clip = npcLines[5]; // wav5
        npcAudio.Play();
        
        // wav5 -> take_12 애니메이션 재생
        if (heroineAnimator != null)
        {
            heroineAnimator.Play("take_12");
        }
        
        yield return new WaitUntil(() => !npcAudio.isPlaying);

        yield return new WaitForSeconds(0.5f);
        npcAudio.clip = npcLines[6]; // wav6
        npcAudio.Play();
        
        // wav6 -> take_13 애니메이션 재생 (take_13이 없다면 적절한 애니메이션으로 대체)
        if (heroineAnimator != null)
        {
            heroineAnimator.Play("take_13");
        }
        
        yield return new WaitUntil(() => !npcAudio.isPlaying);

        Debug.Log("DialogueFlowController2 완료");
        
        // 1초 대기 후 복싱 게임 시작
        yield return new WaitForSeconds(1f);
        StartBoxingGame();
    }
    
    /// <summary>
    /// 복싱 게임 시작
    /// </summary>
    private void StartBoxingGame()
    {
        if (boxingInitializer != null)
        {
            Debug.Log("복싱 게임 시작");
            boxingInitializer.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("BoxingInitializer가 할당되지 않았습니다!");
        }
        
        // 현재 오브젝트 비활성화 (다음 파트로 넘어가므로)
        gameObject.SetActive(false);
    }

    // 🔻 각 패널의 닫기 버튼에 연결
    public void OnCloseGiveup1()
    {
        closedGiveup1 = true;
    }

    public void OnCloseGiveup2()
    {
        closedGiveup2 = true;
    }

    public void OnCloseJjo()
    {
        closedJjo = true;
    }
}

