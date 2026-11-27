using System.IO;
using UnityEditor;
using UnityEngine;

public static class SaveFileMenu
{
    [MenuItem("Tools/Save/저장 파일 위치 열기")]
    private static void ShowSaveFile()
    {
        string path = GameSaveFile.SavePath;

        Debug.Log($"플레이어 저장 파일 경로: {path}");

        if (File.Exists(path))
        {
            EditorUtility.RevealInFinder(path);
            return;
        }

        EditorUtility.DisplayDialog(
            "플레이어 저장 파일",
            $"아직 저장 파일이 없어요.\n\n예정 경로\n{path}\n\n" +
            "Title 씬에서 새 게임을 시작하고 Player 씬까지 입장해주세요.",
            "확인");
    }
}