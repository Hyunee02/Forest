using TMPro;
using UnityEngine;

public class Message : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    private Camera mainCam;

#if UNITY_EDITOR
    private void Reset()
    {
        messageText = GetComponentInChildren<TMP_Text>();
    }
#endif

    private void Awake()
    {
        mainCam = Camera.main;

        HideText();
    }

    private void LateUpdate()
    {
        if (mainCam == null)
            return;

        transform.forward = mainCam.transform.forward;
    }

    public void ShowText(string text)
    {
        messageText.text = $"[E] {text}";
        messageText.gameObject.SetActive(true);
    }

    public void HideText()
    {
        messageText.gameObject.SetActive(false);
    }
}
