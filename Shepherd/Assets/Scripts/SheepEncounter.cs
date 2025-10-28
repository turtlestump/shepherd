using UnityEngine;
using UnityEngine.SceneManagement;

public class SheepEncounter : MonoBehaviour
{
    public RandomEncounter spawner;
    public Transform player; // can be left unassigned in prefab
    public int sceneBuildIndex;
    public float maxLifetime = 20f;
    public float maxDistance = 25f;
    private float timer = 0f;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
            else
                Debug.LogWarning($"{name} could not find Player in scene!");
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Time-based despawn
        if (timer >= maxLifetime)
        {
            Despawn();
            return;
        }

        // Distance-based despawn (only if player is valid)
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance > maxDistance)
            {
                Despawn();
                return;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Single);
        }
    }

    void Despawn()
    {
        if (spawner != null)
            spawner.OnSheepRemoved();

        Destroy(gameObject);
    }
}