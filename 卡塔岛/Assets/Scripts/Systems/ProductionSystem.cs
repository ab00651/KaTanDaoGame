using System.Collections.Generic;
using UnityEngine;

public class ProductionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Map map;

    private void Awake()
    {
        if (map == null)
        {
            map = FindObjectOfType<Map>();
        }
    }

    public void ResolveProduction(int diceTotal)
    {
        if (map == null)
        {
            Debug.LogError("ProductionSystem: 没有找到 Map，无法进行资源生产。");
            return;
        }
    
        List<HexTileData> tiles = map.GetAllTiles();
        bool hasProduced = false;
    
        foreach (HexTileData tile in tiles)
        {
            if (tile.diceNumber != diceTotal)
            {
                continue;
            }
    
            if (!TryGetResourceType(tile.terrainType, out ResourceType resourceType))
            {
                Debug.Log($"地块 {tile.positionNumber} 的类型 {tile.terrainType} 不产生资源。");
                continue;
            }
    
            foreach (int nodeId in tile.nodeIds)
            {
                NodeData node = map.GetNode(nodeId);
    
                if (node == null)
                {
                    continue;
                }
    
                int amount = GetProductionAmount(node);
    
                if (amount <= 0)
                {
                    continue;
                }
    
                if (node.owner == OwnerType.Player)
                {
                    GameDataManager.Instance.playerData.AddResource(resourceType, amount);
                    Debug.Log($"玩家从地块 {tile.positionNumber} 获得 {amount} 个 {resourceType}");
                    hasProduced = true;
                }
                else if (node.owner == OwnerType.NPC)
                {
                    GameDataManager.Instance.npcData.AddResource(resourceType, amount);
                    Debug.Log($"NPC 从地块 {tile.positionNumber} 获得 {amount} 个 {resourceType}");
                    hasProduced = true;
                }
            }
        }
    
        if (!hasProduced)
        {
            Debug.Log($"骰到 {diceTotal}，没有建筑获得资源。");
        }
    }
    
    private int GetProductionAmount(NodeData node)
    {
        if (node.buildingType == NodeBuildingType.RecognitionPoint)
        {
            return 1;
        }
    
        if (node.buildingType == NodeBuildingType.RecognitionCenter)
        {
            return 2;
        }
    
        return 0;
    }

    private bool TryGetResourceType(string terrainType, out ResourceType resourceType)
    {
        switch (terrainType)
        {
            case "farmland":
                resourceType = ResourceType.Food;
                return true;

            case "factory":
                resourceType = ResourceType.Housing;
                return true;

            case "hospital":
                resourceType = ResourceType.Medical;
                return true;

            case "government":
                resourceType = ResourceType.Legal;
                return true;

            case "bank":
                resourceType = ResourceType.Credit;
                return true;

            default:
                resourceType = ResourceType.Food;
                return false;
        }
    }
}