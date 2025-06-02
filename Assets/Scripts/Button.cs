using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Button : MonoBehaviour
{
    public Color PressedColor;
    public Color ReleasedColor;

    public float LerpSpeed = 0.5f;

    public AudioClip PressSound;
    public AudioClip ReleaseSound;
    public bool IsPressed { get; private set; } = false;

    private MeshRenderer meshRenderer;
    private AudioSource audioSource;
    private Animator animator;
    private XRSimpleInteractable xrInteractable;
    
    // 현재 상호작용 중인 Poke Interactor들을 추적
    private HashSet<XRPokeInteractor> activePokeInteractors = new HashSet<XRPokeInteractor>();

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        
        // XR Simple Interactable 컴포넌트 가져오기 또는 추가
        xrInteractable = GetComponent<XRSimpleInteractable>();
        if (xrInteractable == null)
        {
            xrInteractable = gameObject.AddComponent<XRSimpleInteractable>();
        }
        
        // XR Simple Interactable 이벤트 설정 - Hover 이벤트 사용
        xrInteractable.hoverEntered.AddListener(OnHoverEntered);
        xrInteractable.hoverExited.AddListener(OnHoverExited);
    }
    
    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        // Poke Interactor인지 확인
        if (args.interactorObject is XRPokeInteractor pokeInteractor)
        {
            activePokeInteractors.Add(pokeInteractor);
            Debug.Log($"Poke Interactor added. Active count: {activePokeInteractors.Count}");
            
            // 양손 감지 시 모든 상태 변경 (count >= 2가 양손 감지된 상태)
            if (activePokeInteractors.Count >= 2 && !IsPressed)
            {
                IsPressed = true;
                animator.SetBool("IsPressed", true);
                
                // Press 사운드 재생
                if (PressSound != null) audioSource.PlayOneShot(PressSound);
                
                Debug.Log("Button PRESSED - Both hands detected!");
            }
        }
        // Ray Interactor는 무시
        else if (args.interactorObject is XRRayInteractor)
        {
            Debug.Log("Ray Interactor ignored - Dual Poke only!");
        }
    }
    
    private void OnHoverExited(HoverExitEventArgs args)
    {
        // Poke Interactor인지 확인
        if (args.interactorObject is XRPokeInteractor pokeInteractor)
        {
            activePokeInteractors.Remove(pokeInteractor);
            Debug.Log($"Poke Interactor removed. Active count: {activePokeInteractors.Count}");
            
            // 양손 중 하나라도 떼면 모든 상태 해제
            if (activePokeInteractors.Count < 2 && IsPressed)
            {
                IsPressed = false;
                animator.SetBool("IsPressed", false);
                
                // Release 사운드 재생
                if (ReleaseSound != null) audioSource.PlayOneShot(ReleaseSound);
                
                Debug.Log("Button RELEASED - Not enough hands");
            }
        }
    }

    private void Update()
    {
        // 양손이 모두 감지되었을 때만 색깔 변경 (count >= 2가 양손 감지된 상태)
        bool shouldShowPressed = activePokeInteractors.Count >= 2;
        meshRenderer.material.color = Color.Lerp(meshRenderer.material.color, shouldShowPressed ? PressedColor : ReleasedColor, LerpSpeed * Time.deltaTime);
    }

    // 외부에서 버튼 상태를 설정할 수 있는 메서드 (필요시)
    public void SetPressed(bool isPressed)
    {
        IsPressed = isPressed;
        animator.SetBool("IsPressed", isPressed);
    }
    
    // 디버그용: 현재 활성화된 Poke Interactor 수 확인
    public int GetActivePokeCount()
    {
        return activePokeInteractors.Count;
    }
}
