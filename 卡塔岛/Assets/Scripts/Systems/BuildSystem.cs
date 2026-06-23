using UnityEngine;

public class BuildSystem : MonoBehaviour
{
    public bool BuildBond(CharacterData character)
    {
        ResourceCost cost = BuildCosts.CreateBuildBondCost();

        if (!character.SpendResources(cost))
        {
            return false;
        }

        character.AddBond();
        return true;
    }

    public bool BuildRecognitionPoint(CharacterData character)
    {
        ResourceCost cost = BuildCosts.CreateBuildRecognitionPointCost();

        if (!character.SpendResources(cost))
        {
            return false;
        }

        character.AddRecognitionPoint();
        return true;
    }

    public bool UpgradeRecognitionCenter(CharacterData character)
    {
        if (!character.CanUpgradeRecognitionCenter())
        {
            Debug.Log($"{character.characterName} 没有认同点，无法升级认同中心。");
            return false;
        }

        ResourceCost cost = BuildCosts.CreateUpgradeRecognitionCenterCost();

        if (!character.SpendResources(cost))
        {
            return false;
        }

        character.UpgradeRecognitionCenter();
        return true;
    }

    public bool CanBuildBond(CharacterData character)
    {
        ResourceCost cost = BuildCosts.CreateBuildBondCost();
        return character.CanAfford(cost);
    }

    public bool CanBuildRecognitionPoint(CharacterData character)
    {
        ResourceCost cost = BuildCosts.CreateBuildRecognitionPointCost();
        return character.CanAfford(cost);
    }

    public bool CanUpgradeRecognitionCenter(CharacterData character)
    {
        ResourceCost cost = BuildCosts.CreateUpgradeRecognitionCenterCost();

        return character.CanUpgradeRecognitionCenter() &&
               character.CanAfford(cost);
    }
}