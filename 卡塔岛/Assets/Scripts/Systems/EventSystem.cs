using System.Collections.Generic;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    [Header("Event Cards")]
    [SerializeField] private List<EventCardSO> allEventCards = new List<EventCardSO>();

    [Header("References")]
    [SerializeField] private GameUIController uiController;

    private List<EventCardSO> onboardingEventPool = new List<EventCardSO>();
    private List<EventCardSO> globalEventPool = new List<EventCardSO>();

    private void Awake()
    {
        if (uiController == null)
        {
            uiController = FindObjectOfType<GameUIController>();
        }

        BuildEventPools();
    }

    private void BuildEventPools()
    {
        onboardingEventPool.Clear();
        globalEventPool.Clear();

        foreach (EventCardSO card in allEventCards)
        {
            if (card == null)
            {
                continue;
            }

            if (card.poolType == EventCardPoolType.Onboarding)
            {
                onboardingEventPool.Add(card);
            }
            else if (card.poolType == EventCardPoolType.Global)
            {
                globalEventPool.Add(card);
            }
        }

        Debug.Log($"入岛事件池数量：{onboardingEventPool.Count}");
        Debug.Log($"普通全局事件池数量：{globalEventPool.Count}");
    }

    public void DrawAndResolveOnboardingEvent(CharacterData currentCharacter)
    {
        EventCardSO card = DrawRandomCard(onboardingEventPool);

        if (card == null)
        {
            AddLog("入岛事件池为空，无法抽取事件。");
            return;
        }

        ResolveEvent(card, currentCharacter);
    }

    public void DrawAndResolveGlobalEvent(CharacterData currentCharacter)
    {
        EventCardSO card = DrawRandomCard(globalEventPool);

        if (card == null)
        {
            AddLog("普通全局事件池为空，无法抽取事件。");
            return;
        }

        ResolveEvent(card, currentCharacter);
    }

    private EventCardSO DrawRandomCard(List<EventCardSO> pool)
    {
        if (pool == null || pool.Count == 0)
        {
            return null;
        }

        int index = Random.Range(0, pool.Count);
        return pool[index];
    }

    private void ResolveEvent(EventCardSO card, CharacterData currentCharacter)
    {
        AddLog("====================");
        AddLog($"触发事件卡：{card.cardName}");
        AddLog($"事件描述：{card.description}");
        AddLog($"触发角色：{currentCharacter.characterName}");
        AddLog("====================");

        List<CharacterData> targets = GetTargets(card, currentCharacter);

        foreach (CharacterData target in targets)
        {
            ResolveEventForTarget(card, target);
        }

        RefreshUI();
    }

    private List<CharacterData> GetTargets(EventCardSO card, CharacterData currentCharacter)
    {
        List<CharacterData> targets = new List<CharacterData>();

        if (card.targetType == EventTargetType.CurrentCharacter)
        {
            targets.Add(currentCharacter);
        }
        else if (card.targetType == EventTargetType.PlayerOnly)
        {
            targets.Add(GameDataManager.Instance.playerData);
        }
        else if (card.targetType == EventTargetType.NPCOnly)
        {
            targets.Add(GameDataManager.Instance.npcData);
        }
        else if (card.targetType == EventTargetType.BothCharacters)
        {
            targets.Add(GameDataManager.Instance.playerData);
            targets.Add(GameDataManager.Instance.npcData);
        }

        return targets;
    }

    private void ResolveEventForTarget(EventCardSO card, CharacterData target)
    {
        if (target == null)
        {
            return;
        }

        bool identityMatched = true;

        if (card.requireIdentity)
        {
            identityMatched = target.identityType == card.requiredIdentity;
        }

        if (!identityMatched)
        {
            if (card.useFallbackIfIdentityNotMatch)
            {
                ResolveFallbackEffect(card, target);
            }
            else
            {
                AddLog($"{target.characterName} 身份不符合，事件无效果。");
            }

            return;
        }

        ResolveMainEffect(card, target);
    }

    private void ResolveMainEffect(EventCardSO card, CharacterData target)
    {
        switch (card.effectType)
        {
            case EventEffectType.GainResource:
                target.AddResource(card.resourceType, card.amount);
                AddLog($"{target.characterName} 获得 {card.amount} 个 {card.resourceType}");
                break;

            case EventEffectType.LoseResource:
                TryLoseResource(target, card.resourceType, card.amount);
                break;

            case EventEffectType.GainFocusCard:
                target.AddFocusCard(card.amount);
                AddLog($"{target.characterName} 获得 {card.amount} 张焦点卡。");
                break;

            case EventEffectType.GainMultipleResources:
                GainMultipleResources(target, card.resourceAmounts);
                break;

            case EventEffectType.LoseMultipleResources:
                LoseMultipleResources(target, card.resourceAmounts);
                break;
        }
    }

    private void ResolveFallbackEffect(EventCardSO card, CharacterData target)
    {
        if (card.fallbackAmount <= 0)
        {
            AddLog($"{target.characterName} 身份不匹配，没有备用效果。");
            return;
        }

        if (card.effectType == EventEffectType.LoseResource ||
            card.effectType == EventEffectType.LoseMultipleResources)
        {
            TryLoseResource(target, card.fallbackResourceType, card.fallbackAmount);
            AddLog($"{target.characterName} 身份不匹配，受到弱化惩罚。");
        }
        else
        {
            target.AddResource(card.fallbackResourceType, card.fallbackAmount);
            AddLog($"{target.characterName} 身份不匹配，获得弱化效果：{card.fallbackAmount} 个 {card.fallbackResourceType}");
        }
    }

    private void GainMultipleResources(CharacterData target, ResourceAmount[] amounts)
    {
        if (amounts == null)
        {
            return;
        }

        foreach (ResourceAmount item in amounts)
        {
            target.AddResource(item.type, item.amount);
            AddLog($"{target.characterName} 获得 {item.amount} 个 {item.type}");
        }
    }

    private void LoseMultipleResources(CharacterData target, ResourceAmount[] amounts)
    {
        if (amounts == null)
        {
            return;
        }

        foreach (ResourceAmount item in amounts)
        {
            TryLoseResource(target, item.type, item.amount);
        }
    }

    private void TryLoseResource(CharacterData character, ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            AddLog($"{character.characterName} 不需要失去 {type}。");
            return;
        }

        int currentAmount = character.resources.Get(type);

        if (currentAmount <= 0)
        {
            AddLog($"{character.characterName} 没有 {type}，无法支付。");
            return;
        }

        int loseAmount = Mathf.Min(currentAmount, amount);
        character.resources.Add(type, -loseAmount);

        AddLog($"{character.characterName} 失去 {loseAmount} 个 {type}");
    }

    private void AddLog(string message)
    {
        Debug.Log(message);

        if (uiController != null)
        {
            uiController.AddLog(message);
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