using UnityEngine;

public class ProductionSystem : MonoBehaviour
{
    public void ResolveProduction(int diceTotal)
    {
        ResourceType resourceType = GetResourceByDiceNumber(diceTotal);

        CharacterData player = GameDataManager.Instance.playerData;
        CharacterData npc = GameDataManager.Instance.npcData;

        player.AddResource(resourceType, 1);
        npc.AddResource(resourceType, 1);

        Debug.Log($"资源生产：骰子点数 {diceTotal}，玩家和 NPC 各获得 1 个 {resourceType}");
    }

    private ResourceType GetResourceByDiceNumber(int diceTotal)
    {
        // 临时规则。
        // 之后接地图时，这里改成根据地块数字和地块资源类型生产。
        switch (diceTotal)
        {
            case 2:
            case 3:
                return ResourceType.Food;

            case 4:
            case 5:
                return ResourceType.Housing;

            case 6:
            case 8:
                return ResourceType.Medical;

            case 9:
            case 10:
                return ResourceType.Legal;

            case 11:
            case 12:
                return ResourceType.Credit;

            default:
                return ResourceType.Food;
        }
    }
}