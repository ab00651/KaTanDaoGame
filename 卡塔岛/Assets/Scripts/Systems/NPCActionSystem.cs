using UnityEngine;

public class NPCActionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameUIController uiController;

    private void Awake()
    {
        if (uiController == null)
        {
            uiController = FindObjectOfType<GameUIController>();
        }
    }

    public void ExecuteNPCAction()
    {
        if (GameDataManager.Instance == null)
        {
            Debug.LogError("NPCActionSystem: 没有找到 GameDataManager。");
            return;
        }

        CharacterData npc = GameDataManager.Instance.npcData;

        if (TryUpgradeRecognitionCenterOnMap(npc))
        {
            AddLogAndRefreshUI("NPC 行动：升级认同中心。");
        }
        else if (TryBuildRecognitionPointOnMap(npc))
        {
            AddLogAndRefreshUI("NPC 行动：建造认同点。");
        }
        else if (TryBuildBondOnMap(npc))
        {
            AddLogAndRefreshUI("NPC 行动：建造纽带。");
        }
        else
        {
            AddLogAndRefreshUI("NPC 资源不足或没有可建造位置，结束行动。");
        }
    }

    private bool TryUpgradeRecognitionCenterOnMap(CharacterData npc)
    {
        if (!npc.CanUpgradeRecognitionCenter())
        {
            return false;
        }

        ResourceCost cost = BuildCosts.CreateUpgradeRecognitionCenterCost();

        if (!npc.CanAfford(cost))
        {
            return false;
        }

        NodeControler[] nodes = FindObjectsOfType<NodeControler>();

        foreach (NodeControler node in nodes)
        {
            if (node.HasRecognitionPoint(OwnerType.NPC))
            {
                if (!npc.SpendResources(cost))
                {
                    return false;
                }

                node.UpgradeToRecognitionCenter();
                npc.UpgradeRecognitionCenter();

                return true;
            }
        }

        return false;
    }

    private bool TryBuildRecognitionPointOnMap(CharacterData npc)
    {
        ResourceCost cost = BuildCosts.CreateBuildRecognitionPointCost();

        if (!npc.CanAfford(cost))
        {
            return false;
        }

        NodeControler[] nodes = FindObjectsOfType<NodeControler>();

        foreach (NodeControler node in nodes)
        {
            if (node.IsEmpty())
            {
                if (!npc.SpendResources(cost))
                {
                    return false;
                }

                node.SetRecognitionPoint(OwnerType.NPC);
                npc.AddRecognitionPoint();

                return true;
            }
        }

        return false;
    }

    private bool TryBuildBondOnMap(CharacterData npc)
    {
        ResourceCost cost = BuildCosts.CreateBuildBondCost();

        if (!npc.CanAfford(cost))
        {
            return false;
        }

        EdgeControler[] edges = FindObjectsOfType<EdgeControler>();

        foreach (EdgeControler edge in edges)
        {
            if (edge.CanBuildBond())
            {
                if (!npc.SpendResources(cost))
                {
                    return false;
                }

                edge.SetBond(OwnerType.NPC);
                npc.AddBond();

                return true;
            }
        }

        return false;
    }

    private void AddLogAndRefreshUI(string message)
    {
        Debug.Log(message);
        if (uiController != null)
        {
            uiController.RefreshAll();
        }
    }
}