using UnityEngine;
using TMPro;

public class BranchingDialogue : MonoBehaviour
{
    public GameObject affectionTextObj;
    public TextMeshProUGUI affectionText;
    public GameObject currentPanel;

    private int affection = 0;

    public void OnChoiceButtonClicked(GameObject nextPanel, int affectionChange)
    {
        affection += affectionChange;
        ShowAffectionChange($"+{affectionChange}");

        if (currentPanel != null)
            currentPanel.SetActive(false);

        nextPanel.SetActive(true);
        currentPanel = nextPanel;
    }

    // 👉 중간 함수 예시
    public void OnClick_GoToPanel1()
    {
        OnChoiceButtonClicked(GameObject.Find("NPCPanel1"), 20);
    }

    public void OnClick_GoToPanel2()
    {
        OnChoiceButtonClicked(GameObject.Find("NPCPanel2"), 15);
    }

    public void OnClick_GoToPanel3()
    {
        OnChoiceButtonClicked(GameObject.Find("NPCPanel3"), 10);
    }

    public void OnClick_GoToPanel21()
    {
        OnChoiceButtonClicked(GameObject.Find("NPCPanel21"), 5);
    }

    public void OnClick_GoToPanel22()
    {
        OnChoiceButtonClicked(GameObject.Find("NPCPanel22"), 0);
    }

    void ShowAffectionChange(string text)
    {
        affectionText.text = text;
        affectionTextObj.SetActive(true);
        Invoke("HideAffectionChange", 1.5f);
    }

    void HideAffectionChange()
    {
        affectionTextObj.SetActive(false);
    }
}
