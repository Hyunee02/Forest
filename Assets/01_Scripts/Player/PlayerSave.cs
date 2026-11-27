using UnityEngine;

[RequireComponent(typeof(PlayerInventory), typeof(PlayerShop))]
public class PlayerSave : MonoBehaviour
{
    private PlayerInventory inventory;
    private PlayerShop shop;
    private CharacterCreateDraft profile;

    private bool ready;
    private bool dirty;
    private float nextSaveTime;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        shop = GetComponent<PlayerShop>();
    }

    private void OnEnable()
    {
        inventory.OnChanged += MarkDirty;
        shop.TradeCompleted += SaveAfterTrade;
    }

    public bool TryPrepare(
        CharacterCreateDraft selected,
        GameSaveData saved,
        out string error)
    {
        error = null;
        profile = selected;

        if (saved != null)
        {
            if (!saved.Validate(out error))
                return false;

            if (!inventory.RestoreSaveItems(saved.inventoryItems, out error))
                return false;

            shop.RestoreMoney(saved.money);
        }
        else
        {
            if (!WriteSnapshot(true, out error))
                return false;
        }

        ready = true;
        return true;
    }

    private bool WriteSnapshot(bool firstSave, out string error)
    {
        GameSaveData data = new GameSaveData
        {
            version = GameSaveData.CurrentVersion,
            profile = profile,
            money = shop.Money,
            inventoryItems = inventory.CaptureSaveItems()
        };

        if (!inventory.ValidateSaveItems(data.inventoryItems, out error))
            return false;

        if (!GameSaveFile.TrySave(data, firstSave, out error))
            return false;

        GameSession.SetData(data);
        dirty = false;

        return true;
    }

    private void MarkDirty()
    {
        if (!ready || dirty)
            return;

        dirty = true;

        // 슬롯 이동·내구도 변경이 연속으로 발생하면 한 번에 저장한다.
        nextSaveTime = Time.unscaledTime + 1f;
    }

    private void SaveAfterTrade()
    {
        MarkDirty();
        Flush();
    }

    private void Update()
    {
        if (dirty && Time.unscaledTime >= nextSaveTime)
            Flush();
    }

    private void Flush()
    {
        if (!ready || !dirty || shop.IsTrading)
            return;

        if (!WriteSnapshot(false, out string error))
        {
            Debug.LogError(error, this);

            // 실패해도 변경 표시를 유지해서 다음에 다시 저장한다.
            nextSaveTime = Time.unscaledTime + 5f;
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            Flush();
    }

    private void OnApplicationQuit()
    {
        Flush();
    }

    private void OnDisable()
    {
        inventory.OnChanged -= MarkDirty;
        shop.TradeCompleted -= SaveAfterTrade;

        Flush();
    }
}