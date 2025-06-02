using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerManager : MonoBehaviour
{
    public GameObject rayController;

    public Transform startTf;
    private RaycastHit hitInfo;
    private Color color;

    private void Update()
    {
        if(Physics.Raycast(startTf.position, startTf.forward, out hitInfo, Mathf.Infinity))
        {
            if(hitInfo.collider.tag == "UI")
            {
                rayController.SetActive(true);
            }
        }
        else
        {
            rayController.SetActive(false);
        }
    }
}
