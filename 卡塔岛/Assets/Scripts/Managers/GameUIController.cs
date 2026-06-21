using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [Header("State Text")]
    [SerializeField] private TextMeshProUGUI currentStateText;
    [SerializeField] private TextMeshProUGUI currentTurnText;
    [SerializeField] private TextMeshProUGUI diceResultText;
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Data Text")]
    [SerializeField] private TextMeshProUGUI playerDataText;
    [SerializeField] private TextMeshProUGUI npcDataText;

    [Header("Log Text")]
    [SerializeField] private TextMeshProUGUI logText;

    [Header("Buttons")]
    [SerializeField] private Button rollDiceButton;
    [SerializeField] private Button buildBondButton;
    [SerializeField] private Button buildRecognitionPointButton;
    [SerializeField] private Button upgradeRecognitionCenterButton;
    [SerializeField] private Button pushAwayButton;
    [SerializeField] private Button endTurnButton;

    [Header("References")]
    [SerializeField] private GameStateMachine gameStateMachine;

    private void Awake()
    {
        if (gameStateMachine == null)
        {
            gameStateMachine = FindObjectOfType<GameStateMachine>();
        }
    }

    private void Start()
    {
        BindButtons();
        RefreshAll();
        AddLog("UI 初始化完成。");
    }

    private void BindButtons()
    {
        if (gameStateMachine == null)
        {
            Debug.LogError("GameUIController: 没有找到 GameStateMachine。");
            return;
        }

        rollDiceButton.onClick.AddListener(gameStateMachine.OnClickRollDice);
        buildBondButton.onClick.AddListener(gameStateMachine.OnClickBuildBond);
        buildRecognitionPointButton.onClick.AddListener(gameStateMachine.OnClickBuildRecognitionPoint);
        upgradeRecognitionCenterButton.onClick.AddListener(gameStateMachine.OnClickUpgradeRecognitionCenter);
        pushAwayButton.onClick.AddListener(gameStateMachine.OnClickPushAway);
        endTurnButton.onClick.AddListener(gameStateMachine.OnClickEndTurn);
    }

    public void RefreshAll()
    {
        RefreshStateText();
        RefreshDiceText();
        RefreshCharacterData();
        RefreshButtons();
    }

    public void RefreshStateText()
    {
        if (gameStateMachine == null)
        {
            return;
        }

        currentStateText.text = $"当前状态：{gameStateMachine.currentState}";
        currentTurnText.text = $"当前回合：{GetTurnText(gameStateMachine.currentState)}";
        hintText.text = GetHintText(gameStateMachine.currentState);
    }

    public void RefreshDiceText()
    {
        if (gameStateMachine == null)
        {
            return;
        }

        diceResultText.text =
            $"骰子结果：{gameStateMachine.lastDiceA} + {gameStateMachine.lastDiceB} = {gameStateMachine.lastDiceTotal}";
    }

    public void RefreshCharacterData()
    {
        if (GameDataManager.Instance == null)
        {
            return;
        }

        CharacterData player = GameDataManager.Instance.playerData;
        CharacterData npc = GameDataManager.Instance.npcData;

        playerDataText.text = GetCharacterText("玩家", player);
        npcDataText.text = GetCharacterText("NPC", npc);
    }

    private string GetCharacterText(string title, CharacterData data)
    {
        return
            $"{title}\n" +
            $"食物：{data.resources.food}\n" +
            $"住房：{data.resources.housing}\n" +
            $"医疗：{data.resources.medical}\n" +
            $"法律：{data.resources.legal}\n" +
            $"信用：{data.resources.credit}\n" +
            $"认同度：{data.recognitionScore}\n" +
            $"纽带：{data.bondCount}\n" +
            $"认同点：{data.recognitionPointCount}\n" +
            $"认同中心：{data.recognitionCenterCount}\n" +
            $"裂痕：{data.totalCrackCount}\n" +
            $"断裂纽带：{data.brokenBondCount}";
    }

    public void RefreshButtons()
    {
        if (gameStateMachine == null)
        {
            return;
        }

        GameState state = gameStateMachine.currentState;

        bool isRollDiceState = state == GameState.PlayerRollDice;
        bool isPlayerActionState = state == GameState.PlayerAction;
        bool isPlayerEventState = state == GameState.PlayerEvent;

        rollDiceButton.interactable = isRollDiceState;

        buildBondButton.interactable = isPlayerActionState;
        buildRecognitionPointButton.interactable = isPlayerActionState;
        upgradeRecognitionCenterButton.interactable = isPlayerActionState;

        pushAwayButton.interactable = isPlayerActionState || isPlayerEventState;

        endTurnButton.interactable = isPlayerActionState;

        if (state == GameState.GameOver)
        {
            rollDiceButton.interactable = false;
            buildBondButton.interactable = false;
            buildRecognitionPointButton.interactable = false;
            upgradeRecognitionCenterButton.interactable = false;
            pushAwayButton.interactable = false;
            endTurnButton.interactable = false;
        }
    }

    public void AddLog(string message)
    {
        if (logText == null)
        {
            return;
        }

        logText.text += message + "\n";
    }

    private string GetTurnText(GameState state)
    {
        switch (state)
        {
            case GameState.PlayerTurnStart:
            case GameState.PlayerRollDice:
            case GameState.ResolveProduction:
            case GameState.PlayerEvent:
            case GameState.PlayerAction:
                return "玩家";

            case GameState.NPCTurnStart:
            case GameState.NPCRollDice:
            case GameState.NPCProduction:
            case GameState.NPCEvent:
            case GameState.NPCAction:
                return "NPC";

            case GameState.GameOver:
                return "游戏结束";

            default:
                return "无";
        }
    }

    private string GetHintText(GameState state)
    {
        switch (state)
        {
            case GameState.GameStart:
                return "游戏开始，正在初始化。";

            case GameState.PlayerRollDice:
                return "请点击【掷骰】。";

            case GameState.PlayerAction:
                return "玩家行动阶段：可以建造、推开或结束回合。";

            case GameState.PlayerEvent:
                return "事件阶段：可以使用【推开】。";

            case GameState.NPCTurnStart:
            case GameState.NPCRollDice:
            case GameState.NPCProduction:
            case GameState.NPCEvent:
            case GameState.NPCAction:
                return "NPC 正在行动，请等待。";

            case GameState.GameOver:
                return "游戏结束。";

            default:
                return "";
        }
    }
}