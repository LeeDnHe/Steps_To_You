using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RayInteractorEvent : MonoBehaviour
{
    public XRRayInteractor xri; // �޼� �Ǵ� ������ ��Ʈ�ѷ� �Է�

    // Start is called before the first frame update
    void Start()
    {
        xri.selectEntered.AddListener(SelectEnterEvent);
        xri.selectExited.AddListener(SelectExitEvent);
    }
    
    // Interaction �۵��� �߰� �۵�
    private void SelectEnterEvent(SelectEnterEventArgs args) // Event ���Խ� �ۿ�
    {
        Debug.Log("Selected Enter");
    }
    private void SelectExitEvent(SelectExitEventArgs args) // Event Ż��� �ۿ�
    {
        Debug.Log("Selected Exit");
    }
}
