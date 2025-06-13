using UnityEngine;
using TMPro;

public class BranchingDialogue : MonoBehaviour
{
    /*public GameObject affectionTextObj;
    public TextMeshProUGUI affectionText;*/
    public GameObject currentPanel;

    private int affection = 0;

    public GameObject popup_0;
    public GameObject popup_5;
    public GameObject popup_10;
    public GameObject popup_15;
    public GameObject popup_20;

    public void OnChoiceButtonClicked(GameObject nextPanel, int affectionChange)
    {

        // 이미지 띄우기
        switch (affectionChange)
        {
            case 0: ShowPopupImage(popup_0); break;
            case 5: ShowPopupImage(popup_5); break;
            case 10: ShowPopupImage(popup_10); break;
            case 15: ShowPopupImage(popup_15); break;
            case 20: ShowPopupImage(popup_20); break;
        }

        if (currentPanel != null)
            currentPanel.SetActive(false);

        nextPanel.SetActive(true);
        currentPanel = nextPanel;
    }


    public GameObject panel1;
    public GameObject panel2;
    public GameObject panel3;
    public GameObject panel21;
    public GameObject panel22;

    public void OnClick_GoToPanel1()
    {
        OnChoiceButtonClicked(panel1, 20);
    }

    public void OnClick_GoToPanel2()
    {
        OnChoiceButtonClicked(panel2, 15);
    }

    public void OnClick_GoToPanel3()
    {
        OnChoiceButtonClicked(panel3, 10);
    }

    public void OnClick_GoToPanel21()
    {
        OnChoiceButtonClicked(panel21, 5);
    }

    public void OnClick_GoToPanel22()
    {
        OnChoiceButtonClicked(panel22, 0);
    }


    void ShowPopupImage(GameObject popupObj)
    {
        popupObj.SetActive(true);
        Invoke(nameof(HidePopupImage), 1.5f);  // 1.5초 후 숨김
        lastPopup = popupObj;
    }

    GameObject lastPopup;  // 마지막에 띄운 오브젝트 저장용

    void HidePopupImage()
    {
        if (lastPopup != null)
            lastPopup.SetActive(false);
    }

/*
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
    */
}
