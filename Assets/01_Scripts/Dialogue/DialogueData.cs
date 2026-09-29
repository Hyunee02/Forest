using UnityEngine;

[CreateAssetMenu(
    fileName = "DialogueData",
    menuName = "Game/Dialogue Data"
)]
public class DialogueData : ScriptableObject
{
    [TextArea(2, 4)]
    public string[] greetings;

    [Header("<< Yes >>")]
    [TextArea(2, 4)]
    public string yesDialogue;

    [Header("<< No >>")]
    [TextArea(2, 4)]
    public string noDialogue;

    public string GetRandomGreeting()
    {
        if (greetings == null || greetings.Length == 0)
            return string.Empty;

        int randomIndex = Random.Range(0, greetings.Length);

        return greetings[randomIndex];
    }
}