using UnityEngine;

public class SetupPlacementSystem : MonoBehaviour
{
    [Header("Setup State")]
    public int setupRound = 1;
    public OwnerType currentOwner = OwnerType.Player;
    public PlacementPhase currentPlacementPhase = PlacementPhase.None;

    [Header("Selected Point")]
    private NodeControler selectedPointNode;

    [Header("References")]
    [SerializeField] private GameStateMachine gameStateMachine;
    [SerializeField] private GameUIController uiController;

    private const int MaxSetupRound = 2;

    private void Awake()
    {
        if (gameStateMachine == null)
        {
            gameStateMachine = FindObjectOfType<GameStateMachine>();
        }

        if (uiController == null)
        {
            uiController = FindObjectOfType<GameUIController>();
        }
    }

    public void StartSetupPlacement()
    {
        setupRound = 1;
        currentOwner = OwnerType.Player;
        currentPlacementPhase = PlacementPhase.PlaceRecognitionPoint;
        selectedPointNode = null;

        AddLog("初始放置开始：第 1 轮，玩家放置 1 个认同点。");
    }

    public bool IsInSetupPlacement()
    {
        return gameStateMachine != null &&
               gameStateMachine.currentState == GameState.SetupPlacement;
    }

    public void OnNodeClicked(NodeControler node)
    {
        if (!IsInSetupPlacement())
        {
            return;
        }

        if (currentPlacementPhase != PlacementPhase.PlaceRecognitionPoint)
        {
            AddLog("当前需要点击与认同点相连的 Edge。");
            return;
        }

        TryPlaceSetupRecognitionPoint(node);
    }

    public void OnEdgeClicked(EdgeControler edge)
    {
        if (!IsInSetupPlacement())
        {
            return;
        }

        if (currentPlacementPhase != PlacementPhase.PlaceBond)
        {
            AddLog("当前需要先点击一个空 Node 放置认同点。");
            return;
        }

        TryPlaceSetupBond(edge);
    }

    private void TryPlaceSetupRecognitionPoint(NodeControler node)
    {
        if (node == null)
        {
            return;
        }

        if (!node.IsEmpty())
        {
            AddLog("这个 Node 已经有建筑，不能作为初始认同点。");
            return;
        }

        node.SetRecognitionPoint(currentOwner);
        selectedPointNode = node;

        CharacterData character = GetCurrentCharacter();
        character.AddRecognitionPoint();

        currentPlacementPhase = PlacementPhase.PlaceBond;

        AddLog($"{GetOwnerName(currentOwner)} 初始放置认同点成功。请选择一条与该认同点相连的 Edge 放置纽带。");

        RefreshUI();
    }

    private void TryPlaceSetupBond(EdgeControler edge)
    {
        if (edge == null)
        {
            return;
        }

        if (!edge.CanBuildBond())
        {
            AddLog("这条 Edge 已经有纽带，不能放置。");
            return;
        }

        if (selectedPointNode == null)
        {
            AddLog("没有已选择的认同点，无法放置纽带。");
            return;
        }

        if (!selectedPointNode.edgeIds.Contains(edge.id))
        {
            AddLog("初始纽带必须连接刚刚放置的认同点。");
            return;
        }

        edge.SetBond(currentOwner);

        CharacterData character = GetCurrentCharacter();
        character.AddBond();

        AddLog($"{GetOwnerName(currentOwner)} 初始放置纽带成功。");

        AdvanceSetupStep();

        RefreshUI();
    }

    private void AdvanceSetupStep()
    {
        selectedPointNode = null;
        currentPlacementPhase = PlacementPhase.PlaceRecognitionPoint;

        if (currentOwner == OwnerType.Player)
        {
            currentOwner = OwnerType.NPC;
            AddLog($"第 {setupRound} 轮：轮到 NPC 放置认同点。");

            // 如果你希望 NPC 自动放置，打开这一句
            AutoPlaceForNPC();
        }
        else
        {
            setupRound++;

            if (setupRound > MaxSetupRound)
            {
                FinishSetupPlacement();
                return;
            }

            currentOwner = OwnerType.Player;
            AddLog($"第 {setupRound} 轮：轮到玩家放置认同点。");
        }
    }

    private void FinishSetupPlacement()
    {
        currentPlacementPhase = PlacementPhase.None;
        currentOwner = OwnerType.None;
        selectedPointNode = null;

        AddLog("初始放置完成，进入正式游戏回合。");

        if (gameStateMachine != null)
        {
            gameStateMachine.EnterPlayerTurnStartFromSetup();
        }
    }

    private void AutoPlaceForNPC()
    {
        NodeControler[] nodes = FindObjectsOfType<NodeControler>();

        foreach (NodeControler node in nodes)
        {
            if (!node.IsEmpty())
            {
                continue;
            }

            node.SetRecognitionPoint(OwnerType.NPC);
            selectedPointNode = node;

            GameDataManager.Instance.npcData.AddRecognitionPoint();

            AddLog("NPC 初始放置认同点。");

            break;
        }

        if (selectedPointNode == null)
        {
            AddLog("NPC 找不到可放置认同点的 Node。");
            return;
        }

        EdgeControler[] edges = FindObjectsOfType<EdgeControler>();

        foreach (EdgeControler edge in edges)
        {
            if (!edge.CanBuildBond())
            {
                continue;
            }

            if (!selectedPointNode.edgeIds.Contains(edge.id))
            {
                continue;
            }

            edge.SetBond(OwnerType.NPC);
            GameDataManager.Instance.npcData.AddBond();

            AddLog("NPC 初始放置纽带。");

            break;
        }

        // NPC 自动放完后，推进到下一步
        AdvanceSetupStep();
    }

    private CharacterData GetCurrentCharacter()
    {
        if (currentOwner == OwnerType.Player)
        {
            return GameDataManager.Instance.playerData;
        }

        return GameDataManager.Instance.npcData;
    }

    private string GetOwnerName(OwnerType owner)
    {
        if (owner == OwnerType.Player)
        {
            return "玩家";
        }

        if (owner == OwnerType.NPC)
        {
            return "NPC";
        }

        return "无";
    }

    private void AddLog(string message)
    {
        Debug.Log(message);

        if (uiController != null)
        {
            uiController.AddLog(message);
        }
    }

    private void RefreshUI()
    {
        if (uiController != null)
        {
            uiController.RefreshAll();
        }
    }
}