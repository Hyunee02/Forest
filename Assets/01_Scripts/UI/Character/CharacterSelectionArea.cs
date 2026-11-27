using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CharacterSelectArea : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("마우스를 올렸을 때 영역 색상")]
    [SerializeField]
    private Color hoverColor =
        new Color(1f, 0.88f, 0.55f, 0.16f);

    private Image areaImage;
    private CharacterSelectUI owner;
    private int characterIndex;

    private void Awake()
    {
        areaImage = GetComponent<Image>();
        areaImage.raycastTarget = true;
        areaImage.color = Color.clear;
    }

    public void Init(CharacterSelectUI selectUI, int index)
    {
        owner = selectUI;
        characterIndex = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        areaImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        areaImage.color = Color.clear;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (owner != null)
            owner.SelectCharacter(characterIndex);
    }

    private void OnDisable()
    {
        if (areaImage != null)
            areaImage.color = Color.clear;
    }
}