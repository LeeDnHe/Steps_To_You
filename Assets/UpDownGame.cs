using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDownGame : MonoBehaviour
{
    [Header("Button References")]
    public Button UpButton;
    public Button DownButton;
    
    [Header("Game Settings")]
    public int Score = 0;
    
    [Header("Audio")]
    public AudioClip WrongButtonSound; // 잘못 눌렀을 때 재생할 오디오
    
    [Header("Debug Info")]
    public bool IsUpButtonNext = true; // true면 위쪽 버튼 차례, false면 아래쪽 버튼 차례
    
    // 이전 프레임의 버튼 상태 저장용
    private bool previousUpButtonState;
    private bool previousDownButtonState;
    
    // AudioSource 컴포넌트
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        // 시작할 때는 위쪽 버튼부터 누르도록 설정
        IsUpButtonNext = true;
        Score = 0;
        
        // AudioSource 컴포넌트 가져오기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // AudioSource가 없으면 추가
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 초기 버튼 상태 저장
        if (UpButton != null) previousUpButtonState = UpButton.IsPressed;
        if (DownButton != null) previousDownButtonState = DownButton.IsPressed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckButtonInput();
    }
    
    void CheckButtonInput()
    {
        if (UpButton == null || DownButton == null) return;
        
        // 위쪽 버튼이 눌렸는지 확인 (이전에 안눌려있다가 지금 눌린 상태)
        bool upButtonJustPressed = !previousUpButtonState && UpButton.IsPressed;
        // 아래쪽 버튼이 눌렸는지 확인
        bool downButtonJustPressed = !previousDownButtonState && DownButton.IsPressed;
        
        // 올바른 순서로 버튼이 눌렸는지 판정
        if (upButtonJustPressed && IsUpButtonNext)
        {
            // 위쪽 버튼을 눌러야 할 차례에 위쪽 버튼을 눌렀음
            AddScore();
            IsUpButtonNext = false; // 다음은 아래쪽 버튼 차례
            Debug.Log($"Up Button Pressed! Score: {Score}, Next: Down Button");
        }
        else if (downButtonJustPressed && !IsUpButtonNext)
        {
            // 아래쪽 버튼을 눌러야 할 차례에 아래쪽 버튼을 눌렀음
            AddScore();
            IsUpButtonNext = true; // 다음은 위쪽 버튼 차례
            Debug.Log($"Down Button Pressed! Score: {Score}, Next: Up Button");
        }
        else if (upButtonJustPressed && !IsUpButtonNext)
        {
            // 잘못된 순서: 아래쪽 버튼을 눌러야 하는데 위쪽 버튼을 눌렀음
            Debug.Log("Wrong! Should press Down Button");
            PlayWrongSound();
        }
        else if (downButtonJustPressed && IsUpButtonNext)
        {
            // 잘못된 순서: 위쪽 버튼을 눌러야 하는데 아래쪽 버튼을 눌렀음
            Debug.Log("Wrong! Should press Up Button");
            PlayWrongSound();
        }
        
        // 현재 프레임의 상태를 다음 프레임을 위해 저장
        previousUpButtonState = UpButton.IsPressed;
        previousDownButtonState = DownButton.IsPressed;
    }
    
    void AddScore()
    {
        Score++;
    }
    
    void PlayWrongSound()
    {
        // 잘못 눌렀을 때 오디오 재생
        if (WrongButtonSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(WrongButtonSound);
        }
    }
    
    void ResetGame()
    {
        Score = 0;
        IsUpButtonNext = true; // 다시 위쪽 버튼부터 시작
        Debug.Log("Game Reset! Starting with Up Button");
    }
    
    // 외부에서 게임을 리셋할 수 있는 public 메서드
    public void ResetGamePublic()
    {
        ResetGame();
    }
    
    // 현재 점수를 가져오는 메서드
    public int GetScore()
    {
        return Score;
    }
    
    // 다음에 눌러야 할 버튼 정보를 가져오는 메서드
    public string GetNextButtonInfo()
    {
        return IsUpButtonNext ? "Up Button" : "Down Button";
    }
}
