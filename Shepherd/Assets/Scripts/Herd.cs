using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Herd : MonoBehaviour
{
    // Your list of Sheep instances
    public List<Sheep> sheep = new List<Sheep>();

    void Awake()
    {
        sheep = GetComponentsInChildren<Sheep>().ToList();
    }

    // Accessor arrays (for HUDs & Battle logic)
    public string[] names => sheep.Select(s => s.name).ToArray();
    public int[] maxHP => sheep.Select(s => s.maxHP).ToArray();
    public int[] currentHP => sheep.Select(s => s.currentHP).ToArray();
    public int[] levels => sheep.Select(s => s.level).ToArray();
    public int[] damage => sheep.Select(s => GetAttackDamage(s)).ToArray();

    public int GetAttackDamage(Sheep s)
    {
        int baseDamage = s.strength * 2;
        int variance = Random.Range(-2, 3);
        return Mathf.Max(1, baseDamage + variance);
    }

    public void TakeDamage(int index, int damage)
    {
        Sheep s = sheep[index];

        if (s.defending)
        {
            s.defending = false;
            Debug.Log($"{s.name} blocked the attack!");
            return;
        }

        s.currentHP = Mathf.Max(0, s.currentHP - damage);
    }

    public void Defend(int index)
    {
        if (index >= 0 && index < sheep.Count)
        {
            sheep[index].defending = true;
            Debug.Log($"{sheep[index].name} is defending!");
        }
    }

    public void Appeal(Herd targetHerd, int targetIndex, Sheep source)
    {
        Sheep target = targetHerd.sheep[targetIndex];
        int baseChance = Mathf.Clamp(10 + source.charm * 3 - target.resolve * 2, 5, 95);
        int roll = Random.Range(1, 101);

        if (roll <= baseChance)
        {
            Debug.Log($"{target.name}'s heart softened... Tame chance increased!");
            target.resolve = Mathf.Max(1, target.resolve - 1); // easier to tame
        }
        else
        {
            Debug.Log($"{source.name}'s appeal failed!");
        }
    }

    public bool Tame(Herd targetHerd, int targetIndex)
    {
        Sheep target = targetHerd.sheep[targetIndex];

        if (target.tamed || target.currentHP <= 0)
            return false;

        int baseChance = Mathf.Clamp(20 + (100 - target.resolve * 10) - (target.currentHP * 2), 5, 90);
        int roll = Random.Range(1, 101);

        bool success = roll <= baseChance;
        if (success)
        {
            target.tamed = true;
            target.currentHP = 0;
            Debug.Log($"You tamed {target.name}!");
        }
        else
        {
            Debug.Log($"{target.name} resisted being tamed!");
        }

        return success;
    }

    public List<int> GetAliveIndices()
    {
        return sheep
            .Select((s, i) => new { s, i })
            .Where(x => x.s.currentHP > 0 && !x.s.tamed)
            .Select(x => x.i)
            .ToList();
    }

    public bool AllDown()
    {
        return sheep.All(s => s.currentHP <= 0 || s.tamed);
    }
}
