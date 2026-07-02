using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    GameStart,
    SetupPlacement,

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
    [SerializeField] private SetupPlacementSystem setupPlacementSystem;
    [SerializeField] private EventSystem eventSystem;

    [Header("Dice")]
    public int lastDiceA;
    public int lastDiceB;
    public int lastDiceTotal;

    [Header("Round Tracking")]
    [SerializeField] private int fullRoundNumber = 1;
    [SerializeField] private bool onboardingEventTriggered = false;

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

        if (setupPlacementSystem == null)
        {
            setupPlacementSystem = GetComponent<SetupPlacementSystem>();
        }

        if (setupPlacementSystem == null)
        {
            setupPlacementSystem = FindObjectOfType<SetupPlacementSystem>();
        }

        if (eventSystem == null)
        {
            eventSystem = GetComponent<EventSystem>();
        }

        if (eventSystem == null)
        {
            eventSystem = FindObjectOfType<EventSystem>();
        }
    }

    /*
    private void Start()
    {
        StartGame();
    }
    */

    public void StartGame()
    {
        ChangeState(GameState.GameStart);

        GameDataManager.Instance.InitGameData();

        fullRoundNumber = 1;
        onboardingEventTriggered = false;
        playerHasRolledThisTurn = false;

        AddLog("游戏开始。");

        if (setupPlacementSystem == null)
        {
            setupPlacementSystem = FindObjectOfType<SetupPlacementSystem>();
        }

        if (setupPlacementSystem == null)
        {
            Debug.LogError("没有找到 SetupPlacementSystem，无法进入初始放置阶段。");
            return;
        }

        ChangeState(GameState.SetupPlacement);

        setupPlacementSystem.StartSetupPlacement();
    }

    public void EnterPlayerTurnStartFromSetup()
    {
        EnterPlayerTurnStart();
    }

    private void EnterPlayerTurnStart()
    {
        ChangeState(GameState.PlayerTurnStart);

        playerHasRolledThisTurn = false;

        AddLog($"第 {fullRoundNumber} 轮：玩家回合开始。");

        // 第 3 轮保底触发一次入岛事件
        if (fullRoundNumber == 3 && !onboardingEventTriggered)
        {
            AddLog("前两轮没有触发入岛事件，第 3 轮开始自动触发一次入岛事件。");

            if (eventSystem != null)
            {
                eventSystem.DrawAndResolveOnboardingEvent(GameDataManager.Instance.playerData);
                onboardingEventTriggered = true;
            }
            else
            {
                Debug.LogError("没有找到 EventSystem，无法触发入岛事件。");
            }
        }

        ChangeState(GameState.PlayerRollDice);
        AddLog("等待玩家掷骰。");
    }

    public void OnClickRollDice()
    {
        if (currentState != GameState.PlayerRollDice)
        {
            AddLog("当前不是玩家掷骰阶段，不能掷骰。");
            return;
        }

        if (playerHasRolledThisTurn)
        {
            AddLog("本回合已经掷过骰。");
            return;
        }

        playerHasRolledThisTurn = true;

        DiceResult result = diceSystem.RollTwoDice();

        lastDiceA = result.diceA;
        lastDiceB = result.diceB;
        lastDiceTotal = result.total;

        AddLog($"玩家掷骰：{lastDiceA} + {lastDiceB} = {lastDiceTotal}");

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

        RefreshUI();

        EnterPlayerAction();
    }

    private void EnterPlayerEvent()
    {
        ChangeState(GameState.PlayerEvent);

        AddLog("玩家掷出 7，本回合不进行普通资源生产。");

        CharacterData player = GameDataManager.Instance.playerData;

        ResolveEventForCharacter(player);

        EnterPlayerAction();
    }

    private void ResolveEventForCharacter(CharacterData character)
    {
        if (eventSystem == null)
        {
            Debug.LogError("没有找到 EventSystem，无法处理事件。");
            return;
        }

        if (ShouldUseOnboardingEvent())
        {
            AddLog("触发入岛事件池。");
            eventSystem.DrawAndResolveOnboardingEvent(character);
            onboardingEventTriggered = true;
        }
        else
        {
            AddLog("触发普通全局事件池。");
            eventSystem.DrawAndResolveGlobalEvent(character);
        }
    }

    private bool ShouldUseOnboardingEvent()
    {
        return fullRoundNumber <= 2 && !onboardingEventTriggered;
    }

    private void EnterPlayerAction()
    {
        ChangeState(GameState.PlayerAction);

        AddLog("进入玩家行动阶段。");
    }

    public void OnClickBuildBond()
    {
        if (currentState != GameState.PlayerAction)
        {
            AddLog("当前不是玩家行动阶段，不能建造纽带。");
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
            AddLog("当前不是玩家行动阶段，不能建造认同点。");
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
            AddLog("当前不是玩家行动阶段，不能升级认同中心。");
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
            AddLog("当前阶段不能使用【推开】。");
            return;
        }

        if (pushAwaySystem != null)
        {
            pushAwaySystem.SelectPushAwayBondMode();
        }
    }

    public void OnClickEndTurn()
    {
        if (currentState != GameState.PlayerAction)
        {
            AddLog("当前不是玩家行动阶段，不能结束回合。");
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

        AddLog($"NPC 掷骰：{lastDiceA} + {lastDiceB} = {lastDiceTotal}");

        yield return new WaitForSeconds(npcActionDelay);

        if (lastDiceTotal == 7)
        {
            EnterNPCEvent();
        }
        else
        {
            ChangeState(GameState.NPCProduction);
            productionSystem.ResolveProduction(lastDiceTotal);
            RefreshUI();
        }

        yield return new WaitForSeconds(npcActionDelay);

        EnterNPCAction();

        yield return new WaitForSeconds(npcActionDelay);

        if (!CheckGameEnd())
        {
            fullRoundNumber++;
            EnterPlayerTurnStart();
        }
    }

    private void EnterNPCEvent()
    {
        ChangeState(GameState.NPCEvent);

        AddLog("NPC 掷出 7，本回合不进行普通资源生产。");

        CharacterData npc = GameDataManager.Instance.npcData;

        ResolveEventForCharacter(npc);

        RefreshUI();
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
        AddLog("玩家可以继续行动，或结束回合。");
    }

    private bool CheckGameEnd()
    {
        ChangeState(GameState.CheckGameEnd);

        bool isGameEnd = GameDataManager.Instance.CheckGameEnd();

        if (isGameEnd)
        {
            ChangeState(GameState.GameOver);
            AddLog("游戏结束。");
            return true;
        }

        return false;
    }

    private void ChangeState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"当前状态切换为：{currentState}");

        RefreshUI();
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

    private void RefreshUI()
    {
        if (uiController != null)
        {
            uiController.RefreshAll();
        }
    }
}