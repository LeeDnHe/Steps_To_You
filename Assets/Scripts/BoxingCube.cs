using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 개별 큐브를 관리하는 컴포넌트
public class BoxingCube : MonoBehaviour
{
    private BoxingManager manager;
    private bool isLeftCube;
    private bool hasBeenHit = false;
    
    [Header("Timer Settings")]
    public float LifeTime = 10f; // 큐브가 살아있을 시간 (초)
    
    public void Initialize(BoxingManager boxingManager, bool isLeft)
    {
        manager = boxingManager;
        isLeftCube = isLeft;
        
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
        
        // 아직 파괴되지 않았다면 자동으로 파괴
        if (this != null && !hasBeenHit)
        {
            Debug.Log($"{(isLeftCube ? "Left" : "Right")} cube auto-destroyed after {LifeTime} seconds");
            
            // 시간 초과로 사라질 때도 DestroySound 재생
            if (manager != null)
            {
                manager.PlayDestroySound();
            }
            
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenHit || manager == null) return;
        
        // DestroyCollider와 충돌했을 때 큐브 파괴
        if (other == manager.DestroyCollider)
        {
            Debug.Log($"{(isLeftCube ? "Left" : "Right")} cube destroyed by DestroyCollider");
            
            Destroy(gameObject);
            return;
        }
        
        // 컨트롤러와 충돌했는지 확인
        if (other == manager.LeftController || other == manager.RightController)
        {
            hasBeenHit = true;
            bool isCorrectController = manager.IsCorrectController(isLeftCube, other);
            
            manager.OnCubeHit(this, isCorrectController);
            
            Debug.Log($"{(isLeftCube ? "Left" : "Right")} cube hit by {other.name} - {(isCorrectController ? "CORRECT" : "WRONG")}");
        }
    }
} 