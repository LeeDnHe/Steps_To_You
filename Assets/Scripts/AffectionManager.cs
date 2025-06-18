using UnityEngine;

public class AffectionManager : MonoBehaviour
{
    [Header("Affection Settings")]
    public int currentAffection = 0;
    public int maxAffection = 100;
    
    [Header("Debug")]
    public bool debugMode = false;
    
    private static AffectionManager instance;
    
    public static AffectionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AffectionManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AffectionManager");
                    instance = go.AddComponent<AffectionManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 호감도 추가
    /// </summary>
    /// <param name="amount">추가할 호감도</param>
    public void AddAffection(int amount)
    {
        currentAffection = Mathf.Clamp(currentAffection + amount, 0, maxAffection);
        
        if (debugMode)
        {
            Debug.Log($"호감도 추가: +{amount}, 현재 호감도: {currentAffection}");
        }
    }
    
    /// <summary>
    /// 호감도 감소
    /// </summary>
    /// <param name="amount">감소할 호감도</param>
    public void SubtractAffection(int amount)
    {
        currentAffection = Mathf.Clamp(currentAffection - amount, 0, maxAffection);
        
        if (debugMode)
        {
            Debug.Log($"호감도 감소: -{amount}, 현재 호감도: {currentAffection}");
        }
    }
    
    /// <summary>
    /// 현재 호감도 반환
    /// </summary>
    /// <returns>현재 호감도</returns>
    public int GetCurrentAffection()
    {
        return currentAffection;
    }
    
    /// <summary>
    /// 호감도 직접 설정
    /// </summary>
    /// <param name="amount">설정할 호감도</param>
    public void SetAffection(int amount)
    {
        currentAffection = Mathf.Clamp(amount, 0, maxAffection);
        
        if (debugMode)
        {
            Debug.Log($"호감도 설정: {currentAffection}");
        }
    }
    
    /// <summary>
    /// 호감도 리셋
    /// </summary>
    public void ResetAffection()
    {
        currentAffection = 0;
        
        if (debugMode)
        {
            Debug.Log("호감도 리셋");
        }
    }
} 