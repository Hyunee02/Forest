using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class GameSaveFile
{
    // 저장 파일 경로
    public static string SavePath =>
        Path.Combine(
            Application.persistentDataPath,
            "forest_save.json");

    public static string BackupPath => SavePath + ".bak";

    public static bool Exists =>
        File.Exists(SavePath) || File.Exists(BackupPath);
    

    /// <summary>
    /// 게임 저장
    /// </summary>
    /// <param name="data"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static bool TrySave(
        GameSaveData data,
        out string error)
    {
        error = "";

        string tempPath = SavePath + ".tmp";

        try
        {
            // 저장 폴더 생성
            Directory.CreateDirectory(
                Application.persistentDataPath);

            // JSON 변환
            string json = JsonUtility.ToJson(data, true);

            // 파일 쓰기
            File.WriteAllText(
                tempPath,
                json,
                new UTF8Encoding(false));

            // 기존 파일 교체
            if (File.Exists(SavePath))
            {
                // 기존 저장을 백업하면서 새 저장으로 교체
                File.Replace(
                    tempPath,
                    SavePath,
                    BackupPath);
            }
            else
            {
                File.Move(tempPath, SavePath);
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            error = "저장하지 못했습니다. 저장 공간과 파일 접근 상태를 확인해주세요.";
            return false;
        }
    }

    /// <summary>
    /// 저장 파일 불러오기
    /// </summary>
    /// <param name="inventory"></param>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static bool TryLoad(
        PlayerInventory inventory,
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
                // JSON을 객체로 다시 변환
                string json = File.ReadAllText(paths[i]);
                GameSaveData loaded =
                    JsonUtility.FromJson<GameSaveData>(json);

                // 이전 버전 데이터 변환
                UpgradeLegacyData(loaded, inventory);

                // 저장 데이터 검증
                if (!Validate(loaded, inventory, out string error))
                {
                    Debug.LogWarning(
                        $"저장 데이터 검사 실패: {paths[i]}\n{error}");

                    continue;
                }

                // 불러오기 성공
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

    /// <summary>
    /// 이전 저장 데이터 변환
    /// </summary>
    /// <param name="data"></param>
    /// <param name="inventory"></param>
    public static void UpgradeLegacyData(
        GameSaveData data,
        PlayerInventory inventory)
    {
        if (data == null || data.version != 1)
            return;

        // 기존 버전에는 인벤토리·날짜·거래 기록이 없었음
        data.version = GameSaveData.CurrentVersion;
        data.day = 1;
        data.tradeDay = 1;
        data.dailyTrades = new List<ShopDailyRecord>();

        data.inventory = new List<InventoryItem>();

        for (int i = 0; i < inventory.SlotCount; i++)
            data.inventory.Add(new InventoryItem());
    }

    /// <summary>
    /// 전체 저장 데이터 검증
    /// </summary>
    /// <param name="data"></param>
    /// <param name="inventory"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static bool Validate(
        GameSaveData data,
        PlayerInventory inventory,
        out string error)
    {
        error = "";

        if (data == null ||
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

        if (!inventory.ValidateSaveItems(
                data.inventory,
                out error))
        {
            return false;
        }

        HashSet<(string, string)> keys = new();

        foreach (ShopDailyRecord record in data.dailyTrades)
        {
            if (record == null ||
                string.IsNullOrWhiteSpace(record.shopId) ||
                string.IsNullOrWhiteSpace(record.itemId) ||
                record.bought < 0 ||
                record.sold < 0 ||
                !keys.Add((record.shopId, record.itemId)))
            {
                error = "일일 거래 기록이 올바르지 않습니다.";
                return false;
            }
        }

        return true;
    }
}