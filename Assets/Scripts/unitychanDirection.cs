using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unitychanDirection : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform xrOrigin; // XR Origin 참조
    
    [Header("Rotation Settings")]
    public float rotationSpeed = 2.0f; // 회전 속도 (낮을수록 부드럽게)
    public float minAngleThreshold = 5.0f; // 최소 각도 차이 (이 값보다 작으면 회전하지 않음)
    
    // Start is called before the first frame update
    void Start()
    {
        // XR Origin이 할당되지 않았다면 자동으로 찾기
        if (xrOrigin == null)
        {
            GameObject xrOriginObj = GameObject.FindWithTag("MainCamera");
            if (xrOriginObj != null)
            {
                xrOrigin = xrOriginObj.transform.parent; // 보통 XR Origin이 MainCamera의 부모
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (xrOrigin != null)
        {
            LookAtXROriginSmoothly();
        }
    }
    
    void LookAtXROriginSmoothly()
    {
        // XR Origin 방향 계산 (Y축 회전만 고려)
        Vector3 targetDirection = xrOrigin.position - transform.position;
        targetDirection.y = 0; // Y축 무시 (수평 회전만)
        
        // 방향이 너무 가까우면 회전하지 않음
        if (targetDirection.magnitude < 0.1f)
            return;
            
        // 목표 회전 계산
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        
        // 현재 회전과 목표 회전 사이의 각도 차이 계산
        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
        
        // 각도 차이가 최소 임계값보다 클 때만 회전
        if (angleDifference > minAngleThreshold)
        {
            // 부드럽게 회전 (Lerp 사용)
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
