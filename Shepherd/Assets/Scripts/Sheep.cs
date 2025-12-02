using System;
[Serializable]
public class Sheep
{
    public string name;
    public int level;

    // Core stats
    public int strength;
    public int resolve;
    public int charm;
    public int speed;

    // Battle stats
    public int maxHP;
    public int currentHP;

    // Flags
    public bool defending = false;
    public bool tamed = false;

    public Sheep() { }

    public Sheep(string name, int level, int strength, int resolve, int charm, int speed)
    {
        this.name = name;
        this.level = level;
        this.strength = strength;
        this.resolve = resolve;
        this.charm = charm;
        this.speed = speed;

        // Derived stats
        maxHP = 20 + resolve * 2;
        currentHP = maxHP;
        defending = false;
        tamed = false;
    }

    // Convenience: clone
    public Sheep Clone()
    {
        return new Sheep
        {
            name = this.name,
            level = this.level,
            strength = this.strength,
            resolve = this.resolve,
            charm = this.charm,
            speed = this.speed,
            maxHP = this.maxHP,
            currentHP = this.currentHP,
            defending = this.defending,
            tamed = this.tamed
        };
    }
}