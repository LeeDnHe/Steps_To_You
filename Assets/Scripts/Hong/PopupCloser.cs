using UnityEngine;

public class PopupCloser : MonoBehaviour
{
    public GameObject popupPanel;
    public GameObject BoxUnityChan;

    public void ClosePopup()
    {
        popupPanel.SetActive(false);  // 팝업 끄기
        Invoke("ShowBoxUnityChan", 2f);  // 2초 뒤 캐릭터 나타나기
    }

    void ShowBoxUnityChan()
    {
        BoxUnityChan.SetActive(true);
    }
}