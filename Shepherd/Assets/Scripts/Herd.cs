using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Herd : MonoBehaviour
{
    // Data lists
    public List<Sheep> sheep = new List<Sheep>();
    public List<Sheep> tamedSheep = new List<Sheep>();

    // Accessor arrays (for HUD & Battle)
    public string[] names => sheep.Select(s => s.name).ToArray();
    public int[] levels => sheep.Select(s => s.level).ToArray();
    public int[] maxHP => sheep.Select(s => s.maxHP).ToArray();
    public int[] currentHP => sheep.Select(s => s.currentHP).ToArray();
    public int[] charm => sheep.Select(s => s.charm).ToArray();
    public int[] resolve => sheep.Select(s => s.resolve).ToArray();
    public int[] speed => sheep.Select(s => s.speed).ToArray();
    public int[] damage => sheep.Select(s => GetAttackDamage(s)).ToArray();

    public int GetAttackDamage(Sheep s)
    {
        int baseDamage = s.strength * 2;
        int variance = Random.Range(-2, 3);
        return Mathf.Max(1, baseDamage + variance);
    }

    public void TakeDamage(int index, int amount)
    {
        if (index < 0 || index >= sheep.Count) return;
        Sheep s = sheep[index];

        if (s.defending)
        {
            s.defending = false;
            Debug.Log($"{s.name} blocked the attack!");
            return;
        }

        s.currentHP = Mathf.Max(0, s.currentHP - amount);
    }

    public void Defend(int index)
    {
        if (index < 0 || index >= sheep.Count) return;
        sheep[index].defending = true;
        Debug.Log($"{sheep[index].name} is defending!");
    }

    public void Appeal(Herd targetHerd, int targetIndex, Sheep source)
    {
        if (targetHerd == null) return;
        if (targetIndex < 0 || targetIndex >= targetHerd.sheep.Count) return;

        Sheep target = targetHerd.sheep[targetIndex];
        int baseChance = Mathf.Clamp(10 + source.charm * 3 - target.resolve * 2, 5, 95);
        int roll = Random.Range(1, 101);

        if (roll <= baseChance)
        {
            Debug.Log($"{target.name}'s heart softened... Tame chance increased!");
            target.resolve = Mathf.Max(1, target.resolve - 1);
        }
        else
        {
            Debug.Log($"{source.name}'s appeal failed!");
        }
    }

    public bool Tame(Herd targetHerd, int targetIndex)
    {
        if (targetHerd == null) return false;
        if (targetIndex < 0 || targetIndex >= targetHerd.sheep.Count) return false;

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
            targetHerd.tamedSheep.Add(target);
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

    public bool AllUntamedEnemiesDownOrTamed()
    {
        return sheep.All(s => s.currentHP <= 0 || tamedSheep.Contains(s));
    }

    // Duplicate incoming data object so runtime changes won't mutate original GameManager data
    public Sheep AddSheepFromData(Sheep data)
    {
        if (data == null) return null;

        Sheep copy = data.Clone();
        copy.currentHP = copy.maxHP;
        copy.tamed = false;
        sheep.Add(copy);
        return copy;
    }
}