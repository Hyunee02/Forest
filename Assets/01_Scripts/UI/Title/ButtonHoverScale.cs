using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonHoverScale : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("호버 효과")]
    [SerializeField, Min(1f)] private float hoverScale = 1.06f;
    [SerializeField, Min(0f)] private float duration = 0.12f;

    private Button button;
    private Vector3 originalScale;
    private Vector3 startScale;
    private Vector3 targetScale;

    private float elapsed;
    private bool isAnimating;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.IsInteractable())
            AnimateTo(originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateTo(originalScale);
    }

    private void AnimateTo(Vector3 scale)
    {
        // 빠르게 들어갔다 나와도 현재 보이는 크기에서 이어서 움직인다.
        startScale = transform.localScale;
        targetScale = scale;
        elapsed = 0f;

        if (duration <= 0f || startScale == targetScale)
        {
            transform.localScale = targetScale;
            isAnimating = false;
            return;
        }

        isAnimating = true;
    }

    private void Update()
    {
        if (!isAnimating)
            return;

        elapsed += Time.unscaledDeltaTime;

        float progress = Mathf.Clamp01(elapsed / duration);
        float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

        transform.localScale = Vector3.Lerp(startScale, targetScale, easedProgress);

        if (progress >= 1f)
            isAnimating = false;
    }

    private void OnDisable()
    {
        // 타이틀을 다시 열었을 때 버튼이 커진 채로 남는 것을 방지한다.
        isAnimating = false;
        transform.localScale = originalScale;
    }
}