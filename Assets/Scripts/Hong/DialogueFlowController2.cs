using UnityEngine;
using System.Collections;

public class DialogueFlowController2 : MonoBehaviour
{
    public AudioSource npcAudio;
    public AudioClip[] npcLines; // [0] ~ [5] : wav0 ~ wav5

    public GameObject dialogueGiveup1;
    public GameObject dialogueGiveup2;
    public GameObject dialogueJjo;
    
    public BoxingInitializer boxingInitializer; // 복싱 게임 초기화기 참조

    private bool closedGiveup1 = false;
    private bool closedGiveup2 = false;
    private bool closedJjo = false;

    void OnEnable()
    {
        // 복싱 게임 초기 비활성화
        if (boxingInitializer != null)
        {
            boxingInitializer.gameObject.SetActive(false);
        }
        
        StartCoroutine(RunDialogue());
    }

    IEnumerator RunDialogue()
    {
        npcAudio.clip = npcLines[0]; // wav0
        npcAudio.Play();
        yield return new WaitUntil(() => !npcAudio.isPlaying);
        yield return new WaitForSeconds(0.5f);

        npcAudio.clip = npcLines[1]; // wav1
        npcAudio.Play();
        yield return new WaitUntil(() => !npcAudio.isPlaying);

        yield return new WaitForSeconds(0.5f);
        dialogueGiveup1.SetActive(true);
        yield return new WaitUntil(() => closedGiveup1);
        dialogueGiveup1.SetActive(false);

        dialogueGiveup2.SetActive(true);
        yield return new WaitUntil(() => closedGiveup2);
        dialogueGiveup2.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        npcAudio.clip = npcLines[2]; // wav2
        npcAudio.Play();
        yield return new WaitUntil(() => !npcAudio.isPlaying);

        npcAudio.clip = npcLines[3]; // wav3
        npcAudio.Play();
        yield return new WaitUntil(() => !npcAudio.isPlaying);

        dialogueJjo.SetActive(true);
        yield return new WaitUntil(() => closedJjo);
        dialogueJjo.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        npcAudio.clip = npcLines[4]; // wav4
        npcAudio.Play();
        yield return new WaitUntil(() => !npcAudio.isPlaying);

        yield return new WaitForSeconds(0.5f);
        npcAudio.clip = npcLines[5]; // wav5
        npcAudio.Play();
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

