using UnityEngine;

public class CapsuleClickHandler : MonoBehaviour
{
    public GameObject buttonCanvas; // 버튼이 있는 캔버스

    void Start()
    {
        buttonCanvas.SetActive(false); // 처음엔 숨김
    }

    public void OnCapsuleClicked()
    {
        buttonCanvas.SetActive(true); // 클릭 시 버튼 표시
    }
}
