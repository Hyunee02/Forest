using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterinBuilding : Interactable
{
    [Header("<< 이동할 씬 이름 >>")]
    [SerializeField] private string convenienceSceneName = "Convenience";
    [SerializeField] private string bankSceneName = "Bank";
    [SerializeField] private string tentSceneName = "Tent";
    [SerializeField] private string npc1HouseSceneName = "NPC1House";
    [SerializeField] private string npc2HouseSceneName = "NPC2House";

    protected override void Interact()
    {
        EnterBuilding();
    }

    private void EnterBuilding()
    {
        if (CompareTag("EnterinConvenience"))
        {
            SceneManager.LoadScene(convenienceSceneName);
        }
        else if (CompareTag("EnterinBank"))
        {
            SceneManager.LoadScene(bankSceneName);
        }
        else if (CompareTag("EnterinTent"))
        {
            SceneManager.LoadScene(tentSceneName);
        }
        else if (CompareTag("EnterinNPC1House"))
        {
            SceneManager.LoadScene(npc1HouseSceneName);
        }
        else if (CompareTag("EnterinNPC2House"))
        {
            SceneManager.LoadScene(npc2HouseSceneName);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}에 입장 태그가 설정되어 있지 않습니다.");
        }
    }
}