using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum IdentityType
{
    TideSpeaker,   // 潮语者
    CodeCitizen,   // 代码民
    CoralDescendant // 珊瑚裔
}

[System.Serializable]
public class CharacterData 
{
    [Header("Basic Info")]
    public CharacterType characterType;
    public string characterName;

        
    [Header("Identity")]
    public IdentityType identityType;
    
    [Header("Resources")]
    public ResourceStorage resources = new ResourceStorage();

    [Header("Score")]
    public int recognitionScore;

    [Header("Buildings")]
    public int bondCount;
    public int recognitionPointCount;
    public int recognitionCenterCount;

    [Header("Cracks")]
    public int totalCrackCount;
    public int brokenBondCount;
    
    [Header("Cards")]
    public int focusCardCount;

    
    public void InitAsCoreLoopDefault(CharacterType type)
    {
        characterType = type;

        if (type == CharacterType.Player)
        {
            characterName = "Player";
        }
        else
        {
            characterName = "NPC";
        }

        identityType = GetRandomIdentity();
        
        resources.SetDefaultCoreLoopResources();

        bondCount = 0;
        recognitionPointCount = 0;
        recognitionCenterCount = 0;

        recognitionScore = 0;

        totalCrackCount = 0;
        brokenBondCount = 0;
        
        focusCardCount = 0;
    }

    public void AddResource(ResourceType type, int amount)
    {
        resources.Add(type, amount);
        Debug.Log($"{characterName} 获得 {amount} 个 {type}");
    }

    public bool CanAfford(ResourceCost cost)
    {
        return resources.CanAfford(cost);
    }

    public bool SpendResources(ResourceCost cost)
    {
        bool success = resources.Spend(cost);

        if (!success)
        {
            Debug.Log($"{characterName} 资源不足，无法建设。");
        }

        return success;
    }

    public void AddBond()
    {
        bondCount++;
        Debug.Log($"{characterName} 建造纽带，当前纽带数量：{bondCount}");
    }

    public void AddRecognitionPoint()
    {
        recognitionPointCount++;
        recognitionScore++;

        Debug.Log($"{characterName} 建造认同点，认同度 +1，当前认同度：{recognitionScore}");
    }

    public bool CanUpgradeRecognitionCenter()
    {
        return recognitionPointCount >= 1;
    }

    public void UpgradeRecognitionCenter()
    {
        if (!CanUpgradeRecognitionCenter())
        {
            Debug.Log($"{characterName} 没有认同点，无法升级认同中心。");
            return;
        }

        recognitionPointCount--;
        recognitionCenterCount++;

        //认同点本来提供 1 分，认同中心总共提供 2 分，所以升级时额外 +1 分
        recognitionScore++;

        Debug.Log($"{characterName} 升级认同中心，认同度 +1，当前认同度：{recognitionScore}");
    }

    public void AddCrack()
    {
        if (bondCount <= brokenBondCount)
        {
            Debug.Log($"{characterName} 没有可承受裂痕的纽带。");
            return;
        }

        totalCrackCount++;

        // 核心循环简化版：每 2 个裂痕导致 1 条纽带断裂
        int newBrokenBondCount = totalCrackCount / 2;

        if (newBrokenBondCount > brokenBondCount)
        {
            brokenBondCount = Mathf.Min(newBrokenBondCount, bondCount);
            Debug.Log($"{characterName} 的一条纽带断裂。当前断裂纽带数量：{brokenBondCount}");
        }
        else
        {
            Debug.Log($"{characterName} 的一条纽带出现裂痕。当前裂痕数量：{totalCrackCount}");
        }
    }

    public bool HasReachedWinScore(int targetScore)
    {
        return recognitionScore >= targetScore;
    }

    public string GetDebugText()
    {
        return
            $"{characterName}\n" +
            $"资源：{resources.GetDebugText()}\n" +
            $"认同度：{recognitionScore}\n" +
            $"纽带：{bondCount}\n" +
            $"认同点：{recognitionPointCount}\n" +
            $"认同中心：{recognitionCenterCount}\n" +
            $"裂痕：{totalCrackCount}\n" +
            $"断裂纽带：{brokenBondCount}";
    }
    
    private IdentityType GetRandomIdentity()
    {
        int randomIndex = Random.Range(0, 3);
    
        return (IdentityType)randomIndex;
    }
    
    public void AddFocusCard(int amount)
    {
        focusCardCount += amount;
        Debug.Log($"{characterName} 获得 {amount} 张焦点卡，当前焦点卡数量：{focusCardCount}");
    }
}
