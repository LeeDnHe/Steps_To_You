using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MoveNextChapter : MonoBehaviour
{
    [Tooltip("이동할 씬 이름")]
    public string targetSceneName;
    
    private Button uiButton;
    
    // Start is called before the first frame update
    void Start()
    {
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
    /// 대상 씬을 로드합니다.
    /// </summary>
    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("MoveNextChapter: 이동할 씬 이름이 설정되지 않았습니다!");
            return;
        }
        
        Debug.Log($"MoveNextChapter: 씬 '{targetSceneName}'(으)로 이동합니다.");
        SceneManager.LoadScene(targetSceneName);
    }
}
