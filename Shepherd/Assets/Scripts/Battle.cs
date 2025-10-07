using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public enum BattleState { START, PLAYERTURN, ACT, CHOOSEACTIONS, TAMING, RESOLVE, ENEMYTURN, WIN, LOSE }
public enum SheepAction { NONE, ATTACK, DEFEND, APPEAL }

public class Battle : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject player;
    public GameObject enemy;

    [Header("Herds")]
    private Herd playerHerd;
    private Herd enemyHerd;

    [Header("HUDs")]
    public BattleHUD[] playerHUDS;
    public BattleHUD[] enemyHUDS;

    [Header("UI Elements")]
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

    [Header("Action Buttons")]
    public Button attackButton;
    public Button defendButton;
    public Button appealButton;

    private BattleState state;
    private List<int> sheepSelected = new List<int>();

    // Represents one sheep’s action for the turn
    public class SheepCommand
    {
        public int sheepIndex;
        public SheepAction action;
        public int targetIndex;
        public int speed;
        public bool isPlayer;
    }

    private List<SheepCommand> playerCommands = new List<SheepCommand>();
    private int currentActionIndex = 0;

    // Target selection helpers
    private bool awaitingTarget = false;
    private SheepCommand pendingAttack = null;

    void Start()
    {
        state = BattleState.START;
        StartCoroutine(StartBattle());
    }

    IEnumerator StartBattle()
    {
        actButton.interactable = false;
        itemsButton.interactable = false;
        tameButton.interactable = false;
        fleeButton.interactable = false;
        goButton.gameObject.SetActive(false);
        actPanel.SetActive(false);

        // Instantiate player & enemy
        GameObject playerObject = Instantiate(player);
        playerHerd = playerObject.GetComponent<Herd>();

        GameObject enemyObject = Instantiate(enemy);
        enemyHerd = enemyObject.GetComponent<Herd>();

        battleText.text = "A wild herd appeared!";

        // Setup HUDs
        for (int i = 0; i < 5; i++)
        {
            playerHUDS[i].SetHUD(playerHerd, i);
            enemyHUDS[i].SetHUD(enemyHerd, i);
        }

        RefreshSheepButtons();
        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        RefreshSheepButtons();
        PlayerTurn();
    }

    void PlayerTurn()
    {
        RefreshSheepButtons();

        battleText.text = "Select an action:";
        actButton.interactable = true;
        itemsButton.interactable = true;
        tameButton.interactable = true;
        fleeButton.interactable = true;
    }

    public void OnActButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        state = BattleState.ACT;
        battleText.text = "Choose your 3 sheep!";
        actButton.interactable = false;
        itemsButton.interactable = false;
        tameButton.interactable = false;
        fleeButton.interactable = false;

        RefreshSheepButtons();
    }

    public void OnSheepButton(int sheep)
    {
        if (state != BattleState.ACT)
            return;

        if (playerHerd.sheep[sheep - 1].currentHP <= 0)
            return;

        int livingSheepCount = playerHerd.sheep.Count(s => s.currentHP > 0);
        int maxSelectable = Mathf.Min(3, livingSheepCount);

        if (!sheepSelected.Contains(sheep))
        {
            if (sheepSelected.Count >= maxSelectable)
            {
                int oldSheep = sheepSelected[0];
                playerSelections[oldSheep - 1].SetActive(false);
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

        goButton.gameObject.SetActive(sheepSelected.Count == maxSelectable);
    }

    public void OnGoButton()
    {
        if (state != BattleState.ACT)
            return;

        if (playerHerd.sheep.All(s => s.currentHP <= 0))
        {
            battleText.text = "No sheep left!";
            return;
        }

        state = BattleState.CHOOSEACTIONS;
        battlePanel.SetActive(false);
        actPanel.SetActive(true);
        battleText.text = "Choose actions for your sheep!";

        currentActionIndex = 0;
        playerCommands.Clear();
        EnableEnemyButtonsForTargeting(false);

        ShowActionPrompt();
    }

    void ShowActionPrompt()
    {
        if (currentActionIndex >= sheepSelected.Count)
        {
            actPanel.SetActive(false);
            battlePanel.SetActive(true);
            battleText.text = "The battle begins!";
            StartCoroutine(ResolveTurn());
            return;
        }

        int sheepID = sheepSelected[currentActionIndex];
        string sheepName = playerHerd.sheep[sheepID - 1].name;
        battleText.text = $"{sheepName} — Choose an action:";

        if (!awaitingTarget)
        {
            attackButton.interactable = true;
            defendButton.interactable = true;
            appealButton.interactable = true;
        }

        EnableEnemyButtonsForTargeting(false);
    }

    public void OnAttackButton() => BeginAttackChoice();
    public void OnDefendButton() => ChooseActionImmediate(SheepAction.DEFEND);
    public void OnAppealButton() => BeginAppealChoice();


    void ChooseActionImmediate(SheepAction action)
    {
        if (state != BattleState.CHOOSEACTIONS || awaitingTarget)
            return;

        int sheepID = sheepSelected[currentActionIndex];
        int speed = playerHerd.sheep[sheepID - 1].speed;

        SheepCommand cmd = new SheepCommand
        {
            sheepIndex = sheepID - 1,
            action = action,
            targetIndex = -1,
            speed = speed,
            isPlayer = true
        };

        playerCommands.Add(cmd);

        string sheepName = playerHerd.sheep[sheepID - 1].name;
        battleText.text = $"{sheepName} will {action}!";

        currentActionIndex++;
        StartCoroutine(NextActionPromptDelay());
    }

    void BeginAttackChoice()
    {
        if (state != BattleState.CHOOSEACTIONS || awaitingTarget)
            return;

        int sheepID = sheepSelected[currentActionIndex];
        int speed = playerHerd.sheep[sheepID - 1].speed;

        pendingAttack = new SheepCommand
        {
            sheepIndex = sheepID - 1,
            action = SheepAction.ATTACK,
            targetIndex = -1,
            speed = speed,
            isPlayer = true
        };

        awaitingTarget = true;

        attackButton.interactable = false;
        defendButton.interactable = false;
        appealButton.interactable = false;

        EnableEnemyButtonsForTargeting(true);
        battleText.text = "Select a target!";
    }

    void BeginAppealChoice()
    {
        if (state != BattleState.CHOOSEACTIONS || awaitingTarget)
            return;

        int sheepID = sheepSelected[currentActionIndex];

        pendingAttack = new SheepCommand
        {
            sheepIndex = sheepID - 1,
            action = SheepAction.APPEAL,
            targetIndex = -1,
            speed = playerHerd.sheep[sheepID - 1].speed,
            isPlayer = true
        };

        awaitingTarget = true;

        attackButton.interactable = false;
        defendButton.interactable = false;
        appealButton.interactable = false;

        // In this case, target is **enemy sheep**
        EnableEnemyButtonsForTargeting(true);
        battleText.text = "Select a sheep to appeal to!";
    }

    public void OnEnemyTargetSelected(int targetIndex)
    {
        if (!awaitingTarget || pendingAttack == null || state != BattleState.CHOOSEACTIONS)
            return;

        Sheep source = playerHerd.sheep[pendingAttack.sheepIndex];

        if (pendingAttack.action == SheepAction.ATTACK)
        {
            if (enemyHerd.sheep[targetIndex].currentHP <= 0)
                return;

            pendingAttack.targetIndex = targetIndex;
            playerCommands.Add(pendingAttack);

            string enemyName = enemyHerd.sheep[targetIndex].name;
            battleText.text = $"{source.name} will attack {enemyName}!";
        }
        else if (pendingAttack.action == SheepAction.APPEAL)
        {
            if (enemyHerd.sheep[targetIndex].currentHP <= 0)
                return;

            // Execute appeal immediately
            enemyHerd.Appeal(enemyHerd, targetIndex, source);

            string enemyName = enemyHerd.sheep[targetIndex].name;
            battleText.text = $"{source.name} appealed to {enemyName}!";
        }

        pendingAttack = null;
        awaitingTarget = false;
        EnableEnemyButtonsForTargeting(false);

        attackButton.interactable = true;
        defendButton.interactable = true;
        appealButton.interactable = true;

        currentActionIndex++;
        StartCoroutine(NextActionPromptDelay());
    }

    IEnumerator NextActionPromptDelay()
    {
        yield return new WaitForSeconds(0.6f);
        ShowActionPrompt();
    }

    IEnumerator ResolveTurn()
    {
        EnableEnemyButtonsForTargeting(false);
        actPanel.SetActive(false);
        battleText.text = "Executing actions...";
        yield return new WaitForSeconds(1f);

        List<SheepCommand> allCommands = new List<SheepCommand>(playerCommands);
        allCommands.AddRange(MockEnemyActions());
        allCommands = allCommands.OrderByDescending(c => c.speed).ToList();

        foreach (SheepCommand cmd in allCommands)
        {
            if (cmd.isPlayer)
                yield return ExecutePlayerAction(cmd);
            else
                yield return ExecuteEnemyAction(cmd);

            yield return new WaitForSeconds(0.75f);
            if (CheckBattleEnd()) yield break;
        }

        yield return new WaitForSeconds(1f);
        ResetSelections();

        state = BattleState.PLAYERTURN;
        battlePanel.SetActive(true);
        PlayerTurn();
    }

    void ResetSelections()
    {
        sheepSelected.Clear();
        foreach (var s in playerSelections)
            s.SetActive(false);

        goButton.gameObject.SetActive(false);
        RefreshSheepButtons();

        awaitingTarget = false;
        pendingAttack = null;
        playerCommands.Clear();
        currentActionIndex = 0;
    }

    bool CheckBattleEnd()
    {
        bool allEnemiesDown = enemyHerd.AllDown();
        bool allPlayersDown = playerHerd.AllDown();

        if (allEnemiesDown)
        {
            state = BattleState.WIN;
            battleText.text = "You win!";
            StartCoroutine(EndBattle());
            return true;
        }
        else if (allPlayersDown)
        {
            state = BattleState.LOSE;
            battleText.text = "You lose...";
            StartCoroutine(EndBattle());
            return true;
        }

        return false;
    }

    IEnumerator EndBattle()
    {
        yield return new WaitForSeconds(2f);
    }

    List<SheepCommand> MockEnemyActions()
    {
        List<SheepCommand> cmds = new List<SheepCommand>();
        List<int> aliveEnemies = enemyHerd.GetAliveIndices();
        List<int> alivePlayers = playerHerd.GetAliveIndices();

        if (alivePlayers.Count == 0 || aliveEnemies.Count == 0)
            return cmds;

        int actionsToTake = Mathf.Min(3, aliveEnemies.Count);
        aliveEnemies = aliveEnemies.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < actionsToTake; i++)
        {
            int sheepID = aliveEnemies[i];
            SheepCommand cmd = new SheepCommand
            {
                sheepIndex = sheepID,
                action = SheepAction.ATTACK,
                targetIndex = alivePlayers[Random.Range(0, alivePlayers.Count)],
                speed = enemyHerd.sheep[sheepID].speed,
                isPlayer = false
            };
            cmds.Add(cmd);
        }

        return cmds;
    }

    IEnumerator ExecutePlayerAction(SheepCommand cmd)
    {
        Sheep attacker = playerHerd.sheep[cmd.sheepIndex];
        if (attacker.currentHP <= 0) yield break;

        switch (cmd.action)
        {
            case SheepAction.ATTACK:
                Sheep target = enemyHerd.sheep[cmd.targetIndex];
                int damage = playerHerd.damage[cmd.sheepIndex];
                enemyHerd.TakeDamage(cmd.targetIndex, damage);
                enemyHUDS[cmd.targetIndex].SetHP(target.currentHP);
                battleText.text = $"{attacker.name} attacked {target.name}!";
                break;

            case SheepAction.DEFEND:
                attacker.defending = true;
                battleText.text = $"{attacker.name} is defending!";
                break;

            case SheepAction.APPEAL:
                int heal = 5 + attacker.charm / 2;
                attacker.currentHP = Mathf.Min(attacker.maxHP, attacker.currentHP + heal);
                playerHUDS[cmd.sheepIndex].SetHP(attacker.currentHP);
                battleText.text = $"{attacker.name} appealed and recovered {heal} HP!";
                break;
        }

        yield return new WaitForSeconds(1.2f);
        RefreshSheepButtons();
    }

    IEnumerator ExecuteEnemyAction(SheepCommand cmd)
    {
        Sheep attacker = enemyHerd.sheep[cmd.sheepIndex];
        if (attacker.currentHP <= 0) yield break;

        Sheep target = playerHerd.sheep[cmd.targetIndex];
        if (target.currentHP <= 0) yield break;

        switch (cmd.action)
        {
            case SheepAction.ATTACK:
                int damage = enemyHerd.damage[cmd.sheepIndex];
                if (target.defending)
                {
                    damage /= 2;
                    target.defending = false;
                }
                target.currentHP = Mathf.Max(0, target.currentHP - damage);
                playerHUDS[cmd.targetIndex].SetHP(target.currentHP);
                battleText.text = $"{attacker.name} attacked {target.name}!";
                break;

            case SheepAction.APPEAL:
                int heal = 5 + attacker.charm / 2;
                attacker.currentHP = Mathf.Min(attacker.maxHP, attacker.currentHP + heal);
                enemyHUDS[cmd.sheepIndex].SetHP(attacker.currentHP);
                break;
        }

        yield return new WaitForSeconds(1.2f);
        RefreshSheepButtons();
    }

    void RefreshSheepButtons()
    {
        if (playerHerd != null)
        {
            for (int i = 0; i < playerSheep.Length; i++)
            {
                bool alive = playerHerd.sheep[i].currentHP > 0;
                playerSheep[i].interactable = alive && state == BattleState.ACT;
            }
        }

        if (enemyHerd != null)
        {
            for (int i = 0; i < enemySheep.Length; i++)
            {
                bool alive = enemyHerd.sheep[i].currentHP > 0;
                enemySheep[i].interactable = false;
            }
        }
    }

    void EnableEnemyButtonsForTargeting(bool enable)
    {
        if (enemyHerd == null || enemySheep == null) return;

        for (int i = 0; i < enemySheep.Length; i++)
        {
            bool alive = enemyHerd.sheep[i].currentHP > 0;
            enemySheep[i].interactable = enable && alive;
        }
    }

    // Called by Tame button
    public void OnTameButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        List<int> aliveEnemies = enemyHerd.GetAliveIndices();
        if (aliveEnemies.Count == 0)
        {
            battleText.text = "No sheep to tame!";
            return;
        }

        state = BattleState.TAMING;
        battleText.text = "Select an enemy sheep to tame!";
        EnableEnemyButtonsForTargeting(true);
    }

    // Called when an enemy sheep button is clicked
    public void OnEnemyTameTarget(int targetIndex)
    {
        if (state != BattleState.TAMING)
            return;

        // Roll to tame the enemy sheep
        bool success = enemyHerd.Tame(enemyHerd, targetIndex); // target is self-contained

        if (success)
            battleText.text = $"You tamed {enemyHerd.sheep[targetIndex].name}!";
        else
            battleText.text = $"{enemyHerd.sheep[targetIndex].name} resisted being tamed!";

        // Disable buttons
        EnableEnemyButtonsForTargeting(false);

        // Wait a moment so the player can read the result
        StartCoroutine(PostTameEnemyTurn());
    }

    private IEnumerator PostTameEnemyTurn()
    {
        yield return new WaitForSeconds(1.5f);

        // Start enemy turn
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }


    public void OnFleeButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        state = BattleState.RESOLVE; // temporary block actions
        battleText.text = "Attempting to flee...";

        int fleeChance = 50; // Base chance
        int totalEnemySpeed = enemyHerd.sheep.Where(s => s.currentHP > 0).Sum(s => s.speed);
        fleeChance = Mathf.Clamp(fleeChance - totalEnemySpeed / 5, 5, 95);

        int roll = Random.Range(1, 101);

        if (roll <= fleeChance)
        {
            battleText.text = "You successfully fled!";
            StartCoroutine(EndBattle()); // End battle early
        }
        else
        {
            battleText.text = "Failed to flee!";
            StartCoroutine(FailedFleeDelay());
        }
    }

    IEnumerator FailedFleeDelay()
    {
        yield return new WaitForSeconds(1.5f);
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    // Coroutine for enemy actions after a turn-ending action (tame/flee)
    IEnumerator EnemyTurn()
    {
        List<SheepCommand> enemyActions = MockEnemyActions();
        enemyActions = enemyActions.OrderByDescending(c => c.speed).ToList();

        foreach (var cmd in enemyActions)
        {
            yield return ExecuteEnemyAction(cmd);
            if (CheckBattleEnd()) yield break;
            yield return new WaitForSeconds(0.75f);
        }

        state = BattleState.PLAYERTURN;
        battlePanel.SetActive(true);
        PlayerTurn();
    }
}
