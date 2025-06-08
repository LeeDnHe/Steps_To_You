using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BoxingManager : MonoBehaviour
{
    [Header("Cube Prefabs")]
    public GameObject LeftCube;
    public GameObject RightCube;
    
    [Header("Spawn Settings")]
    public Transform SpawnPoint;
    public float SpawnInterval = 0.5f;
    public int TotalCubes = 50;
    public float MoveSpeed = 5f;
    
    [Header("Controllers")]
    public Collider LeftController;
    public Collider RightController;
    public Collider DestroyCollider; // 큐브가 사라질 콜라이더
    
    [Header("Controller Transforms (for velocity)")]
    public Transform LeftControllerTransform;  // 왼쪽 컨트롤러 Transform
    public Transform RightControllerTransform; // 오른쪽 컨트롤러 Transform
    
    [Header("Hit Strength Settings")]
    public float MinHitVelocity = 2.0f; // 최소 충격 속도 (m/s)
    public float StrongHitVelocity = 4.0f; // 강한 충격 속도 (m/s)
    
    [Header("Audio")]
    public AudioClip DestroyAudio; // 큐브가 사라질 때 재생할 오디오
    public AudioClip WrongAudio; // 틀린 컨트롤러와 충돌할 때 재생할 오디오
    public AudioClip WeakHitAudio; // 약한 충격일 때 재생할 오디오
    public AudioClip StrongHitAudio; // 강한 충격일 때 재생할 오디오
    
    [Header("Game Status")]
    public int Score = 0;
    public int SpawnedCount = 0;
    
    private List<BoxingCube> activeCubes = new List<BoxingCube>();
    private AudioSource audioSource;
    
    // 실시간 속도 추적을 위한 변수들
    private Queue<Vector3> leftControllerPositions = new Queue<Vector3>();
    private Queue<Vector3> rightControllerPositions = new Queue<Vector3>();
    private Queue<float> positionTimes = new Queue<float>();
    private int maxPositionHistory = 10; // 최대 10개의 위치 기록
    
    void Start()
    {
        // AudioSource 컴포넌트 가져오기 또는 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        StartCoroutine(SpawnCubes());
    }
    
    void Update()
    {
        // 활성화된 큐브들을 -Z 방향으로 이동
        MoveCubes();
        
        // 화면 밖으로 나간 큐브들 제거
        CleanupCubes();
        
        // 실시간 컨트롤러 위치 추적
        TrackControllerPositions();
    }
    
    void TrackControllerPositions()
    {
        float currentTime = Time.time;
        
        // 시간 큐 관리
        positionTimes.Enqueue(currentTime);
        if (positionTimes.Count > maxPositionHistory)
        {
            positionTimes.Dequeue();
        }
        
        // 왼쪽 컨트롤러 위치 추적
        if (LeftControllerTransform != null)
        {
            leftControllerPositions.Enqueue(LeftControllerTransform.position);
            if (leftControllerPositions.Count > maxPositionHistory)
            {
                leftControllerPositions.Dequeue();
            }
        }
        
        // 오른쪽 컨트롤러 위치 추적
        if (RightControllerTransform != null)
        {
            rightControllerPositions.Enqueue(RightControllerTransform.position);
            if (rightControllerPositions.Count > maxPositionHistory)
            {
                rightControllerPositions.Dequeue();
            }
        }
    }
    
    IEnumerator SpawnCubes()
    {
        while (SpawnedCount < TotalCubes)
        {
            SpawnRandomCube();
            SpawnedCount++;
            
            yield return new WaitForSeconds(SpawnInterval);
        }
        
    }
    
    void SpawnRandomCube()
    {
        // 랜덤하게 Left 또는 Right 큐브 선택
        bool isLeftCube = Random.Range(0, 2) == 0;
        GameObject cubeToSpawn = isLeftCube ? LeftCube : RightCube;
        
        if (cubeToSpawn == null || SpawnPoint == null) return;
        
        // 스폰 포인트 기준 XY -0.5 ~ 0.5 정사각형에서 랜덤 위치
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f),
            0f
        );
        
        Vector3 spawnPosition = SpawnPoint.position + randomOffset;
        
        // 큐브 생성
        GameObject spawnedCube = Instantiate(cubeToSpawn, spawnPosition, Quaternion.identity);
        
        // BoxingCube 컴포넌트 추가/설정
        BoxingCube boxingCube = spawnedCube.GetComponent<BoxingCube>();
        if (boxingCube == null)
        {
            boxingCube = spawnedCube.AddComponent<BoxingCube>();
        }
        
        boxingCube.Initialize(this, isLeftCube);
        activeCubes.Add(boxingCube);
    }
    
    void MoveCubes()
    {
        for (int i = activeCubes.Count - 1; i >= 0; i--)
        {
            if (activeCubes[i] != null)
            {
                // -Z 방향으로 이동 (플레이어 쪽으로)
                activeCubes[i].transform.Translate(Vector3.back * MoveSpeed * Time.deltaTime);
            }
        }
    }
    
    void CleanupCubes()
    {
        // null인 큐브들만 리스트에서 제거 (충돌로 파괴된 큐브들)
        for (int i = activeCubes.Count - 1; i >= 0; i--)
        {
            if (activeCubes[i] == null)
            {
                activeCubes.RemoveAt(i);
            }
        }
    }
    
    public void OnCubeHit(BoxingCube cube, bool hitByCorrectController, Collider hitController)
    {
        if (hitByCorrectController)
        {
            // 충격 강도 측정
            float hitVelocity = GetControllerVelocity(hitController);
            HitStrength strength = GetHitStrength(hitVelocity);
            
            Debug.Log($"Hit velocity: {hitVelocity:F2} m/s - Strength: {strength}");
            
            // 충격 강도에 따른 처리
            switch (strength)
            {
                case HitStrength.TooWeak:
                    Debug.Log("Hit too weak! Try harder!");
                    PlayWeakHitSound();
                    // 큐브는 파괴되지 않음
                    break;
                    
                case HitStrength.Normal:
                    Score++;
                    Debug.Log($"Good hit! Score: {Score}");
                    PlayDestroySound();
                    activeCubes.Remove(cube);
                    Destroy(cube.gameObject);
                    break;
                    
                case HitStrength.Strong:
                    Score += 2; // 강한 충격은 2점
                    Debug.Log($"Excellent hit! Score: {Score} (+2 points)");
                    PlayStrongHitSound();
                    activeCubes.Remove(cube);
                    Destroy(cube.gameObject);
                    break;
            }
        }
        else
        {
            Debug.Log("Hit by wrong controller!");
            PlayWrongSound();
        }
    }
    
    private float GetControllerVelocity(Collider hitController)
    {
        // 실시간 위치 기록으로 속도 계산
        Queue<Vector3> positions = null;
        string controllerType = "";
        
        if (hitController == LeftController && LeftControllerTransform != null)
        {
            positions = leftControllerPositions;
            controllerType = "Left";
        }
        else if (hitController == RightController && RightControllerTransform != null)
        {
            positions = rightControllerPositions;
            controllerType = "Right";
        }
        else
        {
            Debug.LogWarning("Controller mapping issue - returning default velocity");
            return 0f;
        }
        
        if (positions != null && positions.Count >= 2)
        {
            // 최근 2개 위치로 속도 계산
            Vector3[] posArray = positions.ToArray();
            float[] timeArray = positionTimes.ToArray();
            
            int count = posArray.Length;
            Vector3 currentPos = posArray[count - 1];
            Vector3 prevPos = posArray[count - 2];
            float currentTime = timeArray[count - 1];
            float prevTime = timeArray[count - 2];
            
            Vector3 deltaPosition = currentPos - prevPos;
            float deltaTime = currentTime - prevTime;
            float velocity = deltaPosition.magnitude / deltaTime;
            
            Debug.Log($"{controllerType} Controller Velocity: {velocity:F2} m/s");
            
            return velocity;
        }
        else
        {
            Debug.LogWarning("Not enough position data for velocity calculation");
            return 0f;
        }
    }
    
    private HitStrength GetHitStrength(float velocity)
    {
        if (velocity < MinHitVelocity)
        {
            return HitStrength.TooWeak;
        }
        else if (velocity >= StrongHitVelocity)
        {
            return HitStrength.Strong;
        }
        else
        {
            return HitStrength.Normal;
        }
    }
    
    // 충격 강도 열거형
    public enum HitStrength
    {
        TooWeak,    // 너무 약함 - 실패
        Normal,     // 보통 - 1점
        Strong      // 강함 - 2점
    }
    
    public bool IsCorrectController(bool isLeftCube, Collider hitController)
    {
        if (isLeftCube)
        {
            return hitController == LeftController;
        }
        else
        {
            return hitController == RightController;
        }
    }
    
    public void PlayDestroySound()
    {
        if (DestroyAudio != null && audioSource != null)
        {
            audioSource.PlayOneShot(DestroyAudio);
        }
    }

    public void PlayWrongSound()
    {
        if (WrongAudio != null && audioSource != null)
        {
            audioSource.PlayOneShot(WrongAudio);
        }
    }
    
    public void PlayWeakHitSound()
    {
        if (WeakHitAudio != null && audioSource != null)
        {
            audioSource.PlayOneShot(WeakHitAudio);
        }
    }
    
    public void PlayStrongHitSound()
    {
        if (StrongHitAudio != null && audioSource != null)
        {
            audioSource.PlayOneShot(StrongHitAudio);
        }
    }
}
