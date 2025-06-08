using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasicRoomUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject WallpaperPanel;
    public GameObject ContactPanel;
    public GameObject AlbumPanel;
    
    [Header("Wallpaper Buttons")]
    public Button ContactButton;  // Wallpaper 패널에 있는 Contact 버튼
    public Button AlbumButton;    // Wallpaper 패널에 있는 Album 버튼
    
    [Header("Back Buttons")]
    public Button ContactBackButton;  // Contact 패널에 있는 Back 버튼
    public Button AlbumBackButton;    // Album 패널에 있는 Back 버튼

    void Start()
    {
        // 초기 상태 설정: Wallpaper만 활성화
        ShowWallpaperPanel();
        
        // 버튼 이벤트 연결
        SetupButtonEvents();
    }

    void SetupButtonEvents()
    {
        // Wallpaper 패널의 버튼들
        if (ContactButton != null)
        {
            ContactButton.onClick.AddListener(ShowContactPanel);
        }
        
        if (AlbumButton != null)
        {
            AlbumButton.onClick.AddListener(ShowAlbumPanel);
        }
        
        // Back 버튼들
        if (ContactBackButton != null)
        {
            ContactBackButton.onClick.AddListener(ShowWallpaperPanel);
        }
        
        if (AlbumBackButton != null)
        {
            AlbumBackButton.onClick.AddListener(ShowWallpaperPanel);
        }
    }
    
    public void ShowWallpaperPanel()
    {
        SetPanelState(true, false, false);
    }
    
    public void ShowContactPanel()
    {
        SetPanelState(false, true, false);
    }
    
    public void ShowAlbumPanel()
    {
        SetPanelState(false, false, true);
    }
    
    private void SetPanelState(bool wallpaperActive, bool contactActive, bool albumActive)
    {
        if (WallpaperPanel != null) WallpaperPanel.SetActive(wallpaperActive);
        if (ContactPanel != null) ContactPanel.SetActive(contactActive);
        if (AlbumPanel != null) AlbumPanel.SetActive(albumActive);
    }
    
    // 외부에서 현재 활성화된 패널 확인용
    public string GetCurrentActivePanel()
    {
        if (WallpaperPanel != null && WallpaperPanel.activeSelf) return "Wallpaper";
        if (ContactPanel != null && ContactPanel.activeSelf) return "Contact";
        if (AlbumPanel != null && AlbumPanel.activeSelf) return "Album";
        return "None";
    }
}
