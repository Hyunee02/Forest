using UnityEngine;
using UnityEngine.InputSystem;

public class PlacementManager : MonoBehaviour
{
    [Header("<< 참조 >>")]
    [SerializeField] private MapManager mapManager;
    [SerializeField] private Camera mainCamera;

    [Header("<< 그리드 프리뷰 >>")]
    [SerializeField] private GameObject canPlacePrefab;
    [SerializeField] private GameObject cantPlacePrefab;

    private MapObject selectedObject;

    private bool isPlacementSelectMode;
    private bool isMovingObject;
    private bool canPlace;


    private Vector2Int currentTilePos;  // 현재 마우스가 가리키는 타일 위치
    private Vector2Int originalTilePos;  // 이동 전 원래 타일 위치

    private GameObject canPreviewParent;
    private GameObject cantPreviewParent;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        HandleMoveModeToggle();

        if (isMovingObject)
        {
            HandleObjectMoving();
        }
        else if (isPlacementSelectMode)
        {
            HandleObjectSelect();
        }
    }

    private void HandleMoveModeToggle()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            isPlacementSelectMode = !isPlacementSelectMode;

            if (!isPlacementSelectMode)
            {
                Debug.Log("이동 선택 모드 OFF");
            }
            else
            {
                Debug.Log("이동 선택 모드 ON");
            }
        }
    }

    private void HandleObjectSelect()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            MapObject mapObject = hit.collider.GetComponentInParent<MapObject>();

            if (mapObject == null)
                return;
            if (!mapObject.Movable)
                return;

            EnterMoveMode(mapObject);
        }
    }

    private void EnterMoveMode(MapObject mapObject)
    {
        selectedObject = mapObject;
        isMovingObject = true;

        originalTilePos = mapManager.WorldToTile(selectedObject.transform.position);

        mapManager.SetObjectTilesOccupied(selectedObject, false);

        CreatePreviewObjects();

        Debug.Log($"{selectedObject.name}의 위치를 이동합니다.");
    }

    private void HandleObjectMoving()
    {
        UpdatePreviewPosition();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ConfirmMove();
        }

        // 취소
        if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelMove();
        }
    }

    private void UpdatePreviewPosition()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector2Int mouseTilePos = mapManager.WorldToTile(hit.point);
            currentTilePos = GetOriginTileFromCenter(mouseTilePos);

            Vector3 worldPos = mapManager.TileToWorld(currentTilePos);
            selectedObject.transform.position = worldPos;

            canPlace = mapManager.CanPlaceObject(selectedObject, currentTilePos);

            UpdatePreviewObjects(currentTilePos, canPlace);
        }
    }

    private void UpdatePreviewObjects(Vector2Int origin, bool placeable)
    {
        if (canPreviewParent == null || cantPreviewParent == null)
            return;

        canPreviewParent.SetActive(placeable);
        cantPreviewParent.SetActive(!placeable);

        GameObject activeParent = placeable ? canPreviewParent : cantPreviewParent;

        int index = 0;

        for (int x = 0; x < selectedObject.Width; x++)
        {
            for (int z = 0; z < selectedObject.Height; z++)
            {
                Vector2Int tilePos = new Vector2Int(origin.x + x, origin.y + z);
                Vector3 worldPos = mapManager.TileToWorld(tilePos);

                Transform cell = activeParent.transform.GetChild(index);
                cell.position = worldPos + new Vector3(0f, 0.05f, 0f);

                index++;
            }
        }
    }

    private Vector2Int GetOriginTileFromCenter(Vector2Int centerTilePos)
    {
        int offsetX = selectedObject.Width / 2;
        int offsetZ = selectedObject.Height / 2;

        return new Vector2Int(
            centerTilePos.x - offsetX,
            centerTilePos.y - offsetZ
        );
    }

    private void ConfirmMove()
    {
        if (!canPlace)
        {
            Debug.Log("*** 여기에는 배치할 수 없습니다. ***");
            return;
        }

        selectedObject.transform.position = mapManager.TileToWorld(currentTilePos);
        mapManager.SetObjectTilesOccupied(selectedObject, true);

        ExitMoveMode();

        Debug.Log($"{selectedObject}의 위치를 옮겼습니다.");
    }

    private void CancelMove()
    {
        selectedObject.transform.position = mapManager.TileToWorld(originalTilePos);
        mapManager.SetObjectTilesOccupied(selectedObject, true);

        ExitMoveMode();

        Debug.Log("이동 취소");
    }

    private void ExitMoveMode()
    {
        isMovingObject = false;
        selectedObject = null;
        canPlace = false;

        if (canPreviewParent != null)
        {
            Destroy(canPreviewParent);
            canPreviewParent = null;
        }

        if (cantPreviewParent != null)
        {
            Destroy(cantPreviewParent);
            cantPreviewParent = null;
        }
    }

    private void CreatePreviewObjects()
    {
        canPreviewParent = new GameObject("Can Placement Preview Cells");
        cantPreviewParent = new GameObject("Cant Placement Preview Cells");

        for (int x = 0; x < selectedObject.Width; x++)
        {
            for (int z = 0; z < selectedObject.Height; z++)
            {
                GameObject canCell = Instantiate(canPlacePrefab, canPreviewParent.transform);
                GameObject cantCell = Instantiate(cantPlacePrefab, cantPreviewParent.transform);

                canCell.name = $"CanPreviewCell_{x}_{z}";
                cantCell.name = $"CantPreviewCell_{x}_{z}";
            }
        }

        canPreviewParent.SetActive(false);
        cantPreviewParent.SetActive(false);
    }
}
