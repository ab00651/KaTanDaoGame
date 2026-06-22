using UnityEngine;

public class BuildPlacementSystem : MonoBehaviour
{
    [Header("Current Build Mode")]
    public BuildMode currentBuildMode = BuildMode.None;

    [Header("References")]
    [SerializeField] private GameStateMachine gameStateMachine;
    [SerializeField] private GameUIController uiController;

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

    public void SelectBuildBondMode()
    {
        if (!CanPlayerBuildNow())
        {
            return;
        }

        currentBuildMode = BuildMode.BuildBond;
        Debug.Log("进入建造纽带模式：请点击一条 Edge。");
    }

    public void SelectBuildRecognitionPointMode()
    {
        if (!CanPlayerBuildNow())
        {
            return;
        }

        currentBuildMode = BuildMode.BuildRecognitionPoint;
        Debug.Log("进入建造认同点模式：请点击一个空 Node。");
    }

    public void SelectUpgradeRecognitionCenterMode()
    {
        if (!CanPlayerBuildNow())
        {
            return;
        }

        currentBuildMode = BuildMode.UpgradeRecognitionCenter;
        Debug.Log("进入升级认同中心模式：请点击一个自己的认同点 Node。");
    }

    public void CancelBuildMode()
    {
        currentBuildMode = BuildMode.None;
        Debug.Log("取消建造模式。");
    }

    public void OnNodeClicked(NodeControler node)
    {
        if (!CanPlayerBuildNow())
        {
            return;
        }

        if (currentBuildMode == BuildMode.BuildRecognitionPoint)
        {
            TryBuildRecognitionPoint(node);
        }
        else if (currentBuildMode == BuildMode.UpgradeRecognitionCenter)
        {
            TryUpgradeRecognitionCenter(node);
        }
        else
        {
            Debug.Log("当前模式不能点击 Node。");
        }
    }

    public void OnEdgeClicked(EdgeControler edge)
    {
        if (!CanPlayerBuildNow())
        {
            return;
        }

        if (currentBuildMode == BuildMode.BuildBond)
        {
            TryBuildBond(edge);
        }
        else
        {
            Debug.Log("当前模式不能点击 Edge。");
        }
    }

    private bool CanPlayerBuildNow()
    {
        if (gameStateMachine == null)
        {
            Debug.LogError("BuildPlacementSystem: 没有找到 GameStateMachine。");
            return false;
        }

        if (gameStateMachine.currentState != GameState.PlayerAction)
        {
            Debug.Log("当前不是玩家行动阶段，不能建造。");
            return false;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("BuildPlacementSystem: 没有找到 GameDataManager。");
            return false;
        }

        return true;
    }

    private void TryBuildBond(EdgeControler edge)
    {
        if (edge == null)
        {
            return;
        }

        if (!edge.CanBuildBond())
        {
            Debug.Log("这条 Edge 已经有纽带，不能重复建造。");
            return;
        }
        if (!edge.IsConnectedToOwnerNode(OwnerType.Player))
        {
            Debug.Log("纽带必须建在连接自己认同点或认同中心的 Edge 上。");
            return;
        }

        CharacterData player = GameDataManager.Instance.playerData;
        ResourceCost cost = BuildCosts.CreateBuildBondCost();

        if (!player.SpendResources(cost))
        {
            return;
        }

        edge.SetBond(OwnerType.Player);
        player.AddBond();

        currentBuildMode = BuildMode.None;

        Debug.Log("玩家成功建造纽带。");

        uiController.RefreshAll();
        GameDataManager.Instance.CheckGameEnd();
    }

    private void TryBuildRecognitionPoint(NodeControler node)
    {
        if (node == null)
        {
            return;
        }

        if (!node.IsEmpty())
        {
            Debug.Log("这个 Node 已经有建筑，不能建造认同点。");
            return;
        }
        if (!node.IsConnectedToOwnerBond(OwnerType.Player))
        {
            Debug.Log("认同点必须建在连接自己纽带的空 Node 上。");
            return;
        }

        CharacterData player = GameDataManager.Instance.playerData;
        ResourceCost cost = BuildCosts.CreateBuildRecognitionPointCost();

        if (!player.SpendResources(cost))
        {
            return;
        }

        node.SetRecognitionPoint(OwnerType.Player);
        player.AddRecognitionPoint();

        currentBuildMode = BuildMode.None;

        Debug.Log("玩家成功建造认同点。");

        uiController.RefreshAll();
        GameDataManager.Instance.CheckGameEnd();
    }

    private void TryUpgradeRecognitionCenter(NodeControler node)
    {
        if (node == null)
        {
            return;
        }

        if (!node.HasPlayerRecognitionPoint())
        {
            Debug.Log("只能升级自己的认同点。");
            return;
        }

        CharacterData player = GameDataManager.Instance.playerData;

        if (!player.CanUpgradeRecognitionCenter())
        {
            Debug.Log("玩家没有可升级的认同点。");
            return;
        }

        ResourceCost cost = BuildCosts.CreateUpgradeRecognitionCenterCost();

        if (!player.SpendResources(cost))
        {
            return;
        }

        node.UpgradeToRecognitionCenter();
        player.UpgradeRecognitionCenter();

        currentBuildMode = BuildMode.None;

        Debug.Log("玩家成功升级认同中心。");

        uiController.RefreshAll();
        GameDataManager.Instance.CheckGameEnd();
    }

}