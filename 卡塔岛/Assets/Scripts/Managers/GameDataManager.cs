using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("Characters")]
    public CharacterData playerData = new CharacterData();
    public CharacterData npcData = new CharacterData();

    [Header("Game Settings")]
    public int targetRecognitionScore = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitGameData();
    }

    public void InitGameData()
    {
        playerData.InitAsCoreLoopDefault(CharacterType.Player);
        npcData.InitAsCoreLoopDefault(CharacterType.NPC);

        Debug.Log("游戏数据初始化完成。");
        Debug.Log(playerData.GetDebugText());
        Debug.Log(npcData.GetDebugText());
    }

    public bool CheckGameEnd()
    {
        if (playerData.HasReachedWinScore(targetRecognitionScore))
        {
            Debug.Log("玩家达到 10 点认同度，玩家胜利。");
            return true;
        }

        if (npcData.HasReachedWinScore(targetRecognitionScore))
        {
            Debug.Log("NPC 达到 10 点认同度，NPC 胜利。");
            return true;
        }

        return false;
    }
}