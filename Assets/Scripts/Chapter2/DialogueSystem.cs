using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public enum DialogueWaitType
{
    None,           // ��� ����
    WaitForClick,   // Ŭ�� �� ����
    WaitForEvent    // �̺�Ʈ �߻� �� ���� (��: �ڵ��ƺ���)
}
public class FlexibleDialogueSystem : MonoBehaviour
{

    [System.Serializable]
    public class DialogueLine
    {
        public string text;
        public DialogueWaitType waitType = DialogueWaitType.WaitForClick;
        public UnityEngine.Events.UnityEvent onEnterAction;  // �� ��� ������ ������ �׼�
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
                Invoke(nameof(AdvanceLine), 2f);  // 2�� �� �ڵ� ����
                break;
            case DialogueWaitType.WaitForEvent:
                waitingForEvent = true;
                break;
        }
    }

    // �ܺο��� ȣ�� (��: NPC �ڵ��ƺ�)
    public void NotifyEventCompleted()
    {
        if (waitingForEvent)
        {
            waitingForEvent = false;
            AdvanceLine();
        }
    }
}
