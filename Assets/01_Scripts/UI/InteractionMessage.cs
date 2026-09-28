using TMPro;
using UnityEngine;

public class InteractionMessage : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    [SerializeField] private Vector3 offset;
    [SerializeField] private float offsetY;

    private Camera mainCam;
    private Transform target;

#if UNITY_EDITOR
    private void Reset()
    {
        offsetY = 2f;
    }
#endif

    private void Awake()
    {
        messageText = GetComponentInChildren<TMP_Text>();
        offset = new Vector3(0f, offsetY, 0f);
        mainCam = Camera.main;

        HideText();
    }

    private void LateUpdate()
    {
        if (mainCam == null || target == null)
        {
            HideText();
            return;
        }

        transform.position = target.position + offset;

        // 카메라를 향하도록 회전
        transform.forward = mainCam.transform.forward;
    }

    public void ShowText(string text, Transform targetTransform)
    {
        target = targetTransform;

        messageText.text = $"[E] {text}";
        messageText.gameObject.SetActive(true);
    }

    public void HideText()
    {
        target = null;
        messageText.gameObject.SetActive(false);
    }
}
