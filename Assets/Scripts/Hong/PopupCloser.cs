using UnityEngine;

public class PopupCloser : MonoBehaviour
{
    public GameObject popupPanel;
    public GameObject BoxUnityChan;
    public GameObject DialogueFlowController;

    private bool isClosing = false; // 중복 실행 방지

    void OnEnable()
    {
        Debug.Log("PopupCloser OnEnable 실행");
        // 팝업이 활성화되면 3초 후에 자동으로 팝업 닫기
        if (!isClosing)
        {
            Invoke("ClosePopup", 3f);
        }
    }

    void OnDisable()
    {
        // 팝업이 비활성화되면 예약된 Invoke 취소
        CancelInvoke("ClosePopup");
    }

    public void ClosePopup()
    {
        if (isClosing) return; // 이미 실행 중이면 리턴
        isClosing = true;
        
        Debug.Log("ClosePopup 실행");
        popupPanel.SetActive(false);  // 팝업 끄기
        
        if (DialogueFlowController != null)
        {
            bool isActive = DialogueFlowController.activeSelf;
            Debug.Log("DialogueFlowController.activeSelf: " + isActive);
            
            if (!isActive)
            {
                Debug.Log("DialogueFlowController 활성화");
                DialogueFlowController.SetActive(true);
            }
            else
            {
                Debug.Log("DialogueFlowController 이미 활성화됨 - 중복 활성화 방지");
            }
        }
        
        Invoke("ShowBoxUnityChan", 1f);  // 1초 뒤 캐릭터 나타나기
    }

    void ShowBoxUnityChan()
    {
        BoxUnityChan.SetActive(true);
    }
}