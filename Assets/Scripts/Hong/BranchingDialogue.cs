using UnityEngine;
using TMPro;
using System.Collections;

public class BranchingDialogue : MonoBehaviour
{
    /*public GameObject affectionTextObj;
    public TextMeshProUGUI affectionText;*/
    public GameObject currentPanel;

    public int affection = 0;

    [Header("Popup Images")]
    public GameObject popup_0;
    public GameObject popup_5;
    public GameObject popup_10;
    public GameObject popup_15;
    public GameObject popup_20;
    
    [Header("Score Settings")]
    public int currentScore = 0; // 현재 점수 저장
    public bool debugMode = false; // 디버그 모드
    
    private GameObject lastPopup; // 마지막에 띄운 오브젝트 저장용
    private Vector3 originalPosition; // 팝업의 원래 위치 저장용
    private CanvasGroup currentCanvasGroup; // 현재 팝업의 CanvasGroup
    
    /// <summary>
    /// 점수에 따른 팝업 표시 및 점수 저장
    /// </summary>
    /// <param name="score">저장할 점수</param>
    public void ShowPopupAndSaveScore(int score)
    {
        // 점수 저장
        currentScore = score;
        
        if (debugMode)
        {
            Debug.Log($"BranchingDialogue: Score saved = {currentScore}");
        }
        
        // 점수에 따른 팝업 표시
        switch (score)
        {
            case 0: ShowPopupImage(popup_0); break;
            case 5: ShowPopupImage(popup_5); break;
            case 10: ShowPopupImage(popup_10); break;
            case 15: ShowPopupImage(popup_15); break;
            case 20: ShowPopupImage(popup_20); break;
            default:
                if (debugMode)
                    Debug.LogWarning($"No popup defined for score: {score}");
                break;
        }
    }
    
    /// <summary>
    /// 팝업 이미지 표시
    /// </summary>
    /// <param name="popupObj">표시할 팝업 오브젝트</param>
    void ShowPopupImage(GameObject popupObj)
    {
        if (popupObj == null)
        {
            if (debugMode)
                Debug.LogWarning("Popup object is null!");
            return;
        }
        
        // CanvasGroup 컴포넌트 확인 및 추가
        currentCanvasGroup = popupObj.GetComponent<CanvasGroup>();
        if (currentCanvasGroup == null)
        {
            currentCanvasGroup = popupObj.AddComponent<CanvasGroup>();
        }
        
        // 원래 위치 저장
        originalPosition = popupObj.transform.position;
        
        // 초기 상태 설정 (투명, 원래 위치)
        currentCanvasGroup.alpha = 0f;
        popupObj.SetActive(true);
        lastPopup = popupObj;
        
        // 애니메이션 시작
        StartCoroutine(PopupAnimation());
        
        if (debugMode)
        {
            Debug.Log($"Popup animation started: {popupObj.name}");
        }
    }
    
    /// <summary>
    /// 팝업 애니메이션 (페이드 인/아웃 + 위로 이동)
    /// </summary>
    /// <returns></returns>
    IEnumerator PopupAnimation()
    {
        if (lastPopup == null || currentCanvasGroup == null) yield break;
        
        float totalDuration = 1.5f;
        float fadeInDuration = 0.5f;
        float displayDuration = 0.5f;
        float fadeOutDuration = 0.5f;
        float moveDistance = 0.5f;
        
        Vector3 startPosition = originalPosition;
        Vector3 endPosition = originalPosition + Vector3.up * moveDistance;
        
        // 1단계: 페이드 인 (0.5초)
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            float t = elapsedTime / fadeInDuration;
            currentCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            lastPopup.transform.position = Vector3.Lerp(startPosition, 
                Vector3.Lerp(startPosition, endPosition, 0.3f), t); // 전체 이동의 30%
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        currentCanvasGroup.alpha = 1f;
        
        // 2단계: 표시 유지 + 계속 이동 (0.5초)
        elapsedTime = 0f;
        Vector3 midPosition = Vector3.Lerp(startPosition, endPosition, 0.3f);
        Vector3 almostEndPosition = Vector3.Lerp(startPosition, endPosition, 0.8f);
        
        while (elapsedTime < displayDuration)
        {
            float t = elapsedTime / displayDuration;
            lastPopup.transform.position = Vector3.Lerp(midPosition, almostEndPosition, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 3단계: 페이드 아웃 + 마지막 이동 (0.5초)
        elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            float t = elapsedTime / fadeOutDuration;
            currentCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            lastPopup.transform.position = Vector3.Lerp(almostEndPosition, endPosition, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 최종 상태
        currentCanvasGroup.alpha = 0f;
        lastPopup.transform.position = originalPosition; // 원래 위치로 복원
        lastPopup.SetActive(false);
        
        if (debugMode)
        {
            Debug.Log($"Popup animation completed: {lastPopup.name}");
        }
        
        lastPopup = null;
        currentCanvasGroup = null;
    }
    
    /// <summary>
    /// 현재 저장된 점수 반환
    /// </summary>
    /// <returns>현재 점수</returns>
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    /// <summary>
    /// 점수 초기화
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        
        if (debugMode)
        {
            Debug.Log("BranchingDialogue: Score reset to 0");
        }
    }
}
