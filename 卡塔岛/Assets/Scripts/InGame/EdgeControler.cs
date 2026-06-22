using UnityEngine;

public class EdgeControler : MonoBehaviour
{
    public int id;
    public int nodeIdA;
    public int nodeIdB;

    public OwnerType owner;
    public bool hasBond;

    private EdgeData data;
    private SpriteRenderer spriteRenderer;

    public void InitFromData(EdgeData data)
    {
        this.data = data;

        id = data.id;
        nodeIdA = data.nodeIdA;
        nodeIdB = data.nodeIdB;

        owner = data.owner;
        hasBond = data.hasBond;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        RefreshVisual();
    }

    private void OnMouseDown()
    {
        BuildPlacementSystem buildPlacementSystem = FindObjectOfType<BuildPlacementSystem>();

        if (buildPlacementSystem != null)
        {
            buildPlacementSystem.OnEdgeClicked(this);
        }
    }

    public bool CanBuildBond()
    {
        return owner == OwnerType.None && !hasBond;
    }

    public void SetBond(OwnerType newOwner)
    {
        owner = newOwner;
        hasBond = true;

        if (data != null)
        {
            data.owner = newOwner;
            data.hasBond = true;
        }

        RefreshVisual();

        Debug.Log($"Edge {id} 建造纽带。Owner = {newOwner}");
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

        // 没有纽带
        if (!hasBond)
        {
            spriteRenderer.color = Color.white;
            return;
        }

        // 玩家纽带
        if (owner == OwnerType.Player)
        {
            spriteRenderer.color = Color.red;
            return;
        }

        // NPC 纽带
        if (owner == OwnerType.NPC)
        {
            spriteRenderer.color = Color.black;
            return;
        }
        
        spriteRenderer.color = Color.gray;
    }
}