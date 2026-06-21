using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    GameStart,

    PlayerTurnStart,
    PlayerRollDice,
    ResolveProduction,
    PlayerEvent,
    PlayerAction,

    NPCTurnStart,
    NPCRollDice,
    NPCProduction,
    NPCEvent,
    NPCAction,

    CheckGameEnd,
    GameOver
}

public class GameStateMachine : MonoBehaviour
{
    [Header("State")]
    public GameState currentState;
    
    [Header("UI")]
    [SerializeField] private GameUIController uiController;

    [Header("System References")]
    [SerializeField] private DiceSystem diceSystem;
    [SerializeField] private ProductionSystem productionSystem;
    [SerializeField] private BuildSystem buildSystem;
    [SerializeField] private PushAwaySystem pushAwaySystem;
    [SerializeField] private NPCActionSystem npcActionSystem;

    [Header("Dice")]
    public int lastDiceA;
    public int lastDiceB;
    public int lastDiceTotal;

    [Header("Settings")]
    [SerializeField] private float npcActionDelay = 0.8f;

    private bool playerHasRolledThisTurn;

    private void Awake()
    {
        if (uiController == null)
        {
            uiController = FindObjectOfType<GameUIController>();
        }
        if (diceSystem == null)
        {
            diceSystem = GetComponent<DiceSystem>();
        }

        if (productionSystem == null)
        {
            productionSystem = GetComponent<ProductionSystem>();
        }

        if (buildSystem == null)
        {
            buildSystem = GetComponent<BuildSystem>();
        }

        if (pushAwaySystem == null)
        {
            pushAwaySystem = GetComponent<PushAwaySystem>();
        }

        if (npcActionSystem == null)
        {
            npcActionSystem = GetComponent<NPCActionSystem>();
        }
    }

    /*
    private void Start()
    {
        StartGame();
    }*/

    private void Update()
    {
        // 临时键盘测试，之后可以换成 UI Button
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnClickRollDice();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnClickBuildBond();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnClickBuildRecognitionPoint();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnClickUpgradeRecognitionCenter();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            OnClickPushAway();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnClickEndTurn();
        }
    }

    public void StartGame()
    {
        ChangeState(GameState.GameStart);

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("没有找到 GameDataManager，无法开始游戏。");
            return;
        }

        GameDataManager.Instance.InitGameData();

        AddLog("游戏开始。");

        EnterPlayerTurnStart();
    }

    private void EnterPlayerTurnStart()
    {
        ChangeState(GameState.PlayerTurnStart);

        playerHasRolledThisTurn = false;

        AddLog("玩家回合开始。");

        ChangeState(GameState.PlayerRollDice);
        AddLog("等待玩家掷骰。按 R 掷骰。");
    }

    public void OnClickRollDice()
    {
        if (currentState != GameState.PlayerRollDice)
        {
            Debug.Log("当前不是玩家掷骰阶段，不能掷骰。");
            return;
        }

        if (playerHasRolledThisTurn)
        {
            Debug.Log("本回合已经掷过骰。");
            return;
        }

        playerHasRolledThisTurn = true;

        DiceResult result = diceSystem.RollTwoDice();

        lastDiceA = result.diceA;
        lastDiceB = result.diceB;
        lastDiceTotal = result.total;

        Debug.Log($"玩家掷骰：{lastDiceA} + {lastDiceB} = {lastDiceTotal}");

        if (lastDiceTotal == 7)
        {
            EnterPlayerEvent();
        }
        else
        {
            EnterResolveProduction();
        }
    }

    private void EnterResolveProduction()
    {
        ChangeState(GameState.ResolveProduction);

        productionSystem.ResolveProduction(lastDiceTotal);

        EnterPlayerAction();
    }

    private void EnterPlayerEvent()
    {
        ChangeState(GameState.PlayerEvent);

        Debug.Log("玩家掷出 7，本回合不进行普通资源生产。");
        Debug.Log("当前版本暂时跳过正式事件弹窗，直接进入玩家行动阶段。");

        EnterPlayerAction();
    }

    private void EnterPlayerAction()
    {
        ChangeState(GameState.PlayerAction);

        Debug.Log("进入玩家行动阶段。");
        Debug.Log("按 1 建造纽带，按 2 建造认同点，按 3 升级认同中心，按 4 推开，按 Enter 结束回合。");
    }

    public void OnClickBuildBond()
    {
        if (currentState != GameState.PlayerAction)
        {
            Debug.Log("当前不是玩家行动阶段，不能建造纽带。");
            return;
        }

        CharacterData player = GameDataManager.Instance.playerData;

        buildSystem.BuildBond(player);

        CheckGameEndAfterPlayerAction();
    }

    public void OnClickBuildRecognitionPoint()
    {
        if (currentState != GameState.PlayerAction)
        {
            Debug.Log("当前不是玩家行动阶段，不能建造认同点。");
            return;
        }

        CharacterData player = GameDataManager.Instance.playerData;

        buildSystem.BuildRecognitionPoint(player);

        CheckGameEndAfterPlayerAction();
    }

    public void OnClickUpgradeRecognitionCenter()
    {
        if (currentState != GameState.PlayerAction)
        {
            Debug.Log("当前不是玩家行动阶段，不能升级认同中心。");
            return;
        }

        CharacterData player = GameDataManager.Instance.playerData;

        buildSystem.UpgradeRecognitionCenter(player);

        CheckGameEndAfterPlayerAction();
    }

    public void OnClickPushAway()
    {
        if (currentState != GameState.PlayerAction && currentState != GameState.PlayerEvent)
        {
            Debug.Log("当前阶段不能使用【推开】。");
            return;
        }

        CharacterData player = GameDataManager.Instance.playerData;

        pushAwaySystem.UsePushAway(player, ResourceType.Food);

        CheckGameEndAfterPlayerAction();
    }

    public void OnClickEndTurn()
    {
        if (currentState != GameState.PlayerAction)
        {
            Debug.Log("当前不是玩家行动阶段，不能结束回合。");
            return;
        }

        AddLog("玩家结束回合。");

        StartCoroutine(NPCTurnRoutine());
    }

    private IEnumerator NPCTurnRoutine()
    {
        ChangeState(GameState.NPCTurnStart);
        AddLog("NPC 回合开始。");

        yield return new WaitForSeconds(npcActionDelay);

        ChangeState(GameState.NPCRollDice);

        DiceResult result = diceSystem.RollTwoDice();

        lastDiceA = result.diceA;
        lastDiceB = result.diceB;
        lastDiceTotal = result.total;

        Debug.Log($"NPC 掷骰：{lastDiceA} + {lastDiceB} = {lastDiceTotal}");

        yield return new WaitForSeconds(npcActionDelay);

        if (lastDiceTotal == 7)
        {
            EnterNPCEvent();
        }
        else
        {
            ChangeState(GameState.NPCProduction);
            productionSystem.ResolveProduction(lastDiceTotal);
        }

        yield return new WaitForSeconds(npcActionDelay);

        EnterNPCAction();

        yield return new WaitForSeconds(npcActionDelay);

        if (!CheckGameEnd())
        {
            EnterPlayerTurnStart();
        }
    }

    private void EnterNPCEvent()
    {
        ChangeState(GameState.NPCEvent);

        Debug.Log("NPC 掷出 7，本回合不进行普通资源生产。");
        Debug.Log("NPC 事件系统暂时简化处理。");

        CharacterData npc = GameDataManager.Instance.npcData;

        // 简化事件：NPC 失去 1 个随机资源
        LoseRandomResource(npc);
    }

    private void LoseRandomResource(CharacterData character)
    {
        ResourceType randomType = (ResourceType)Random.Range(0, 5);

        if (character.resources.Get(randomType) > 0)
        {
            character.resources.Add(randomType, -1);
            Debug.Log($"{character.characterName} 因事件失去 1 个 {randomType}");
        }
        else
        {
            Debug.Log($"{character.characterName} 没有可失去的 {randomType}");
        }
    }

    private void EnterNPCAction()
    {
        ChangeState(GameState.NPCAction);

        npcActionSystem.ExecuteNPCAction();
    }

    private void CheckGameEndAfterPlayerAction()
    {
        if (CheckGameEnd())
        {
            return;
        }

        ChangeState(GameState.PlayerAction);
        Debug.Log("玩家可以继续行动，或按 Enter 结束回合。");
    }

    private bool CheckGameEnd()
    {
        ChangeState(GameState.CheckGameEnd);

        bool isGameEnd = GameDataManager.Instance.CheckGameEnd();

        if (isGameEnd)
        {
            ChangeState(GameState.GameOver);
            Debug.Log("游戏结束。");
            return true;
        }

        return false;
    }

    private void ChangeState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"当前状态切换为：{currentState}");
        string message = $"当前状态切换为：{currentState}";
            Debug.Log(message);
        
            if (uiController != null)
            {
                uiController.AddLog(message);
                uiController.RefreshAll();
            }
    }
    private void AddLog(string message)
    {
        Debug.Log(message);
    
        if (uiController != null)
        {
            uiController.AddLog(message);
            uiController.RefreshAll();
        }
    }
}
