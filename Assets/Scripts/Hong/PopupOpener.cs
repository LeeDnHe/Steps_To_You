using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class PopupOpener : MonoBehaviour
{
    public GameObject popupPanel;
    public Transform leftControllerTransform;
    public Button vibrateButton; // 진동을 실행할 버튼
    
    private bool isGrabbed = false;
    private XRGrabInteractable grabInteractable;
    private AudioSource audioSource;

    void Start()
    {
        // AudioSource 컴포넌트 가져오기 및 기본 재생
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = true; // 반복 재생
            audioSource.Play();
        }
        
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelected);
            grabInteractable.selectExited.AddListener(OnDeselected);
        }
        
        // 버튼 클릭 이벤트에 진동 메서드 연결
        if (vibrateButton != null)
        {
            vibrateButton.onClick.AddListener(TriggerVibration);
        }
    }

    void Update()
    {
        // 그랩 중일 때 왼쪽 컨트롤러를 따라다니기
        if (isGrabbed && leftControllerTransform != null)
        {
            transform.position = leftControllerTransform.position;
            transform.rotation = leftControllerTransform.rotation;
        }
    }

    void OnSelected(SelectEnterEventArgs args)
    {
        popupPanel.SetActive(true);
        isGrabbed = true;
        
        // 오디오 끄기
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        
        // XR Grab Interactable의 기본 추적 비활성화
        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = false;
            grabInteractable.trackRotation = false;
        }
    }
    
    void OnDeselected(SelectExitEventArgs args)
    {
        isGrabbed = false;
        
        // XR Grab Interactable의 추적 다시 활성화
        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
        }
    }
    
    // 버튼이 눌렸을 때 진동 실행
    public void TriggerVibration()
    {
        // 왼쪽 컨트롤러 진동
        if (leftControllerTransform != null)
        {
            var controller = leftControllerTransform.GetComponent<XRBaseController>();
            if (controller != null)
            {
                controller.SendHapticImpulse(0.5f, 0.2f); // 강도 0.5, 시간 0.2초
            }
        }
    }
}
