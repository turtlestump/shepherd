using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleStarter : MonoBehaviour
{
    public int sceneBuildIndex;

    private void OnTriggerEnter2D(Collider2D collision)
    {
       if (collision.tag == "Player")
        {

            SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Single);

        }
    }
}
