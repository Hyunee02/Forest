using UnityEngine;

[System.Serializable]
public class DialogueChoiceData
{
    public string choiceText;

    [TextArea(2, 4)]
    public string dialogue;
}

[CreateAssetMenu(
    fileName = "DialogueData",
    menuName = "Game/Dialogue Data"
)]
public class DialogueData : ScriptableObject
{
    [Header("<< NPC ÀÌ¸§ >>")]
    public string npcName;

    [Header("<< Greetings >>")]
    [TextArea(2, 4)]
    public string[] greetings;

    [Header("<< Choices >>")]
    public DialogueChoiceData[] choices;

    [Header("<< Bye >>")]
    [TextArea(2, 4)]
    public string[] byes;

    public string GetRandomGreeting()
    {
        if (greetings == null || greetings.Length == 0)
            return string.Empty;

        int randomIndex = Random.Range(0, greetings.Length);

        return greetings[randomIndex];
    }

    public string GetRandomBye()
    {
        if (byes == null || byes.Length == 0)
            return string.Empty;

        int randomIndex = Random.Range(0, byes.Length);

        return byes[randomIndex];
    }
}