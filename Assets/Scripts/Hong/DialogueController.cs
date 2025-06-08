using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public GameObject dialogueUI;

    public void CloseDialogue()
    {
        dialogueUI.SetActive(false);
    }
}
