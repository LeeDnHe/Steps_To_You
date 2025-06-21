using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariousAudioController : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip messagePopupOpenClip;  // 문자 팝업창 열림
    public AudioClip messagePopupCloseClip; // 문자 팝업창 닫힘
    public AudioClip footstepClip;          // 발걸음
    public AudioClip doorOpenClip;          // 문 열림
    public AudioClip doorCloseClip;         // 문 닫힘
    public AudioClip choiceAppearClip;      // 선택지 등장
    public AudioClip choiceClickClip;       // 선택지 클릭
    public AudioClip resultPanelOpenClip;   // 결과창 열림
    public AudioClip resultPanelCloseClip;  // 결과창 닫힘
    
    private AudioSource audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource가 없습니다!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 1. 문자 팝업창 열림 (특별한 시퀀스 포함)
    public void PlayMessagePopupOpenSequence()
    {
        StartCoroutine(MessagePopupSequence());
    }
    
    private IEnumerator MessagePopupSequence()
    {
        // 팝업창 열림 오디오 재생
        PlayClip(messagePopupOpenClip);
        
        // 1초 대기 후 발걸음 오디오
        yield return new WaitForSeconds(1f);
        PlayClip(footstepClip);
        
        // 총 3초 대기 후 팝업창 닫힘 오디오 (1초는 이미 지났으므로 2초 더 대기)
        yield return new WaitForSeconds(2f);
        PlayClip(messagePopupCloseClip);
    }
    
    // 2. 문자 팝업창 닫힘 (단독 호출용)
    public void PlayMessagePopupClose()
    {
        PlayClip(messagePopupCloseClip);
    }
    
    // 3. 발걸음 (단독 호출용)
    public void PlayFootstep()
    {
        PlayClip(footstepClip);
    }
    
    // 4. 문 열림
    public void PlayDoorOpen()
    {
        PlayClip(doorOpenClip);
    }
    
    // 5. 문 닫힘
    public void PlayDoorClose()
    {
        PlayClip(doorCloseClip);
    }
    
    // 6. 선택지 등장
    public void PlayChoiceAppear()
    {
        PlayClip(choiceAppearClip);
    }
    
    // 7. 선택지 클릭
    public void PlayChoiceClick()
    {
        PlayClip(choiceClickClip);
    }
    
    // 8. 결과창 열림
    public void PlayResultPanelOpen()
    {
        PlayClip(resultPanelOpenClip);
    }
    
    // 9. 결과창 닫힘
    public void PlayResultPanelClose()
    {
        PlayClip(resultPanelCloseClip);
    }
    
    // 오디오클립 재생 헬퍼 메서드
    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            if (audioSource == null) Debug.LogWarning("AudioSource가 null입니다.");
            if (clip == null) Debug.LogWarning("재생할 AudioClip이 null입니다.");
        }
    }
}
