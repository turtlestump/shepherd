using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public enum BattleState { START, PLAYERTURN, ACTPHASE, ACTIONPHASE, ENEMYTURN, WIN, LOSE }

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

        state = BattleState.ACTPHASE;

        battleText.text = "Choose your sheep!";

        // Set other buttons to inactive.
        itemsButton.GetComponent<Button>().interactable = false;
        tameButton.GetComponent<Button>().interactable = false;
        fleeButton.GetComponent<Button>().interactable = false;


    }
    public void OnSheepButton1()
    {

        if (state != BattleState.ACTPHASE)
            return;

        // Check size of sheepSelected.
        if (sheepSelected.Count == 3)
        {

            sheepSelected.RemoveAt(0);
            playerSelections[0].SetActive(false);

        }

        if (sheepSelected.Contains(1) == false)
        {

            sheepSelected.Add(1);

            playerSelections[0].GetComponent<TMP_Text>().SetText(sheepSelected.Count.ToString());
            playerSelections[0].SetActive(true);

        }
        else
        {

            sheepSelected.Remove(1);
            playerSelections[0].SetActive(false);

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
    public void OnSheepButton2()
    {

        if (state != BattleState.ACTPHASE)
            return;

        // Check size of sheepSelected.
        if (sheepSelected.Count == 3)
        {

            sheepSelected.RemoveAt(0);
            playerSelections[0].SetActive(false);

        }

        if (sheepSelected.Contains(2) == false)
        {

            sheepSelected.Add(2);

            playerSelections[1].GetComponent<TMP_Text>().SetText(sheepSelected.Count.ToString());
            playerSelections[1].SetActive(true);

        }
        else
        {

            sheepSelected.Remove(2);
            playerSelections[1].SetActive(false);

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
    public void OnSheepButton3()
    {

        if (state != BattleState.ACTPHASE)
            return;

        // Check size of sheepSelected.
        if (sheepSelected.Count == 3)
        {

            sheepSelected.RemoveAt(0);
            playerSelections[0].SetActive(false);

        }

        if (sheepSelected.Contains(3) == false)
        {

            sheepSelected.Add(3);

            playerSelections[2].GetComponent<TMP_Text>().SetText(sheepSelected.Count.ToString());
            playerSelections[2].SetActive(true);

        }
        else
        {

            sheepSelected.Remove(3);
            playerSelections[2].SetActive(false);

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
    public void OnSheepButton4()
    {

        if (state != BattleState.ACTPHASE)
            return;

        // Check size of sheepSelected.
        if (sheepSelected.Count == 3)
        {

            sheepSelected.RemoveAt(0);
            playerSelections[0].SetActive(false);

        }

        if (sheepSelected.Contains(4) == false)
        {

            sheepSelected.Add(4);

            playerSelections[3].GetComponent<TMP_Text>().SetText(sheepSelected.Count.ToString());
            playerSelections[3].SetActive(true);

        }
        else
        {

            sheepSelected.Remove(4);
            playerSelections[3].SetActive(false);

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
    public void OnSheepButton5()
    {

        if (state != BattleState.ACTPHASE)
            return;

        // Check size of sheepSelected.
        if (sheepSelected.Count == 3)
        {

            sheepSelected.RemoveAt(0);
            playerSelections[0].SetActive(false);

        }

        if (sheepSelected.Contains(5) == false)
        {

            sheepSelected.Add(5);

            playerSelections[4].GetComponent<TMP_Text>().SetText(sheepSelected.Count.ToString());
            playerSelections[4].SetActive(true);

        }
        else
        {

            sheepSelected.Remove(5);
            playerSelections[4].SetActive(false);

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

        if (state != BattleState.ACTPHASE)
            return;

        state = BattleState.ACTIONPHASE;
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
