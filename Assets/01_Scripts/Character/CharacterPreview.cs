using UnityEngine;

public class CharacterPreview : MonoBehaviour
{
    [SerializeField] private Transform previewRoot;

    private CharacterView currentView;

    public void Show(CharacterData data)
    {
        Clear();

        if (data == null || data.CharacterPrefab == null)
            return;

        Transform parent;

        if (previewRoot != null)
            parent = previewRoot;

        else
            parent = transform;

        currentView = Instantiate(data.CharacterPrefab, parent);

        currentView.transform.localPosition = Vector3.zero;
        currentView.transform.localRotation = Quaternion.identity;

        // 미리보기에서는 모델 이동 X
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
