using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterHoverEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Transform previewRoot;
    [SerializeField, Min(1f)] private float hoverScale = 1.08f;

    private const float SmoothTime = 0.08f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Vector3 velocity;

    private void Awake()
    {
        if (previewRoot == null)
        {
            Debug.LogWarning("확대할 캐릭터 프리뷰를 연결해주세요.", this);
            enabled = false;
            return;
        }

        originalScale = previewRoot.localScale;
        targetScale = originalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    private void Update()
    {
        previewRoot.localScale = Vector3.SmoothDamp(
            previewRoot.localScale,
            targetScale,
            ref velocity,
            SmoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime);
    }

    private void OnDisable()
    {
        if (previewRoot == null)
            return;

        previewRoot.localScale = originalScale;
        targetScale = originalScale;
        velocity = Vector3.zero;
    }
}