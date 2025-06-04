using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestButton : MonoBehaviour
{
    public GameObject targetObject;
    public GameObject uiCanvas;

    public void ClosePanel()
    {
        if (uiCanvas != null)
        {
            uiCanvas.SetActive(false);
        }
    }
    public void OnOffDesk()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);
        }
    }


}
