using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] Canvas canvas;

    PlayerBindInput input;

    private bool bOpen;


    private void Awake()
    {
        input = GetComponent<PlayerBindInput>();
    }

    private void OnEnable()
    {
        input.OnInventoryInput += ToggleInventory;
    }

    private void OnDisable()
    {
        input.OnInventoryInput -= ToggleInventory;
    }

    private void ToggleInventory()
    {
        // bOpen 값을 반대로 변환
        bOpen = !bOpen;

        // bOpen 값에 맞춰 인벤토리 온/오프
        if (canvas != null)
            canvas.gameObject.SetActive(bOpen);

    }
}
