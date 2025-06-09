using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public enum DialogueWaitType
{
    None,           // 즉시 진행
    WaitForClick,   // 클릭 후 진행
    WaitForEvent    // 이벤트 발생 시 진행 (예: 뒤돌아보기)
}
public class FlexibleDialogueSystem : MonoBehaviour
{

    [System.Serializable]
    public class DialogueLine
    {
        public string text;
        public DialogueWaitType waitType = DialogueWaitType.WaitForClick;
        public UnityEngine.Events.UnityEvent onEnterAction;  // 이 대사 직전에 실행할 액션
    }

    public Text dialogueText;
    public GameObject textPanel;
    public List<DialogueLine> dialogueLines;

    private int currentIndex = 0;
    private bool waitingForEvent = false;

    void Start()
    {
        PlayCurrentLine();
    }

    public void OnTextClicked()
    {
        if (waitingForEvent) return;

        var line = dialogueLines[currentIndex];
        if (line.waitType == DialogueWaitType.WaitForClick)
        {
            AdvanceLine();
        }
    }

    public void AdvanceLine()
    {
        currentIndex++;
        if (currentIndex < dialogueLines.Count)
        {
            PlayCurrentLine();
        }
        else
        {
            textPanel.SetActive(false);
        }
    }

    void PlayCurrentLine()
    {
        var line = dialogueLines[currentIndex];
        dialogueText.text = line.text;
        textPanel.SetActive(true);

        line.onEnterAction?.Invoke();

        switch (line.waitType)
        {
            case DialogueWaitType.None:
                Invoke(nameof(AdvanceLine), 2f);  // 2초 후 자동 진행
                break;
            case DialogueWaitType.WaitForEvent:
                waitingForEvent = true;
                break;
        }
    }

    // 외부에서 호출 (예: NPC 뒤돌아봄)
    public void NotifyEventCompleted()
    {
        if (waitingForEvent)
        {
            waitingForEvent = false;
            AdvanceLine();
        }
    }
}
