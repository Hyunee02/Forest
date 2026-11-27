using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class GameSaveFile
{
    public static string SavePath =>
        Path.Combine(Application.persistentDataPath, "player_save.json");

    public static bool Exists => File.Exists(SavePath);

    public static bool TryLoad(out GameSaveData data, out string error)
    {
        data = null;

        try
        {
            string json = File.ReadAllText(SavePath);

            // JSON에서 필수 숫자 필드가 빠졌을 때도 검사할 수 있게 한다.
            GameSaveData candidate = new GameSaveData
            {
                version = -1,
                money = -1
            };

            JsonUtility.FromJsonOverwrite(json, candidate);

            if (!candidate.Validate(out error))
                return false;

            data = candidate;
            return true;
        }
        catch (FileNotFoundException)
        {
            error = "이어갈 저장 파일이 없어요.";
        }
        catch (Exception exception)
        {
            error = $"저장 파일을 읽을 수 없어요. {exception.Message}";
        }

        return false;
    }

    public static bool TrySave(
        GameSaveData data,
        bool firstSave,
        out string error)
    {
        if (!data.Validate(out error))
            return false;

        string path = SavePath;
        string temp = path + ".tmp";

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);

            string json = JsonUtility.ToJson(data, true);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            using (FileStream stream = new FileStream(
                temp,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }

            if (firstSave)
                File.Move(temp, path);
            else
                File.Replace(temp, path, null);

            return true;
        }
        catch (Exception exception)
        {
            error = $"저장하지 못했어요. {exception.Message}";
            return false;
        }
    }
}