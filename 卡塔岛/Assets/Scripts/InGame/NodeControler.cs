using System.Collections.Generic;
using UnityEngine;

public class NodeControler : MonoBehaviour
{
    public int id;
    public List<int> edgeIds;   // 与之连接的3条边ID
    
    public OwnerType owner;
    public NodeBuildingType buildingType;
    
    private NodeData data;
    private SpriteRenderer spriteRenderer;

    public void InitFromData(NodeData data)
    {
        this.data = data;
        id = data.id;
        edgeIds = new List<int>(data.edgeIds);
        
        owner = data.owner;
        buildingType = data.buildingType;
        
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        RefreshVisual();
    }
    private void OnMouseDown()
    {
        BuildPlacementSystem buildPlacementSystem = FindObjectOfType<BuildPlacementSystem>();

        if (buildPlacementSystem != null)
        {
            buildPlacementSystem.OnNodeClicked(this);
        }
    }

    public bool IsEmpty()
    {
        return owner == OwnerType.None &&
               buildingType == NodeBuildingType.None;
    }

    public bool HasPlayerRecognitionPoint()
    {
        return owner == OwnerType.Player &&
               buildingType == NodeBuildingType.RecognitionPoint;
    }
    
    public bool HasRecognitionPoint(OwnerType targetOwner)
    {
        return owner == targetOwner &&
               buildingType == NodeBuildingType.RecognitionPoint;
    }

    public void SetRecognitionPoint(OwnerType newOwner)
    {
        owner = newOwner;
        buildingType = NodeBuildingType.RecognitionPoint;

        if (data != null)
        {
            data.owner = newOwner;
            data.buildingType = NodeBuildingType.RecognitionPoint;
        }

        RefreshVisual();

        Debug.Log($"Node {id} 建造认同点。Owner = {newOwner}");
    }

    public void UpgradeToRecognitionCenter()
    {
        buildingType = NodeBuildingType.RecognitionCenter;

        if (data != null)
        {
            data.owner = owner;
            data.buildingType = NodeBuildingType.RecognitionCenter;
        }

        RefreshVisual();

        Debug.Log($"Node {id} 升级为认同中心。");
    }

    private void RefreshVisual()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            return;
        }

        if (buildingType == NodeBuildingType.None)
        {
            spriteRenderer.color = Color.white;
            return;
        }
        if (owner == OwnerType.Player)
        {
            if (buildingType == NodeBuildingType.RecognitionPoint)
            {
                spriteRenderer.color = Color.yellow;
            }
            else if (buildingType == NodeBuildingType.RecognitionCenter)
            {
                spriteRenderer.color = Color.red;
            }

            return;
        }

    // NPC 建筑
    if (owner == OwnerType.NPC)
    {
        if (buildingType == NodeBuildingType.RecognitionPoint)
        {
            spriteRenderer.color = Color.green;
        }
        else if (buildingType == NodeBuildingType.RecognitionCenter)
        {
            spriteRenderer.color = Color.blue;
        }

        return;
    }
        
    }}
