using UnityEngine;

public class FollowCameraUI : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector3 offset = new Vector3(0, 0, 2.0f);

    void LateUpdate()
    {
        if (cameraTransform != null)
        {
            transform.position = cameraTransform.position + cameraTransform.forward * offset.z + cameraTransform.up * offset.y;
            transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
        }
    }
}
