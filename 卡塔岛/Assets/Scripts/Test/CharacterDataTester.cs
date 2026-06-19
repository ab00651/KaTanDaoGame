using UnityEngine;

public class CharacterDataTester : MonoBehaviour
{
    private ResourceCost buildBondCost;
    private ResourceCost buildRecognitionPointCost;
    private ResourceCost upgradeRecognitionCenterCost;

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestBuildBond();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestBuildRecognitionPoint();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestUpgradeRecognitionCenter();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestAddCrack();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            PrintPlayerData();
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            TestAddResources();
        }
    }

    private void TestBuildBond()
    {
        CharacterData player = GameDataManager.Instance.playerData;
        ResourceCost cost = BuildCosts.CreateBuildBondCost();

        if (player.SpendResources(cost))
        {
            player.AddBond();
        }

        PrintPlayerData();
    }

    private void TestBuildRecognitionPoint()
    {
        CharacterData player = GameDataManager.Instance.playerData;
        ResourceCost cost = BuildCosts.CreateBuildRecognitionPointCost();

        if (player.SpendResources(cost))
        {
            player.AddRecognitionPoint();
        }

        GameDataManager.Instance.CheckGameEnd();
        PrintPlayerData();
    }

    private void TestUpgradeRecognitionCenter()
    {
        CharacterData player = GameDataManager.Instance.playerData;

        if (!player.CanUpgradeRecognitionCenter())
        {
            Debug.Log("没有认同点，无法升级认同中心。");
            PrintPlayerData();
            return;
        }

        ResourceCost cost = BuildCosts.CreateUpgradeRecognitionCenterCost();

        if (player.SpendResources(cost))
        {
            player.UpgradeRecognitionCenter();
        }
        else
        {
            Debug.Log("资源不足，无法升级认同中心。需要：2 法律 + 3 医疗。");
        }

        GameDataManager.Instance.CheckGameEnd();
        PrintPlayerData();
    }

    private void TestAddCrack()
    {
        CharacterData player = GameDataManager.Instance.playerData;

        player.AddCrack();

        PrintPlayerData();
    }

    private void TestAddResources()
    {
        CharacterData player = GameDataManager.Instance.playerData;

        player.AddResource(ResourceType.Food, 3);
        player.AddResource(ResourceType.Housing, 3);
        player.AddResource(ResourceType.Medical, 3);
        player.AddResource(ResourceType.Legal, 3);
        player.AddResource(ResourceType.Credit, 3);

        PrintPlayerData();
    }

    private void PrintPlayerData()
    {
        CharacterData player = GameDataManager.Instance.playerData;
        Debug.Log(player.GetDebugText());
    }
}