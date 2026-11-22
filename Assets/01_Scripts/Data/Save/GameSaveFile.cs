using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class GameSaveFile
{
    public static string SavePath =>
        Path.Combine(Application.persistentDataPath, "forest_save.json");

    public static string BackupPath => SavePath + ".bak";

    public static bool Exists =>
        File.Exists(SavePath) || File.Exists(BackupPath);

    public delegate bool AcceptSave(GameSaveData candidate, out string error);

    public static bool TrySave(GameSaveData data, out string error)
    {
        error = "";

        if (data == null || data.version != GameSaveData.CurrentVersion ||
            data.profile == null || data.inventory == null)
        {
            error = "저장에 필요한 데이터가 준비되지 않았습니다.";
            return false;
        }

        string tempPath = SavePath + ".tmp";

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(tempPath, json, new UTF8Encoding(false));

            if (File.Exists(SavePath))
                File.Replace(tempPath, SavePath, BackupPath);
            else
                File.Move(tempPath, SavePath);

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            error = "저장하지 못했습니다. 저장 공간과 파일 접근 상태를 확인해주세요.";
            return false;
        }
    }

    public static bool TryLoad(
        PlayerInventory inventory,
        out GameSaveData data,
        out string message)
    {
        bool Accept(GameSaveData candidate, out string error)
        {
            UpgradeLegacyData(candidate, inventory);
            return Validate(candidate, inventory, out error);
        }

        return TryLoad(Accept, out data, out message);
    }

    public static bool TryLoad(
        AcceptSave accept,
        out GameSaveData data,
        out string message)
    {
        data = null;
        message = "";

        string[] paths = { SavePath, BackupPath };

        for (int i = 0; i < paths.Length; i++)
        {
            if (!File.Exists(paths[i]))
                continue;

            try
            {
                string json = File.ReadAllText(paths[i]);
                GameSaveData loaded = JsonUtility.FromJson<GameSaveData>(json);

                if (!accept(loaded, out string error))
                {
                    Debug.LogWarning($"저장 데이터 검사 실패: {paths[i]}\n{error}");
                    continue;
                }

                data = loaded;
                message = i == 0
                    ? "불러왔습니다."
                    : "기본 저장을 읽지 못해 직전 백업을 불러왔습니다.";

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        message = "사용할 수 있는 저장 파일을 찾지 못했습니다.";
        return false;
    }

    public static void UpgradeLegacyData(
        GameSaveData data,
        PlayerInventory inventory)
    {
        if (inventory == null || data == null || data.version != 1)
            return;

        data.version = GameSaveData.CurrentVersion;

        if (data.day == 0)
            data.day = 1;

        if (data.tradeDay == 0)
            data.tradeDay = data.day;

        data.dailyTrades ??= new List<ShopDailyRecord>();

        if (data.inventory == null || data.inventory.Count == 0)
        {
            data.inventory = new List<InventoryItem>(inventory.SlotCount);

            for (int i = 0; i < inventory.SlotCount; i++)
                data.inventory.Add(new InventoryItem());
        }
    }

    public static bool Validate(
        GameSaveData data,
        PlayerInventory inventory,
        out string error)
    {
        error = "";

        if (inventory == null || data == null ||
            data.version != GameSaveData.CurrentVersion ||
            data.profile == null ||
            data.money < 0 ||
            data.day < 1 ||
            data.tradeDay != data.day ||
            data.dailyTrades == null)
        {
            error = "저장 데이터의 기본 정보가 올바르지 않습니다.";
            return false;
        }

        if (!inventory.ValidateSaveItems(data.inventory, out error))
            return false;

        HashSet<(string, string)> keys = new();

        foreach (ShopDailyRecord record in data.dailyTrades)
        {
            if (record == null ||
                string.IsNullOrWhiteSpace(record.shopId) ||
                string.IsNullOrWhiteSpace(record.itemId) ||
                record.bought < 0 ||
                !keys.Add((record.shopId, record.itemId)))
            {
                error = "일일 거래 기록이 올바르지 않습니다.";
                return false;
            }
        }

        return true;
    }
}