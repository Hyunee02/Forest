using UnityEngine;

public static class GameSession
{
    public static CharacterCreateDraft Profile { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        Profile = null;
    }

    public static void SetProfile(CharacterCreateDraft profile)
    {
        Profile = profile;
    }
}