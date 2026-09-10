using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterinBuilding : Interactable
{
    private enum BuildingType
    {
        Convenience,
        Bank,
        PlayerHouse,
        NPC1House,
        NPC2House
    }

    [Header("<< 건물 타입 >>")]
    [SerializeField] private BuildingType buildingType;

    [Header("<< 이동할 씬 이름 >>")]
    [SerializeField] private string convenienceSceneName = "Convenience";
    [SerializeField] private string bankSceneName = "Bank";
    [SerializeField] private string tentSceneName = "Tent";
    [SerializeField] private string playerHouseSceneName = "PlayerHouse";
    [SerializeField] private string npc1HouseSceneName = "NPC1House";
    [SerializeField] private string npc2HouseSceneName = "NPC2House";

    protected override void Interact()
    {
        EnterBuilding();
    }

    private void EnterBuilding()
    {
        switch (buildingType)
        {
            case BuildingType.Convenience:
                SceneManager.LoadScene(convenienceSceneName);
                break;

            case BuildingType.Bank:
                SceneManager.LoadScene(bankSceneName);
                break;

            case BuildingType.PlayerHouse:
                EnterPlayerHouse();
                break;

            case BuildingType.NPC1House:
                SceneManager.LoadScene(npc1HouseSceneName);
                break;

            case BuildingType.NPC2House:
                SceneManager.LoadScene(npc2HouseSceneName);
                break;
        }
    }

    private void EnterPlayerHouse()
    {
        int houseLevel = HouseUpgradeManager.Instance.HouseLevel;

        if (houseLevel == 0)
        {
            SceneManager.LoadScene(tentSceneName);
        }
        else
        {
            SceneManager.LoadScene(playerHouseSceneName);
        }
    }
}