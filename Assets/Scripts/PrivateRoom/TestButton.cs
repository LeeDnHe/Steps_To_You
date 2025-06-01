using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestButton : MonoBehaviour
{
    public GameObject targetObject;

    public void OnOffDesk()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);
        }
    }
}
