using UnityEngine;

public enum EventCardPoolType
{
    Onboarding,
    Global
}

public enum EventTargetType
{
    CurrentCharacter,
    PlayerOnly,
    NPCOnly,
    BothCharacters
}

public enum EventEffectType
{
    GainResource,
    LoseResource,
    GainFocusCard,
    LoseMultipleResources,
    GainMultipleResources
}

[CreateAssetMenu(fileName = "NewEventCard", menuName = "XXIsland/Event Card")]
public class EventCardSO : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;
    [TextArea(2, 5)] public string description;

    [Header("Pool")]
    public EventCardPoolType poolType;

    [Header("Target")]
    public EventTargetType targetType;

    [Header("Effect")]
    public EventEffectType effectType;

    [Header("Single Resource Effect")]
    public ResourceType resourceType;
    public int amount;

    [Header("Multiple Resource Effect")]
    public ResourceAmount[] resourceAmounts;

    [Header("Identity Requirement Optional")]
    public bool requireIdentity;
    public IdentityType requiredIdentity;

    [Header("Fallback Effect")]
    public bool useFallbackIfIdentityNotMatch;
    public ResourceType fallbackResourceType;
    public int fallbackAmount;
}