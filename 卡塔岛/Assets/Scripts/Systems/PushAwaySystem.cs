using UnityEngine;

public class PushAwaySystem : MonoBehaviour
{

    public bool IsSelectingBond { get; private set; }
    
    [SerializeField] private ResourceType rewardType = ResourceType.Food;
    
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

    public void SelectPushAwayBondMode()
    {
        if (!CanUsePushAwayNow())
        {
            return;
        }

        IsSelectingBond = true;

        AddLog("进入【推开】模式：请选择一条自己的纽带。");
    }

    public void CancelPushAwayMode()
    {
        IsSelectingBond = false;
        AddLog("取消【推开】模式。");
    }

    public void OnEdgeClicked(EdgeControler edge)
    {
        if (!IsSelectingBond)
        {
            return;
        }

        TryPushAwayOnBond(edge);
    }

    private bool CanUsePushAwayNow()
    {
        if (gameStateMachine == null)
        {
            Debug.LogError("PushAwaySystem: 没有找到 GameStateMachine。");
            return false;
        }

        if (gameStateMachine.currentState != GameState.PlayerAction &&
            gameStateMachine.currentState != GameState.PlayerEvent)
        {
            AddLog("当前阶段不能使用【推开】。");
            return false;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("PushAwaySystem: 没有找到 GameDataManager。");
            return false;
        }

        return true;
    }

    private void TryPushAwayOnBond(EdgeControler edge)
    {
        if (edge == null)
        {
            return;
        }

        if (!edge.HasBondOwnedBy(OwnerType.Player))
        {
            AddLog("只能选择自己已经建好的纽带。");
            return;
        }

        CharacterData player = GameDataManager.Instance.playerData;

        player.AddResource(rewardType, 1);

        edge.RemoveBond();

        player.bondCount = Mathf.Max(0, player.bondCount - 1);
        player.totalCrackCount++;
        player.brokenBondCount++;

        IsSelectingBond = false;

        AddLog($"玩家使用【推开】：获得 1 个 {rewardType}，并破坏了一条纽带。");

        if (uiController != null)
        {
            uiController.RefreshAll();
        }

        GameDataManager.Instance.CheckGameEnd();
    }

    private void AddLog(string message)
    {
        Debug.Log(message);

        if (uiController != null)
        {
            uiController.RefreshAll();
        }
    }
}