using UnityEngine;
using System.Collections;

public class DialogueFlowController : MonoBehaviour
{
    public AudioSource npcAudio;

    public AudioClip[] npcLines; // 0: wav1 ~ 7, 7: wav8

    public GameObject dialogueDesk;  // 🔥 주인공 대화 UI로 통일

    public GameObject NPCPanel;     // 선택지 3개
    public GameObject NPCPanel2;    // 선택지 2개

    private bool playerClosedDialogue = false;
    private int choiceFromPanel = -1;
    private int choiceFromPanel2 = -1;

    public GameObject DialogueFlowController2;

    void Start()
    {
        StartCoroutine(RunDialogue());
    }

    IEnumerator RunDialogue()
    {
        yield return new WaitForSeconds(1f);

        npcAudio.clip = npcLines[0]; // wav1
        npcAudio.Play();
        yield return new WaitUntil(() => !npcAudio.isPlaying);
        yield return new WaitForSeconds(0.5f);

        npcAudio.clip = npcLines[1]; // wav2
        npcAudio.Play();
        yield return new WaitUntil(() => !npcAudio.isPlaying);
        yield return new WaitForSeconds(0.5f);

        // 🔥 dialogueDesk UI 활성화
        dialogueDesk.SetActive(true);

        // 🔥 닫기 버튼 누를 때까지 기다림
        yield return new WaitUntil(() => playerClosedDialogue);

        // 🔥 UI 비활성화
        dialogueDesk.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        npcAudio.clip = npcLines[2]; // wav3
        npcAudio.Play();
        yield return new WaitUntil(() => !npcAudio.isPlaying);
        yield return new WaitForSeconds(0.5f);

        NPCPanel.SetActive(true);
        yield return new WaitUntil(() => choiceFromPanel != -1); // 선택지 1 클릭 대기

        NPCPanel.SetActive(false);

        switch (choiceFromPanel)
        {
            case 0: // 1번 선택 → wav4
                npcAudio.clip = npcLines[3];
                npcAudio.Play();
                yield return new WaitForSeconds(0.5f);
                break;

            case 1: // 2번 선택 → wav5 + 추가 선택
                npcAudio.clip = npcLines[4];
                npcAudio.Play();
                yield return new WaitUntil(() => !npcAudio.isPlaying);

                NPCPanel2.SetActive(true);
                yield return new WaitUntil(() => choiceFromPanel2 != -1);
                NPCPanel2.SetActive(false);

                if (choiceFromPanel2 == 0)
                    npcAudio.clip = npcLines[6]; // wav7
                else
                    npcAudio.clip = npcLines[7]; // wav8

                npcAudio.Play();
                yield return new WaitForSeconds(0.5f);
                break;

            case 2: // 3번 선택 → wav6
                npcAudio.clip = npcLines[5];
                npcAudio.Play();
                yield return new WaitForSeconds(0.5f);
                break;
        }

        yield return new WaitUntil(() => !npcAudio.isPlaying);
        Debug.Log("대사 시퀀스 완료");

        // 🔥 1초 기다림
        yield return new WaitForSeconds(1f);

        // 🔥 두번째 씬 오브젝트 활성화 → OnEnable() → 대사 자동 시작
        if (DialogueFlowController2 != null)
        {
            DialogueFlowController2.SetActive(true);
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
