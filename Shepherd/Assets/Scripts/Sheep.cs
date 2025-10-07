using System.Collections;
using UnityEngine;

public class Sheep : MonoBehaviour
{

    // Basic attributes
    public string name;
    public int level;

    // Core stats
    public int strength;
    public int resolve;
    public int charm;
    public int speed;
    // public int special;

    // Battle stats
    public int maxHP;
    public int currentHP;

    // Flags
    public bool defending = false;
    public bool tamed = false;
    
    // Constructor
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

    }

}
