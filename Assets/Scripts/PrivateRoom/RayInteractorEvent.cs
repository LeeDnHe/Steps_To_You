using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RayInteractorEvent : MonoBehaviour
{
    public XRRayInteractor xri; // 왼손 또는 오른손 컨트롤러 입력

    // Start is called before the first frame update
    void Start()
    {
        xri.selectEntered.AddListener(SelectEnterEvent);
        xri.selectExited.AddListener(SelectExitEvent);
    }
    
    // Interaction 작동시 추가 작동
    private void SelectEnterEvent(SelectEnterEventArgs args) // Event 진입시 작용
    {
        Debug.Log("Selected Enter");
    }
    private void SelectExitEvent(SelectExitEventArgs args) // Event 탈출시 작용
    {
        Debug.Log("Selected Exit");
    }
}
