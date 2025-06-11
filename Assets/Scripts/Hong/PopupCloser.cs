using UnityEngine;

public class PopupCloser : MonoBehaviour
{
    public GameObject popupPanel;
    public GameObject BoxUnityChan;

    void OnEnable()
    {
        // 팝업이 활성화되면 3초 후에 자동으로 팝업 닫기
        Invoke("ClosePopup", 3f);
    }

    void OnDisable()
    {
        // 팝업이 비활성화되면 예약된 Invoke 취소
        CancelInvoke("ClosePopup");
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);  // 팝업 끄기
        Invoke("ShowBoxUnityChan", 1f);  // 1초 뒤 캐릭터 나타나기
    }

    void ShowBoxUnityChan()
    {
        BoxUnityChan.SetActive(true);
    }
}