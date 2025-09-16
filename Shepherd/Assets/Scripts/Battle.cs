using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WIN, LOSE }

public class Battle : MonoBehaviour
{

    // Create references to player / enemy prefabs.
    public GameObject player;
    public GameObject enemy;

    // Create references to player / enemy herds (contains sheep stats).
    Herd playerHerd;
    Herd enemyHerd;

    // Create references to HUD panels.
    public BattleHUD[] playerHUDS;
    public BattleHUD[] enemyHUDS;

    // Create a reference to the BattlePanel text area.
    public TMP_Text battleText;

    // This will be used to change the battle state between turns.
    public BattleState state;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        SceneManager.LoadScene("BattleScene");
        state = BattleState.START;
        StartCoroutine(StartBattle());

    }

    IEnumerator StartBattle()
    {

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

    }

    public void OnActButton()
    {

        if (state != BattleState.PLAYERTURN)
            return;

    }

}
