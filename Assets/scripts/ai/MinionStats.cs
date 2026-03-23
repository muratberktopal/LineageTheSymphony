using System.Collections.Generic;
using UnityEngine;

// ============================================================
// MinionStats.cs
// Her minyonun tüm veri alanları — hiçbir logic yok burada
// ============================================================
[System.Serializable]
public class MinionStats
{
    [Header("Kimlik")]
    public string name;
    public bool isFemale;
    public int generation;
    public float age;
    public float maxAge;
    public string mutationType = "None";

    [Header("Yaşam")]
    public bool isAlive = true;
    public float health = 100f;
    public bool isInjured = false;
    public bool isSick = false;
    public bool isInMourning = false;

    [Header("Sosyal")]
    public bool isMarried = false;
    public MinionAI spouse = null;
    public MinionAI secretLover = null;
    public bool hasSecretRelationship = false;
    public List<MinionAI> children  = new List<MinionAI>();
    public List<MinionAI> friends   = new List<MinionAI>();
    public List<MinionAI> enemies   = new List<MinionAI>();
    public bool isLeader = false;
    public float reputation = 50f;

    [Header("Davranış")]
    public float workSpeed = 1f;
    public float sociability = 50f;
    public float trustLevel  = 50f;
    public float hoardingTendency = 0f;
    public float fearThreshold    = 50f;
    public float learningRate     = 1f;
    public float randomBehaviorChance = 0.05f;

    [Header("Flags")]
    public bool consideringSabotage = false;
    public bool wasWronged          = false;
    public bool hasBeenAttacked     = false;
    public bool hasBeenHumiliated   = false;
    public bool hasLostFamily       = false;
    public bool caughtStealing      = false;

    [Header("Üreme")]
    public float lastMatingTime  = -999f;
    public float matingCooldown  = 180f;

    [Header("İnşaat / Sosyal sayaçlar")]
    public int   buildingsConstructed = 0;
    public int   recentSuccesses      = 0;
    public float baseGriefLevel       = 0f;

    [Header("Ebeveyn")]
    public MinionAI parent1 = null;
    public MinionAI parent2 = null;

    public void Initialize(int gen, bool female, string minionName)
    {
        generation = gen;
        isFemale   = female;
        name       = minionName;
        isAlive    = true;
        health     = 100f;
        maxAge     = Random.Range(20f, 40f);
        reputation = 50f;
        trustLevel = 50f;
        sociability = Random.Range(30f, 70f);
        workSpeed   = 1f;
        learningRate = 1f;
    }
}
