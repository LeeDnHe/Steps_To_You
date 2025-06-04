using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ShowUIOnClick : MonoBehaviour
{
    public GameObject uiCanvas;  // ¶ç¿ï Canvas

    void Start()
    {
        var interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
        {
            Debug.Log("Interactable found, listener added");
            interactable.selectEntered.AddListener(OnSelected);
        }
        else
        {
            Debug.LogError("XRBaseInteractable not found!");
        }
    }

    void OnSelected(SelectEnterEventArgs args)
    {
        Debug.Log("Select event triggered!");
        if (uiCanvas != null)
        {
            uiCanvas.SetActive(true);
        }
    }
}
