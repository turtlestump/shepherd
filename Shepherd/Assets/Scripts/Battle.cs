using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.InputSystem;

public enum BattleState { START, PLAYERTURN, ACT, CHOOSEACTIONS, TAMING, RESOLVE, ENEMYTURN, WIN, LOSE }
public enum SheepAction { NONE, ATTACK, DEFEND, APPEAL }

public class Battle : MonoBehaviour
{
    // Prefab references
    public GameObject player;
    public GameObject enemy;

    // Object references
    private Herd playerHerd;
    private Herd enemyHerd;

    // UI Elements
    public BattleHUD[] playerHUDS;
    public BattleHUD[] enemyHUDS;
    public Button[] playerSheep;
    public Button[] enemySheep;
    public GameObject[] playerSelections;
    public TMP_Text battleText;
    public TMP_Text statsText;
    public TMP_Text infoText;
    public TMP_InputField nameField;
    public Button actButton;
    public Button itemsButton;
    public Button tameButton;
    public Button fleeButton;
    public Button goButton;
    public Button confirmButton;
    public GameObject battlePanel;
    public GameObject actPanel;
    public GameObject summaryPanel;

    // Action buttons
    public Button attackButton;
    public Button defendButton;
    public Button appealButton;

    private BattleState state;
    private List<int> sheepSelected = new List<int>();

    private bool transitioning = false;

    // Summary display
    private int summaryIndex = 0;
    private List<Sheep> summarySheep = new List<Sheep>();

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
        // Disable buttons at start
        actButton.interactable = false;
        itemsButton.interactable = false;
        tameButton.interactable = false;
        fleeButton.interactable = false;
        goButton.gameObject.SetActive(false);
        actPanel.SetActive(false);

        // Instantiate player & enemy herds
        GameObject playerObject = Instantiate(player);
        playerHerd = playerObject.GetComponent<Herd>();

        GameObject enemyObject = Instantiate(enemy);
        enemyHerd = enemyObject.GetComponent<Herd>();

        // Load player herd dynamically from GameManager
        playerHerd.sheep.Clear();
        foreach (var sData in GameManager.Instance.playerHerd)
            playerHerd.AddSheepFromData(sData);

        // Restore player sheep HP for testing
        foreach (var sheep in playerHerd.sheep)
        {
            sheep.currentHP = sheep.maxHP;
        }

        // Limit enemy herd to 1–3 sheep safely
        int enemyCount = enemyHerd.sheep.Count;
        if (enemyCount > 1)
        {
            int minEnemy = 1;
            int maxEnemy = Mathf.Min(3, enemyCount);
            enemyCount = Random.Range(minEnemy, maxEnemy + 1); // Random.Range max is exclusive for ints, so +1
            enemyHerd.sheep = enemyHerd.sheep.Take(enemyCount).ToList();
        }

        // Setup player HUD/buttons safely
        int playerCount = playerHerd.sheep.Count;
        for (int i = 0; i < playerHUDS.Length; i++)
        {
            bool active = i < playerCount;
            playerHUDS[i].gameObject.SetActive(active);
            playerSheep[i].gameObject.SetActive(active);
            if (active)
                playerHUDS[i].SetHUD(playerHerd, i);
        }

        // Setup enemy HUD/buttons safely
        int enemyCountSafe = enemyHerd.sheep.Count;
        for (int i = 0; i < enemyHUDS.Length; i++)
        {
            bool active = i < enemyCountSafe;
            enemyHUDS[i].gameObject.SetActive(active);
            enemySheep[i].gameObject.SetActive(active);
            if (active)
                enemyHUDS[i].SetHUD(enemyHerd, i);
        }

        RefreshSheepButtons();
        yield return new WaitForSeconds(1f);

        state = BattleState.PLAYERTURN;
        RefreshSheepButtons();
        PlayerTurn();
    }

    void PlayerTurn()
    {
        RefreshSheepButtons();
        goButton.interactable = true;

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

        int sheepIndex = sheep - 1; // Convert to 0-based index

        // Safety check: ensure the index is within the player's herd
        if (sheepIndex < 0 || sheepIndex >= playerHerd.sheep.Count)
            return;

        if (playerHerd.sheep[sheepIndex].currentHP <= 0)
            return;

        int livingSheepCount = playerHerd.sheep.Count(s => s.currentHP > 0);
        int maxSelectable = Mathf.Min(3, livingSheepCount);

        if (!sheepSelected.Contains(sheep))
        {
            if (sheepSelected.Count >= maxSelectable)
            {
                int oldSheep = sheepSelected[0];
                int oldIndex = oldSheep - 1;

                if (oldIndex >= 0 && oldIndex < playerSelections.Length)
                    playerSelections[oldIndex].SetActive(false);

                sheepSelected.RemoveAt(0);
            }

            if (sheepIndex >= 0 && sheepIndex < playerSelections.Length)
            {
                playerSelections[sheepIndex].SetActive(true);
                playerSelections[sheepIndex].GetComponent<TMP_Text>().SetText("S");
            }

            sheepSelected.Add(sheep);
        }
        else
        {
            if (sheepIndex >= 0 && sheepIndex < playerSelections.Length)
                playerSelections[sheepIndex].SetActive(false);

            sheepSelected.Remove(sheep);
        }

        // Enable the Go button if the player has selected the allowed number of sheep
        goButton.gameObject.SetActive(sheepSelected.Count == maxSelectable);
    }

    public void OnGoButton()
    {
        goButton.interactable = false;

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

    public void OnConfirmButton()
    {
        // Safety
        if (summaryIndex >= summarySheep.Count)
            return;

        Sheep s = summarySheep[summaryIndex];

        // Apply name if provided
        if (!string.IsNullOrWhiteSpace(nameField.text))
            s.name = nameField.text;

        // Now add this sheep to persistent herd
        GameManager.Instance.AddTamedSheep(s);

        summaryIndex++;

        // If more tamed sheep exist, show next one
        if (summaryIndex < summarySheep.Count)
        {
            ShowSummary();
        }
        else
        {
            // No more – transition to world scene
            StartCoroutine(ExitSummary());
        }
    }

    void ShowActionPrompt()
    {

        // Safety reset
        awaitingTarget = false;
        pendingAttack = null;
        EnableEnemyButtonsForTargeting(false);

        attackButton.interactable = true;
        defendButton.interactable = true;
        appealButton.interactable = true;

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
    public void OnDefendButton() => ChooseAction(SheepAction.DEFEND);
    public void OnAppealButton() => BeginAppealChoice();


    void ChooseAction(SheepAction action)
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
        // Safety checks
        if (state != BattleState.CHOOSEACTIONS || !awaitingTarget || pendingAttack == null)
        {
            Debug.LogWarning($"OnEnemyTargetSelected ignored: state={state}, awaitingTarget={awaitingTarget}, pendingAttackNull={pendingAttack == null}");
            return; // ignore stray clicks (don't alter UI because we aren't in target mode)
        }

        // Validate index bounds and that target is alive
        if (targetIndex < 0 || targetIndex >= enemyHerd.sheep.Count || enemyHerd.sheep[targetIndex].currentHP <= 0)
        {
            battleText.text = "Invalid target — choose a living sheep!";
            // Keep targeting UI active so the player can pick again
            EnableEnemyButtonsForTargeting(true);
            attackButton.interactable = false;
            defendButton.interactable = false;
            appealButton.interactable = false;
            return;
        }

        Sheep source = playerHerd.sheep[pendingAttack.sheepIndex];

        if (pendingAttack.action == SheepAction.ATTACK)
        {
            pendingAttack.targetIndex = targetIndex;
            playerCommands.Add(pendingAttack);

            string enemyName = enemyHerd.sheep[targetIndex].name;
            battleText.text = $"{source.name} will attack {enemyName}!";
        }
        else if (pendingAttack.action == SheepAction.APPEAL)
        {
            // Execute appeal immediately (as your original code does)
            enemyHerd.Appeal(enemyHerd, targetIndex, source);
            string enemyName = enemyHerd.sheep[targetIndex].name;
            battleText.text = $"{source.name} appealed to {enemyName}!";
        }

        // Reset targeting state and restore UI for next action
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
        if (transitioning) yield break;
        transitioning = true;

        // Give the UI a full frame to settle before the next prompt
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.3f);

        transitioning = false;
        ShowActionPrompt();
    }

    IEnumerator ResolveTurn()
    {
        EnableEnemyButtonsForTargeting(false);
        actPanel.SetActive(false);
        battleText.text = "Executing actions...";
        yield return new WaitForSeconds(1f);

        List<SheepCommand> allCommands = new List<SheepCommand>(playerCommands);
        allCommands.AddRange(EnemyActions());
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
        bool allEnemiesDownOrTamed = enemyHerd.AllUntamedEnemiesDownOrTamed();
        bool allPlayersDown = playerHerd.AllDown();

        if (allEnemiesDownOrTamed)
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
        // If no sheep were tamed, skip to scene transition
        if (enemyHerd.tamedSheep.Count == 0)
        {
            yield return new WaitForSeconds(1.5f);
            SceneManager.LoadScene("WorldScene");
            yield break;
        }

        // Store tamed sheep locally before adding to GameManager
        summarySheep = new List<Sheep>(enemyHerd.tamedSheep);
        summaryIndex = 0;

        // Hide normal UI
        battlePanel.SetActive(false);
        actPanel.SetActive(false);

        // Show summary
        ShowSummary();
        summaryPanel.SetActive(true);

        // Wait for player input through confirmButton
    }

    void ShowSummary()
    {
        // Safety
        if (summaryIndex >= summarySheep.Count)
            return;

        Sheep s = summarySheep[summaryIndex];

        infoText.text = $"Lv.  {s.level}" +
            $"\nHP: {s.currentHP} / {s.maxHP}";
        statsText.text = $"Strength: {s.speed}" +
            $"\n\nResolve: {s.resolve}" +
            $"\n\nCharm: {s.charm}" +
            $"\n\nSpeed: {s.speed}" +
            $"\n\nSpecial: ";

        nameField.text = ""; // clear input field for naming
    }

    IEnumerator ExitSummary()
    {
        summaryPanel.SetActive(false);
        battleText.text = "Returning to the world...";
        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene("WorldScene");
    }

    List<SheepCommand> EnemyActions()
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
            int playerCount = playerHerd.sheep.Count;
            for (int i = 0; i < playerSheep.Length; i++)
            {
                if (i < playerCount)
                {
                    bool alive = playerHerd.sheep[i].currentHP > 0;
                    playerSheep[i].interactable = alive && state == BattleState.ACT;
                }
                else
                {
                    // Disable buttons beyond the herd size
                    playerSheep[i].interactable = false;
                }
            }
        }

        if (enemyHerd != null)
        {
            int enemyCount = enemyHerd.sheep.Count;
            for (int i = 0; i < enemySheep.Length; i++)
            {
                if (i < enemyCount)
                {
                    bool alive = enemyHerd.sheep[i].currentHP > 0;
                    enemySheep[i].interactable = false; // default: not interactable
                }
                else
                {
                    enemySheep[i].interactable = false; // extra buttons off
                }
            }
        }
    }

    void EnableEnemyButtonsForTargeting(bool enable)
    {
        if (enemyHerd == null || enemySheep == null) return;

        int count = Mathf.Min(enemySheep.Length, enemyHerd.sheep.Count);

        for (int i = 0; i < count; i++)
        {
            bool alive = enemyHerd.sheep[i].currentHP > 0;
            enemySheep[i].interactable = enable && alive;
        }

        // Extra buttons beyond herd size are always disabled
        for (int i = count; i < enemySheep.Length; i++)
        {
            enemySheep[i].interactable = false;
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

        bool success = enemyHerd.Tame(enemyHerd, targetIndex);

        if (success)
            battleText.text = $"You tamed {enemyHerd.sheep[targetIndex].name}!";
        else
            battleText.text = $"{enemyHerd.sheep[targetIndex].name} resisted being tamed!";

        EnableEnemyButtonsForTargeting(false);

        // Immediately check for battle end
        if (CheckBattleEnd())
            return;

        // Otherwise, proceed to enemy turn
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

        // Only proceed if battle hasn’t ended
        if (!CheckBattleEnd())
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    // Coroutine for enemy actions after a turn-ending action (tame/flee)
    IEnumerator EnemyTurn()
    {
        List<SheepCommand> enemyActions = EnemyActions();
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
