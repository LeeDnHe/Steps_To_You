using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MoveNextChapter : MonoBehaviour
{
    [Tooltip("이동할 씬 이름")]
    public string targetSceneName;
    
    [Header("White Fade Settings")]
    [Tooltip("페이드 효과에 사용할 하얀 이미지 GameObject")]
    public GameObject whiteBackgroundObject;
    
    [Tooltip("페이드인 지속 시간 (초)")]
    public float fadeInDuration = 2f;
    
    private Button uiButton;
    private float originalAmbientIntensity; // 원래 감마값 저장용
    
    // Start is called before the first frame update
    void Start()
    {
        // 원래 감마값 저장
        originalAmbientIntensity = RenderSettings.ambientIntensity;
        
        // 하얀 배경 초기화 (항상 활성화, 알파값 0)
        if (whiteBackgroundObject != null)
        {
            whiteBackgroundObject.SetActive(true);
            
            // 하얀색 이미지 컴포넌트 찾아서 알파값 0으로 설정
            UnityEngine.UI.Image whiteImage = whiteBackgroundObject.GetComponent<UnityEngine.UI.Image>();
            if (whiteImage == null)
            {
                whiteImage = whiteBackgroundObject.GetComponentInChildren<UnityEngine.UI.Image>();
            }
            
            if (whiteImage != null)
            {
                Color color = whiteImage.color;
                color.a = 0f;
                whiteImage.color = color;
                Debug.Log("MoveNextChapter: White background initialized - always active with alpha 0");
            }
        }
        
        // UI 버튼 컴포넌트 검색 및 이벤트 연결
        uiButton = GetComponent<Button>();
        if (uiButton != null)
        {
            uiButton.onClick.AddListener(LoadTargetScene);
        }
        else
        {
            Debug.LogWarning("MoveNextChapter: Button 컴포넌트가 필요합니다!");
        }
    }
    
    /// <summary>
    /// 대상 씬을 로드합니다. (하얀 페이드 효과 포함)
    /// </summary>
    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("MoveNextChapter: 이동할 씬 이름이 설정되지 않았습니다!");
            return;
        }
        
        Debug.Log($"MoveNextChapter: 하얀 페이드 효과와 함께 씬 '{targetSceneName}'(으)로 이동합니다.");
        
        // 버튼 비활성화 (중복 클릭 방지)
        if (uiButton != null)
        {
            uiButton.interactable = false;
        }
        
        // 하얀 페이드 효과 시작
        StartCoroutine(FadeToWhiteAndLoadScene());
    }
    
    /// <summary>
    /// 하얀 페이드 효과 후 씬 로드 (알파값 조절 방식)
    /// </summary>
    IEnumerator FadeToWhiteAndLoadScene()
    {
        if (whiteBackgroundObject == null)
        {
            Debug.LogWarning("MoveNextChapter: 하얀 배경 오브젝트가 설정되지 않았습니다. 즉시 씬을 로드합니다.");
            SceneManager.LoadScene(targetSceneName);
            yield break;
        }
        
        // 하얀색 이미지 컴포넌트 찾기
        UnityEngine.UI.Image whiteImage = whiteBackgroundObject.GetComponent<UnityEngine.UI.Image>();
        
        if (whiteImage == null)
        {
            // 하위 오브젝트에서 Image 컴포넌트 찾기
            whiteImage = whiteBackgroundObject.GetComponentInChildren<UnityEngine.UI.Image>();
        }
        
        if (whiteImage == null)
        {
            Debug.LogError("MoveNextChapter: White background object has no Image component! Loading scene immediately.");
            SceneManager.LoadScene(targetSceneName);
            yield break;
        }
        
        Debug.Log("MoveNextChapter: 하얀 페이드 효과 시작 (알파값 조절)");
        
        // 알파값을 0에서 1로 서서히 증가
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            float t = elapsedTime / fadeInDuration;
            float smoothT = t * t * (3f - 2f * t); // 부드러운 곡선 보간
            float currentAlpha = Mathf.Lerp(0f, 1f, smoothT);
            
            Color color = whiteImage.color;
            color.a = currentAlpha;
            whiteImage.color = color;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 최종 알파값 설정 (완전히 하얀 화면)
        Color finalColor = whiteImage.color;
        finalColor.a = 1f;
        whiteImage.color = finalColor;
        
        Debug.Log($"MoveNextChapter: 하얀 페이드 효과 완료 - Alpha set to 1.0");
        
        // 씬 로드
        Debug.Log($"MoveNextChapter: 씬 '{targetSceneName}' 로드 시작");
        SceneManager.LoadScene(targetSceneName);
    }
}
