using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceStorage
{
    [Header("Resources")]
    public int food;
    public int housing;
    public int medical;
    public int legal;
    public int credit;

    public int Get(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Food:
                return food;

            case ResourceType.Housing:
                return housing;

            case ResourceType.Medical:
                return medical;

            case ResourceType.Legal:
                return legal;

            case ResourceType.Credit:
                return credit;

            default:
                return 0;
        }
    }

    public void Add(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Food:
                food += amount;
                break;

            case ResourceType.Housing:
                housing += amount;
                break;

            case ResourceType.Medical:
                medical += amount;
                break;

            case ResourceType.Legal:
                legal += amount;
                break;

            case ResourceType.Credit:
                credit += amount;
                break;
        }

        ClampToZero();
    }

    public bool CanAfford(ResourceCost cost)
    {
        foreach (ResourceAmount item in cost.items)
        {
            if (Get(item.type) < item.amount)
            {
                return false;
            }
        }

        return true;
    }

    public bool Spend(ResourceCost cost)
    {
        if (!CanAfford(cost))
        {
            return false;
        }

        foreach (ResourceAmount item in cost.items)
        {
            Add(item.type, -item.amount);
        }

        return true;
    }

    public void SetDefaultCoreLoopResources()
    {
        food = 2;
        housing = 2;
        medical = 1;
        legal = 1;
        credit = 1;
    }

    private void ClampToZero()
    {
        food = Mathf.Max(0, food);
        housing = Mathf.Max(0, housing);
        medical = Mathf.Max(0, medical);
        legal = Mathf.Max(0, legal);
        credit = Mathf.Max(0, credit);
    }

    public string GetDebugText()
    {
        return $"食物:{food}, 住房:{housing}, 医疗:{medical}, 法律:{legal}, 信用:{credit}";
    }
}
