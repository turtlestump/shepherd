using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Persistent, data-only herd
    public List<Sheep> playerHerd = new List<Sheep>();
    public List<Sheep> campStorage = new List<Sheep>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (playerHerd == null) playerHerd = new List<Sheep>();
        if (campStorage == null) campStorage = new List<Sheep>();

        // Ensure a starter sheep exists (optional)
        if (playerHerd.Count == 0)
        {
            playerHerd.Add(new Sheep("PSheep", 1, 5, 5, 5, 5));
        }
    }

    // Add a tamed sheep (data) to persistent storage (keeps to max 5 -> camp logic can be elsewhere)
    public void AddTamedSheep(Sheep newSheep)
    {
        if (newSheep == null) return;

        const int MaxPlayerHerd = 5;

        if (playerHerd.Count < MaxPlayerHerd)
        {
            playerHerd.Add(newSheep.Clone());
            Debug.Log($"Added {newSheep.name} to player herd.");
        }
        else
        {
            campStorage.Add(newSheep.Clone());
            Debug.Log($"Player herd full. Stored {newSheep.name} in camp.");
        }
    }
}