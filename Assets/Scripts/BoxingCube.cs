using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 개별 큐브를 관리하는 컴포넌트
public class BoxingCube : MonoBehaviour
{
    private BoxingManager manager;
    private bool isLeftCube;
    private bool isForbiddenCube;
    private bool hasBeenHit = false;
    
    [Header("Timer Settings")]
    public float LifeTime = 10f; // 큐브가 살아있을 시간 (초)
    
    public bool IsForbiddenCube => isForbiddenCube;
    public bool IsLeftCube => isLeftCube;
    
    public void Initialize(BoxingManager boxingManager, bool isLeft, bool isForbidden = false)
    {
        manager = boxingManager;
        isLeftCube = isLeft;
        isForbiddenCube = isForbidden;
        
        // Collider가 Trigger인지 확인
        Collider cubeCollider = GetComponent<Collider>();
        if (cubeCollider != null)
        {
            cubeCollider.isTrigger = true;
        }
        
        // 10초 후 자동 삭제 타이머 시작
        StartCoroutine(AutoDestroyTimer());
    }
    
    private IEnumerator AutoDestroyTimer()
    {
        yield return new WaitForSeconds(LifeTime);
        
        // 아직 파괴되지 않았다면 놓친 것으로 처리
        if (this != null && !hasBeenHit)
        {
            Debug.Log($"{(isForbiddenCube ? "Forbidden" : (isLeftCube ? "Left" : "Right"))} cube missed after {LifeTime} seconds");
            
            // 놓친 큐브로 처리 (콤보 초기화 등)
            if (manager != null)
            {
                manager.OnCubeMissed(this);
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenHit || manager == null) return;
        
        // DestroyCollider와 충돌했을 때 놓친 큐브로 처리
        if (other == manager.DestroyCollider)
        {
            Debug.Log($"{(isForbiddenCube ? "Forbidden" : (isLeftCube ? "Left" : "Right"))} cube missed by DestroyCollider");
            
            manager.OnCubeMissed(this);
            return;
        }
        
        // 컨트롤러와 충돌했는지 확인
        if (other == manager.LeftController || other == manager.RightController)
        {
            hasBeenHit = true;
            bool isCorrectController = manager.IsCorrectController(isLeftCube, other);
            
            // hitController 정보도 함께 전달
            manager.OnCubeHit(this, isCorrectController, other);
        }
    }
} 