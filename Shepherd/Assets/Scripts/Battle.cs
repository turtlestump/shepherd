using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public enum BattleState { START, PLAYERTURN, ACT, ACTION, ENEMYTURN, WIN, LOSE }

public class Battle : MonoBehaviour
{

    // Create references to player / enemy prefabs.
    public GameObject player;
    public GameObject enemy;

    // Create references to player / enemy herds (contains sheep stats).
    Herd playerHerd;
    Herd enemyHerd;

    // Create references to HUD elements.
    public BattleHUD[] playerHUDS;
    public BattleHUD[] enemyHUDS;
    public Button[] playerSheep;
    public Button[] enemySheep;
    public GameObject[] playerSelections;
    public TMP_Text battleText;
    public Button actButton;
    public Button itemsButton;
    public Button tameButton;
    public Button fleeButton;
    public Button goButton;
    public GameObject battlePanel;
    public GameObject actPanel;

    // This will be used to change the battle state between turns.
    public BattleState state;

    // Create a temporary array to hold chosen sheep.
    public List<int> sheepSelected = new List<int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        state = BattleState.START;
        StartCoroutine(StartBattle());

    }

    IEnumerator StartBattle()
    {

        // Set buttons to inactive.
        actButton.GetComponent<Button>().interactable = false;
        itemsButton.GetComponent<Button>().interactable = false;
        tameButton.GetComponent<Button>().interactable = false;
        fleeButton.GetComponent<Button>().interactable = false;
        goButton.gameObject.SetActive(false);

        // Instantiate player / enemy GameObjects.
        GameObject playerObject = Instantiate(player);
        playerHerd = playerObject.GetComponent<Herd>();

        GameObject enemyObject = Instantiate(enemy);
        enemyHerd = enemyObject.GetComponent<Herd>();

        // Display starting text.
        battleText.text = "A wild herd appeared!";

        for (int i = 0; i < 5; i++)
        {

            playerHUDS[i].SetHUD(playerHerd, i);
            enemyHUDS[i].SetHUD(enemyHerd, i);

        }

        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();

    }

    void PlayerTurn()
    {

        battleText.text = "Select an action:";

        // Activate HUD buttons.
        actButton.GetComponent<Button>().interactable = true;
        itemsButton.GetComponent<Button>().interactable = true;
        tameButton.GetComponent<Button>().interactable = true;
        fleeButton.GetComponent<Button>().interactable = true;

    }

    public void OnActButton()
    {

        if (state != BattleState.PLAYERTURN)
            return;

        state = BattleState.ACT;

        battleText.text = "Choose your sheep!";

        // Set other buttons to inactive.
        itemsButton.GetComponent<Button>().interactable = false;
        tameButton.GetComponent<Button>().interactable = false;
        fleeButton.GetComponent<Button>().interactable = false;


    }
    public void OnFleeButton(int sceneBuildIndex)
    {

        SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Single);

    }

    public void OnSheepButton(int sheep)
    {

        if (state != BattleState.ACT)
            return;

        if (sheepSelected.Contains(sheep) == false)
        {

            // Check size of sheepSelected.
            if (sheepSelected.Count >= 3)
            {

                playerSelections[sheepSelected[0] - 1].SetActive(false);
                sheepSelected.RemoveAt(0);
                
            }

            playerSelections[sheep - 1].SetActive(true);
            playerSelections[sheep - 1].GetComponent<TMP_Text>().SetText("S");
            sheepSelected.Add(sheep);
            
        }
        else
        {

            playerSelections[sheep - 1].SetActive(false);
            sheepSelected.Remove(sheep);
            
        }

        // Make GO! Button active
        if (sheepSelected.Count == 3)
        {

            goButton.gameObject.SetActive(true);

        }
        else
        {

            goButton.gameObject.SetActive(false);

        }

    }
    public void OnGoButton()
    {

        if (state != BattleState.ACT)
            return;

        state = BattleState.ACTION;
        battlePanel.SetActive(false);
        actPanel.SetActive(true);

        // Reset selection numbers.
        for (int i = 0; i < sheepSelected.Count; i++)
        {

            sheepSelected.RemoveAt(i);

        }
        for (int i = 0; i < playerSelections.Length; i++)
        {

            playerSelections[i].SetActive(false);

        }

    }

}
