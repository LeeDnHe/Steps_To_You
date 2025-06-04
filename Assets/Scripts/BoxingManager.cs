using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    [Header("Audio")]
    public AudioClip DestroyAudio; // 큐브가 사라질 때 재생할 오디오
    public AudioClip WrongAudio; // 틀린 컨트롤러와 충돌할 때 재생할 오디오
    
    [Header("Game Status")]
    public int Score = 0;
    public int SpawnedCount = 0;
    
    private List<BoxingCube> activeCubes = new List<BoxingCube>();
    private AudioSource audioSource;
    
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
    }
    
    IEnumerator SpawnCubes()
    {
        while (SpawnedCount < TotalCubes)
        {
            SpawnRandomCube();
            SpawnedCount++;
            
            yield return new WaitForSeconds(SpawnInterval);
        }
        
        Debug.Log("All cubes spawned!");
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
        
        Debug.Log($"Spawned {(isLeftCube ? "Left" : "Right")} cube at {spawnPosition}");
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
    
    public void OnCubeHit(BoxingCube cube, bool hitByCorrectController)
    {
        if (hitByCorrectController)
        {
            Score++;
            Debug.Log($"Score: {Score}");
            PlayDestroySound();
            activeCubes.Remove(cube);
            Destroy(cube.gameObject);
        }
        else
        {
            Debug.Log("Hit by wrong controller!");
            PlayWrongSound();
        }
        
        // 큐브 제거
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
}
