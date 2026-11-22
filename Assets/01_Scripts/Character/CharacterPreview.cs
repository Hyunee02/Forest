using UnityEngine;

public class CharacterPreview : MonoBehaviour
{
    [SerializeField] private Transform previewRoot;

    private CharacterView currentView;

    public void Show(CharacterData data)
    {
        Clear();

        if (data == null || data.PreviewPrefab == null)
            return;

        Transform parent = previewRoot != null ? previewRoot : transform;

        currentView = Instantiate(data.PreviewPrefab, parent);
        currentView.transform.localPosition = Vector3.zero;
        currentView.transform.localRotation = Quaternion.identity;

        if (currentView.Animator != null)
            currentView.Animator.applyRootMotion = false;
    }

    public void Clear()
    {
        if (currentView == null)
            return;

        currentView.gameObject.SetActive(false);
        Destroy(currentView.gameObject);
        currentView = null;
    }
}