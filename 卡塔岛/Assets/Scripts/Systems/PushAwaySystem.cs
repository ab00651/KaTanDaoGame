using UnityEngine;

public class PushAwaySystem : MonoBehaviour
{
    public bool UsePushAway(CharacterData character, ResourceType rewardType)
    {
        if (character.bondCount <= character.brokenBondCount)
        {
            Debug.Log($"{character.characterName} 没有可承受裂痕的纽带，无法使用【推开】。");
            return false;
        }

        character.AddResource(rewardType, 1);
        character.AddCrack();

        Debug.Log($"{character.characterName} 使用【推开】：获得 1 个 {rewardType}，一条纽带出现裂痕。");

        return true;
    }
}