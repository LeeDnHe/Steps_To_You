using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PopupOpener : MonoBehaviour
{
    public GameObject popupPanel;

    void Start()
    {
        var interactable = GetComponent<XRBaseInteractable>();
        interactable.selectEntered.AddListener(OnSelected);
    }

    void OnSelected(SelectEnterEventArgs args)
    {
        popupPanel.SetActive(true);
    }
}
