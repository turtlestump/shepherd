using System.Collections;
using UnityEngine;

public class RandomEncounter : MonoBehaviour
{

    public GameObject sheepPrefab;
    public Transform playerPosition;
    public int maxSheep = 3;
    public float spawnInterval = 1f;
    public float spawnChance = 1f;
    public float spawnRadius = 8f;
    public float minDistance = 5f;

    private int sheepOnScreen = 0;

    void Start()
    {

        StartCoroutine(Spawner());

    }

    IEnumerator Spawner()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            Debug.Log($"[Spawner Tick] sheepOnScreen = {sheepOnScreen}");

            if (sheepOnScreen < maxSheep)
            {
                float roll = Random.value;
                Debug.Log($"[Spawner Tick] Roll = {roll}, spawnChance = {spawnChance}");

                if (roll <= spawnChance)
                    Spawn();
            }
        }
    }

    void Spawn()
    {
        Vector2 direction = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minDistance, spawnRadius);
        Vector2 position = (Vector2)playerPosition.position + direction * distance;

        GameObject sheep = Instantiate(sheepPrefab, position, Quaternion.identity);
        SheepEncounter encounter = sheep.GetComponent<SheepEncounter>();
        if (encounter != null)
        {
            encounter.spawner = this;
        }

        sheepOnScreen++;
        Debug.Log($"[Spawn] Sheep spawned. Total: {sheepOnScreen}");
    }

    public void OnSheepRemoved()
    {
        sheepOnScreen = Mathf.Max(0, sheepOnScreen - 1);
        Debug.Log($"[Despawn] Sheep removed. Total: {sheepOnScreen}");
    }

}
