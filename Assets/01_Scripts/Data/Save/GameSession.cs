using UnityEngine;

public static class GameSession
{
    public static CharacterCreateDraft Profile { get; private set; }
    public static GameSaveData CurrentData { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Clear()
    {
        Profile = null;
        CurrentData = null;
    }

    // CharacterSelectUI에서 캐릭터 생성을 확정할 때 호출한다.
    public static void SetProfile(CharacterCreateDraft profile)
    {
        Profile = profile;
        CurrentData = null;
    }

    // 이어하기 또는 저장 성공 시 호출한다.
    public static void SetData(GameSaveData data)
    {
        Profile = data.profile;
        CurrentData = data;
    }
}