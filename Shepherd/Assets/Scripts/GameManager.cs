using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<SheepData> playerHerd = new List<SheepData>();

    private void Awake()
    {
        
        if (Instance != null && Instance != this)
        {

            Destroy(gameObject);
            return;

        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize first sheep if empty
        if (playerHerd.Count == 0)
        {
            Sheep starter = new Sheep("PSheep", 1, 5, 5, 5, 5);
            playerHerd.Add(new SheepData(starter));
        }
    }

    public void AddTamedSheep(Sheep newSheep)
    {
        playerHerd.Add(new SheepData(newSheep));
    }

}
