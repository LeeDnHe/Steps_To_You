using UnityEngine;

public class GazeTrigger : MonoBehaviour
{
    public Transform npcObject;
    public Camera mainCam;
    public float delay = 5f;
    public GameObject popup;

    private bool isChecking = false;
    private float timer = 0f;
    private bool triggered = false;

    public void StartGazeCheck()
    {
        isChecking = true;
        timer = 0f;
        popup.SetActive(false);
        triggered = false;
    }

    void Update()
    {
        if (!isChecking || triggered) return;

        Vector3 dir = (npcObject.position - mainCam.transform.position).normalized;
        float dot = Vector3.Dot(mainCam.transform.forward, dir);

        if (dot > 0.85f)
        {
            popup.SetActive(false);
            triggered = true;
            isChecking = false;

            // 여기 중요! 다음 대사로 넘겨줌
            FindObjectOfType<FlexibleDialogueSystem>()?.NotifyEventCompleted();
        }
        else
        {
            timer += Time.deltaTime;
            if (timer >= delay)
            {
                popup.SetActive(true);
            }
        }
    }
}
