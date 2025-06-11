using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject alertPanel;

    public void ShowAlert()
    {
        alertPanel.SetActive(true);
    }
}