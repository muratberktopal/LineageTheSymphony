using System.Collections.Generic;
using UnityEngine;

// ============================================================
// RandomEventData.cs
// Tüm encounter'lerin veri yapıları
// ============================================================

[System.Serializable]
public class EventCondition
{
    public enum ConditionType
    {
        HasCard, NeedAbove, NeedBelow, EmotionAbove, EmotionBelow,
        RelationshipBelow, RelationshipAbove, ThresholdMet,
        IsMarried, IsLeader, HasInvention, ReputationBelow, ReputationAbove,
        GenerationAbove, FriendCountAbove, EnemyCountAbove,
        IsNight, IsWinter, HasShelter, IsInjured, IsSick,
        SpouseCheating, NearbyFightExists, ResourceAbove, ResourceBelow,
        AgeAbove, HoardingAbove, TrustBelow, IsInMourning, FactionExists,
        PopulationAbove, PopulationBelow
    }
    public ConditionType conditionType;
    public string        parameter;
    public float         value;
}

[System.Serializable]
public class EventOutcome
{
    public enum OutcomeType
    {
        SetEmotion, ChangeNeed, ChangeRelationship, ChangeReputation,
        AttackMinion, KillMinion, InventItem, TriggerCascade,
        ShowNotification, SpawnItem, BuildBuilding, FormFaction,
        ChangeStat, StartFight, Flee, StealFrom, GiveItem,
        BecomeFriends, BecomeEnemies, Marry, Divorce, HealMinion, MakeLeader
    }
    public OutcomeType      outcomeType;
    public string           parameter;
    public float            value;
    public string           notificationMessage;
    public NotificationType notificationType;
}

[System.Serializable]
public class RandomEventData
{
    public string           eventId;
    public string           description;
    public string           category;
    public int              priority;       // 1-5, öncelik sistemi için
    public float            triggerChance;  // 0-1
    public float            cooldown;       // saniye, 0=bir kez
    public bool             isUnique;       // bir kez tetiklenir mi
    public List<EventCondition> conditions  = new List<EventCondition>();
    public List<EventOutcome>   outcomes    = new List<EventOutcome>();
}

public enum NotificationType
{
    Social, Drama, Conflict, Surprising, Major, Death,
    Evolution, Religion, Nature, Comedy
}
