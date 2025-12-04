using UnityEngine;
using UnityEngine.SceneManagement;

public class SheepPost : MonoBehaviour
{

    public GameObject sheepManager;
    private bool playerInRange = false;

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
            sheepManager.SetActive(true);
        else if (!playerInRange)
            sheepManager.SetActive(false);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInRange = false;
    }

}
