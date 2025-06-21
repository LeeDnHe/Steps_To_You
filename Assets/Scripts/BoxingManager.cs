using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class BoxingManager : MonoBehaviour
{
    [Header("Cube Prefabs")]
    public GameObject LeftCube;
    public GameObject RightCube;
    public GameObject ForbiddenCube; // 치면 안 되는 오브젝트
    
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
    
    [Header("UI Elements")]
    public GameObject ComboUI; // 콤보 UI
    public TextMeshProUGUI ComboText; // 콤보 텍스트
    
    [Header("Audio")]
    public AudioClip DestroyAudio; // 큐브가 사라질 때 재생할 오디오
    public AudioClip WrongAudio; // 틀린 컨트롤러와 충돌할 때 재생할 오디오
    public AudioClip WeakHitAudio; // 약한 충격일 때 재생할 오디오
    public AudioClip ComboBreakAudio; // 콤보가 끊어질 때 재생할 오디오
    public AudioClip ForbiddenHitAudio; // 금지 오브젝트를 쳤을 때 재생할 오디오
    
    [Header("Game Status")]
    public int Score = 0;
    public int Combo = 0;
    public int SpawnedCount = 0;
    
    [Header("Game Manager Reference")]
    public BoxingInitializer boxingInitializer; // BoxingInitializer 참조 추가
    
    [Header("VFX Settings")]
    public GameObject leftCubeHitVFX; // 왼손 큐브 타격 시 VFX
    public GameObject rightCubeHitVFX; // 오른손 큐브 타격 시 VFX
    public GameObject forbiddenCubeHitVFX; // 금지 오브젝트 타격 시 VFX
    
    // 게임 페이즈 관련
    public GamePhase currentPhase = GamePhase.Easy;
    private float gameStartTime;
    private float phaseStartTime; // 현재 페이즈 시작 시간
    private bool isRestTime = false;
    private bool gameStarted = false;
    
    // 페이즈별 설정값
    private PhaseSettings easyPhase = new PhaseSettings(20f, 1.5f, 0.5f, 0.4f, 8f); // 20초, 1.5초 스폰, 0.5배속, 0.4배 범위
    private PhaseSettings normalPhase = new PhaseSettings(60f, 1.0f, 1.0f, 0.8f, 4f); // 60초, 1초 스폰, 1배속, 0.8배 범위
    private PhaseSettings hardPhase = new PhaseSettings(20f, 0.5f, 1.0f, 0.8f, 0f); // 20초, 0.5초 스폰, 1배속, 0.8배 범위
    
    private List<BoxingCube> activeCubes = new List<BoxingCube>();
    private AudioSource audioSource;
    
    // 실시간 속도 추적을 위한 변수들
    private Queue<Vector3> leftControllerPositions = new Queue<Vector3>();
    private Queue<Vector3> rightControllerPositions = new Queue<Vector3>();
    private Queue<float> positionTimes = new Queue<float>();
    private int maxPositionHistory = 10; // 최대 10개의 위치 기록
    
    // 게임 페이즈 열거형
    public enum GamePhase
    {
        Easy,
        Normal,
        Hard,
        Finished
    }
    
    // 페이즈 설정 클래스
    [System.Serializable]
    public class PhaseSettings
    {
        public float duration;
        public float spawnInterval;
        public float speedMultiplier;
        public float spawnRangeMultiplier;
        public float restTime;
        
        public PhaseSettings(float dur, float spawn, float speed, float range, float rest)
        {
            duration = dur;
            spawnInterval = spawn;
            speedMultiplier = speed;
            spawnRangeMultiplier = range;
            restTime = rest;
        }
    }
    
    void Start()
    {
        // AudioSource 컴포넌트 가져오기 또는 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
    }

        // 게임 초기화
        InitializeGame();
    }
    
    void InitializeGame()
    {
        gameStartTime = Time.time;
        currentPhase = GamePhase.Easy;
        gameStarted = false; // TTS에서 제어하도록 변경
        
        Debug.Log("=== Game Initialized - Waiting for TTS ===");
    }
    
    /// <summary>
    /// BoxingTTSManager에서 호출하는 게임 시작 메서드
    /// </summary>
    public void StartGame()
    {
        gameStarted = true;
        phaseStartTime = Time.time; // 페이즈 시작 시간 기록
        
        // 첫 번째 페이즈 시작
        StartCoroutine(GamePhaseManager());
        StartCoroutine(SpawnCubes());
        
        Debug.Log("=== Game Started - Easy Phase ===");
    }
    
    IEnumerator GamePhaseManager()
    {
        while (currentPhase != GamePhase.Finished)
        {
            PhaseSettings currentSettings = GetCurrentPhaseSettings();
            
            // 현재 페이즈 실행
            yield return new WaitForSeconds(currentSettings.duration);
            
            // 휴식 시간
            if (currentSettings.restTime > 0)
            {
                isRestTime = true;
                Debug.Log($"=== Rest Time: {currentSettings.restTime} seconds ===");
                yield return new WaitForSeconds(currentSettings.restTime);
                isRestTime = false;
            }
            
            // 다음 페이즈로 전환
            TransitionToNextPhase();
        }
        
        Debug.Log("=== Game Finished ===");
        
        // BoxingInitializer의 EndGame 호출
        if (boxingInitializer != null)
        {
            boxingInitializer.EndGame();
        }
    }
    
    void TransitionToNextPhase()
    {
        phaseStartTime = Time.time; // 새 페이즈 시작 시간 기록
        
        switch (currentPhase)
        {
            case GamePhase.Easy:
                currentPhase = GamePhase.Normal;
                Debug.Log("=== Transition to Normal Phase ===");
                break;
            case GamePhase.Normal:
                currentPhase = GamePhase.Hard;
                Debug.Log("=== Transition to Hard Phase ===");
                break;
            case GamePhase.Hard:
                currentPhase = GamePhase.Finished;
                Debug.Log("=== Game Completed ===");
                break;
        }
    }
    
    PhaseSettings GetCurrentPhaseSettings()
    {
        switch (currentPhase)
        {
            case GamePhase.Easy:
                return easyPhase;
            case GamePhase.Normal:
                return normalPhase;
            case GamePhase.Hard:
                return hardPhase;
            default:
                return normalPhase;
        }
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
        while (currentPhase != GamePhase.Finished && gameStarted)
        {
            // 휴식 시간 중에는 스폰하지 않음
            if (!isRestTime)
            {
                SpawnRandomCube();
                SpawnedCount++;
            }
            
            // 현재 페이즈에 따른 스폰 간격
            PhaseSettings currentSettings = GetCurrentPhaseSettings();
            yield return new WaitForSeconds(currentSettings.spawnInterval);
        }
    }
    
    void SpawnRandomCube()
    {
        // 1/20 확률로 금지 오브젝트 스폰
        bool isForbiddenCube = Random.Range(0, 20) == 0;
        GameObject cubeToSpawn;
        bool isLeftCube = false;
        
        if (isForbiddenCube && ForbiddenCube != null)
        {
            cubeToSpawn = ForbiddenCube;
        }
        else
        {
            // 랜덤하게 Left 또는 Right 큐브 선택
            isLeftCube = Random.Range(0, 2) == 0;
            cubeToSpawn = isLeftCube ? LeftCube : RightCube;
        }
        
        if (cubeToSpawn == null || SpawnPoint == null) return;
        
        // 현재 페이즈에 따른 스폰 범위
        PhaseSettings currentSettings = GetCurrentPhaseSettings();
        float spawnRange = 0.5f * currentSettings.spawnRangeMultiplier;
        
        // 스폰 포인트 기준 XY 범위에서 랜덤 위치
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnRange, spawnRange),
            Random.Range(-spawnRange, spawnRange),
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
        
        boxingCube.Initialize(this, isLeftCube, isForbiddenCube);
        activeCubes.Add(boxingCube);
    }
    
    void MoveCubes()
    {
        // 현재 페이즈에 따른 이동 속도
        PhaseSettings currentSettings = GetCurrentPhaseSettings();
        float currentSpeed = MoveSpeed * currentSettings.speedMultiplier;
        
        for (int i = activeCubes.Count - 1; i >= 0; i--)
        {
            if (activeCubes[i] != null)
            {
                // -Z 방향으로 이동 (플레이어 쪽으로)
                activeCubes[i].transform.Translate(Vector3.back * currentSpeed * Time.deltaTime);
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
        // 금지 오브젝트를 쳤을 때
        if (cube.IsForbiddenCube)
        {
            Debug.Log("Forbidden cube hit! Combo broken!");
            ResetCombo();
            PlayForbiddenHitSound();
            PlayForbiddenCubeHitVFX(cube.transform.position);
            activeCubes.Remove(cube);
            Destroy(cube.gameObject);
            return;
        }
        
        if (hitByCorrectController)
        {
            // 충격 강도 측정
            float hitVelocity = GetControllerVelocity(hitController);
            
            Debug.Log($"Hit velocity: {hitVelocity:F2} m/s");
            
            // 충격 강도에 따른 처리
            if (hitVelocity < MinHitVelocity)

            {
                Debug.Log("Hit too weak! Try harder!");
                ResetCombo();
                PlayWeakHitSound();
                // 큐브는 파괴되지 않음
            }
            else
            {
                // 콤보에 따른 점수 계산 방식 변경
                Combo++;
                
                // 콤보에 따른 점수 추가
                if (Combo >= 20)
                {
                    Score += 3;
                    Debug.Log($"Excellent hit! (+3 points) Score: {Score}, Combo: {Combo}");
                }
                else if (Combo >= 5)
                {
                    Score += 2;
                    Debug.Log($"Great hit! (+2 points) Score: {Score}, Combo: {Combo}");
                }
                else
                {
                    Score += 1;
                    Debug.Log($"Good hit! (+1 point) Score: {Score}, Combo: {Combo}");
                }
                
                PlayDestroySound();
                
                // 큐브 타입에 따른 VFX 재생
                if (cube.IsLeftCube)
                {
                    PlayLeftCubeHitVFX(cube.transform.position);
                }
                else
                {
                    PlayRightCubeHitVFX(cube.transform.position);
                }
                
                UpdateComboUI();
                activeCubes.Remove(cube);
                Destroy(cube.gameObject);
            }
        }
        else
        {
            Debug.Log("Hit by wrong controller! Combo broken!");
            ResetCombo();
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
    

    
    public void PlayComboBreakSound()
    {
        if (ComboBreakAudio != null && audioSource != null)
        {
            audioSource.PlayOneShot(ComboBreakAudio);
        }
    }
    
    public void PlayForbiddenHitSound()
    {
        if (ForbiddenHitAudio != null && audioSource != null)
        {
            audioSource.PlayOneShot(ForbiddenHitAudio);
        }
    }
    
    /// <summary>
    /// 왼손 큐브 타격 VFX 재생
    /// </summary>
    /// <param name="position">VFX가 재생될 위치</param>
    public void PlayLeftCubeHitVFX(Vector3 position)
    {
        PlayVFXAtPosition(leftCubeHitVFX, position, "Left cube hit VFX");
    }
    
    /// <summary>
    /// 오른손 큐브 타격 VFX 재생
    /// </summary>
    /// <param name="position">VFX가 재생될 위치</param>
    public void PlayRightCubeHitVFX(Vector3 position)
    {
        PlayVFXAtPosition(rightCubeHitVFX, position, "Right cube hit VFX");
    }
    
    /// <summary>
    /// 금지 오브젝트 타격 VFX 재생
    /// </summary>
    /// <param name="position">VFX가 재생될 위치</param>
    public void PlayForbiddenCubeHitVFX(Vector3 position)
    {
        PlayVFXAtPosition(forbiddenCubeHitVFX, position, "Forbidden cube hit VFX");
    }
    
    /// <summary>
    /// 지정된 위치에 VFX 재생 (공통 메서드)
    /// </summary>
    /// <param name="vfxPrefab">재생할 VFX 프리팹</param>
    /// <param name="position">VFX가 재생될 위치</param>
    /// <param name="logMessage">로그 메시지</param>
    private void PlayVFXAtPosition(GameObject vfxPrefab, Vector3 position, string logMessage)
    {
        if (vfxPrefab != null)
        {
            // VFX를 큐브 위치에 생성
            GameObject vfxInstance = Instantiate(vfxPrefab, position, Quaternion.identity);
            
            // VFX가 파티클 시스템이라면 자동으로 파괴되도록 설정
            ParticleSystem ps = vfxInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // 파티클 시스템의 duration + startLifetime 후에 자동 파괴
                float destroyTime = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(vfxInstance, destroyTime);
                Debug.Log($"{logMessage} played at position: {position}");
            }
            else
            {
                // 파티클 시스템이 아닌 경우 3초 후 파괴
                Destroy(vfxInstance, 3f);
                Debug.Log($"{logMessage} (non-particle) played at position: {position}");
            }
        }
        else
        {
            Debug.LogWarning($"{logMessage} prefab is not assigned!");
        }
    }
    
    // 콤보 관련 메서드들
    void UpdateComboUI()
    {
        if (ComboText != null)
        {
            ComboText.text = $"Combo: {Combo}";
        }
    }
    
    void ResetCombo()
    {
        if (Combo > 0)
        {
            Combo = 0;
            UpdateComboUI();
            PlayComboBreakSound();
            Debug.Log("Combo Reset!");
        }
    }
    
    // 큐브가 놓쳤을 때 호출되는 메서드
    public void OnCubeMissed(BoxingCube cube)
    {
        if (!cube.IsForbiddenCube) // 금지 오브젝트가 아닌 경우에만 콤보 초기화
        {
            Debug.Log("Cube missed! Combo broken!");
            ResetCombo();
        }
        
        activeCubes.Remove(cube);
        Destroy(cube.gameObject);
    }
    
    /// <summary>
    /// 현재 페이즈의 남은 시간을 반환
    /// </summary>
    /// <returns>남은 시간 (초)</returns>
    public float GetRemainingPhaseTime()
    {
        if (!gameStarted || currentPhase == GamePhase.Finished)
            return 0f;
            
        PhaseSettings currentSettings = GetCurrentPhaseSettings();
        float elapsedTime = Time.time - phaseStartTime;
        float remainingTime = currentSettings.duration - elapsedTime;
        
        return Mathf.Max(0f, remainingTime);
    }
}
