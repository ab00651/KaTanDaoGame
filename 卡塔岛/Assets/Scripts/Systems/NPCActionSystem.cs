using UnityEngine;

public class NPCActionSystem : MonoBehaviour
{
    [SerializeField] private BuildSystem buildSystem;

    private void Awake()
    {
        if (buildSystem == null)
        {
            buildSystem = GetComponent<BuildSystem>();
        }
    }

    public void ExecuteNPCAction()
    {
        CharacterData npc = GameDataManager.Instance.npcData;

        if (buildSystem.CanUpgradeRecognitionCenter(npc))
        {
            buildSystem.UpgradeRecognitionCenter(npc);
            Debug.Log("NPC 行动：升级认同中心。");
        }
        else if (buildSystem.CanBuildRecognitionPoint(npc))
        {
            buildSystem.BuildRecognitionPoint(npc);
            Debug.Log("NPC 行动：建造认同点。");
        }
        else if (buildSystem.CanBuildBond(npc))
        {
            buildSystem.BuildBond(npc);
            Debug.Log("NPC 行动：建造纽带。");
        }
        else
        {
            Debug.Log("NPC 资源不足，无法建设，结束行动。");
        }
    }
}