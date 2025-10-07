using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public enum BattleState { START, PLAYERTURN, ACT, CHOOSEACTIONS, RESOLVE, ENEMYTURN, WIN, LOSE }
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
    public Button[] playerSheep;      // buttons for player's sheep (index 0..4)
    public Button[] enemySheep;       // buttons for enemy's sheep (index 0..4)
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
        public int sheepIndex;   // 0-based index into herd arrays
        public SheepAction action;
        public int targetIndex;  // 0-based index into enemy/player arrays
        public int speed;
        public bool isPlayer;
    }

    private List<SheepCommand> playerCommands = new List<SheepCommand>();
    private int currentActionIndex = 0;

    // --- Target selection helpers ---
    private bool awaitingTarget = false;
    private SheepCommand pendingAttack = null;

    void Start()
    {
        state = BattleState.START;
        StartCoroutine(StartBattle());
    }

    IEnumerator StartBattle()
    {
        // Disable buttons initially
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

        // Ensure buttons reflect initial HP & state
        RefreshSheepButtons();

        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;

        // update buttons now that we are in PLAYERTURN
        RefreshSheepButtons();

        PlayerTurn();
    }

    void PlayerTurn()
    {
        // Update UI and ensure buttons reflect alive/dead
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

        // Update the sheep buttons now that we're in ACT so live sheep become clickable
        RefreshSheepButtons();
    }


    public void OnSheepButton(int sheep)
    {
        if (state != BattleState.ACT)
            return;

        // 🛑 Ignore if sheep is dead
        if (playerHerd.currentHP[sheep - 1] <= 0)
            return;

        int livingSheepCount = playerHerd.currentHP.Count(hp => hp > 0);
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

        // If player has no live sheep, don't proceed
        if (playerHerd.currentHP.All(h => h <= 0))
        {
            battleText.text = "No sheep left!";
            return;
        }

        state = BattleState.CHOOSEACTIONS;
        battlePanel.SetActive(false);
        actPanel.SetActive(true);
        battleText.text = $"Choose actions for your sheep!";

        currentActionIndex = 0;
        playerCommands.Clear();

        // Ensure enemy buttons are disabled until needed
        EnableEnemyButtonsForTargeting(false);

        ShowActionPrompt();
    }

    void ShowActionPrompt()
    {
        // If we've chosen actions for all selected sheep, finalize
        if (currentActionIndex >= sheepSelected.Count)
        {
            // ✅ All actions chosen → switch panels and start battle
            actPanel.SetActive(false);
            battlePanel.SetActive(true);
            battleText.text = "The battle begins!";
            StartCoroutine(ResolveTurn());
            return;
        }

        int sheepID = sheepSelected[currentActionIndex];
        string sheepName = playerHerd.sheepNames[sheepID - 1];
        battleText.text = $"{sheepName} — Choose an action:";

        // Ensure action buttons are enabled (unless awaitingTarget)
        if (!awaitingTarget)
        {
            attackButton.interactable = true;
            defendButton.interactable = true;
            appealButton.interactable = true;
        }

        // Make sure enemy target buttons are disabled by default
        EnableEnemyButtonsForTargeting(false);
    }

    public void OnAttackButton() => BeginAttackChoice();
    public void OnDefendButton() => ChooseActionImmediate(SheepAction.DEFEND);
    public void OnAppealButton() => ChooseActionImmediate(SheepAction.APPEAL);

    // When player chooses DEFEND or APPEAL we add the command immediately
    void ChooseActionImmediate(SheepAction action)
    {
        if (state != BattleState.CHOOSEACTIONS || awaitingTarget)
            return;

        int sheepID = sheepSelected[currentActionIndex];
        int speed = playerHerd.sheepLevels[sheepID - 1]; // temporary speed proxy

        SheepCommand cmd = new SheepCommand
        {
            sheepIndex = sheepID - 1,
            action = action,
            targetIndex = -1,
            speed = speed,
            isPlayer = true
        };

        playerCommands.Add(cmd);

        // Feedback & advance
        string sheepName = playerHerd.sheepNames[sheepID - 1];
        battleText.text = $"{sheepName} will {action}!";
        currentActionIndex++;

        StartCoroutine(NextActionPromptDelay());
    }

    // Begin target-selection flow for attack
    void BeginAttackChoice()
    {
        if (state != BattleState.CHOOSEACTIONS || awaitingTarget)
            return;

        int sheepID = sheepSelected[currentActionIndex];
        int speed = playerHerd.sheepLevels[sheepID - 1]; // use level until a speed stat is added

        pendingAttack = new SheepCommand
        {
            sheepIndex = sheepID - 1,
            action = SheepAction.ATTACK,
            targetIndex = -1,
            speed = speed,
            isPlayer = true
        };

        awaitingTarget = true;

        // Disable other action buttons until target chosen
        attackButton.interactable = false;
        defendButton.interactable = false;
        appealButton.interactable = false;

        // Enable only live enemy buttons so player can choose a target
        EnableEnemyButtonsForTargeting(true);

        battleText.text = "Select a target!";
    }

    // Called by enemy buttons' OnClick (pass index 0..4)
    // e.g. in the Inspector: EnemyButton.OnClick -> Battle.OnEnemyTargetSelected (int) with correct index
    public void OnEnemyTargetSelected(int targetIndex)
    {
        if (!awaitingTarget || pendingAttack == null || state != BattleState.CHOOSEACTIONS)
            return;

        // Ignore if target dead
        if (enemyHerd.currentHP[targetIndex] <= 0)
            return;

        // Lock target and add to commands
        pendingAttack.targetIndex = targetIndex;
        playerCommands.Add(pendingAttack);

        string sheepName = playerHerd.sheepNames[pendingAttack.sheepIndex];
        string enemyName = enemyHerd.sheepNames[targetIndex];
        battleText.text = $"{sheepName} will attack {enemyName}!";

        // Reset pending state BEFORE the coroutine is called
        pendingAttack = null;
        awaitingTarget = false;

        // Disable target buttons again
        EnableEnemyButtonsForTargeting(false);

        // Advance to next sheep action (UI will re-enable buttons next frame)
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
        // Make sure player can't click enemy buttons during resolution
        EnableEnemyButtonsForTargeting(false);

        actPanel.SetActive(false);
        battleText.text = "Executing actions...";
        yield return new WaitForSeconds(1f);

        // Get AI actions
        List<SheepCommand> allCommands = new List<SheepCommand>(playerCommands);
        allCommands.AddRange(MockEnemyActions());

        // Sort by speed descending
        allCommands = allCommands.OrderByDescending(c => c.speed).ToList();

        // Alternate actions with visible updates
        foreach (SheepCommand cmd in allCommands)
        {
            if (cmd.isPlayer)
                yield return ExecutePlayerAction(cmd);
            else
                yield return ExecuteEnemyAction(cmd);

            yield return new WaitForSeconds(0.75f);

            // Stop if battle ends early
            if (CheckBattleEnd()) yield break;
        }

        // Wait a second before starting next turn
        yield return new WaitForSeconds(1f);

        // Reset selections & re-enable buttons for next turn
        ResetSelections();

        // Return to player turn
        state = BattleState.PLAYERTURN;
        battlePanel.SetActive(true);
        PlayerTurn();
    }

    void ResetSelections()
    {
        // Clear player selections and reset highlights
        sheepSelected.Clear();
        for (int i = 0; i < playerSelections.Length; i++)
        {
            playerSelections[i].SetActive(false);
        }

        // Disable GO button until picks again
        goButton.gameObject.SetActive(false);

        // Re-enable alive sheep buttons only
        RefreshSheepButtons();

        // Clear pending states
        awaitingTarget = false;
        pendingAttack = null;
        playerCommands.Clear();
        currentActionIndex = 0;
    }

    bool CheckBattleEnd()
    {
        bool allEnemiesDown = enemyHerd.currentHP.All(hp => hp <= 0);
        bool allPlayersDown = playerHerd.currentHP.All(hp => hp <= 0);

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
        // You can add scene transitions or restart here if desired.
    }

    // --- Enemy AI: choose up to N unique living enemies to act (N = min(3, aliveEnemiesCount)) ---
    List<SheepCommand> MockEnemyActions()
    {
        List<SheepCommand> cmds = new List<SheepCommand>();

        // Get list of alive enemy sheep (indices)
        List<int> aliveEnemies = Enumerable.Range(0, enemyHerd.currentHP.Length)
                                           .Where(i => enemyHerd.currentHP[i] > 0)
                                           .ToList();

        // Get list of alive player sheep (for targets)
        List<int> alivePlayers = Enumerable.Range(0, playerHerd.currentHP.Length)
                                           .Where(i => playerHerd.currentHP[i] > 0)
                                           .ToList();

        // If no alive players, stop battle early
        if (alivePlayers.Count == 0 || aliveEnemies.Count == 0)
            return cmds;

        // Select up to 3 living enemies to act (unique)
        int actionsToTake = Mathf.Min(3, aliveEnemies.Count);

        // Shuffle aliveEnemies then take first actionsToTake to avoid repeated same attacker
        aliveEnemies = aliveEnemies.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < actionsToTake; i++)
        {
            int sheepID = aliveEnemies[i];

            SheepCommand cmd = new SheepCommand
            {
                sheepIndex = sheepID,
                action = SheepAction.ATTACK, // can be expanded later
                targetIndex = alivePlayers[Random.Range(0, alivePlayers.Count)],
                speed = enemyHerd.sheepLevels[sheepID], // temporary proxy for speed
                isPlayer = false
            };

            cmds.Add(cmd);
        }

        return cmds;
    }

    IEnumerator ExecutePlayerAction(SheepCommand cmd)
    {
        // Skip if attacker died before their action
        if (playerHerd.currentHP[cmd.sheepIndex] <= 0)
            yield break;

        // If attacking, ensure there's a live enemy target (retarget if needed)
        if (cmd.action == SheepAction.ATTACK)
        {
            // If target died, retarget to a live enemy
            if (cmd.targetIndex < 0 || enemyHerd.currentHP[cmd.targetIndex] <= 0)
            {
                List<int> aliveEnemies = Enumerable.Range(0, enemyHerd.currentHP.Length)
                                                   .Where(i => enemyHerd.currentHP[i] > 0)
                                                   .ToList();

                if (aliveEnemies.Count == 0)
                {
                    yield break; // no targets
                }

                cmd.targetIndex = aliveEnemies[Random.Range(0, aliveEnemies.Count)];
            }
        }

        string name = playerHerd.sheepNames[cmd.sheepIndex];
        string actionText = cmd.action == SheepAction.ATTACK ? $"attacked {enemyHerd.sheepNames[cmd.targetIndex]}" : cmd.action.ToString();
        battleText.text = $"{name} {actionText}!";
        yield return new WaitForSeconds(1.2f);

        switch (cmd.action)
        {
            case SheepAction.ATTACK:
                int damage = playerHerd.sheepDamage[cmd.sheepIndex];
                enemyHerd.currentHP[cmd.targetIndex] = Mathf.Max(0, enemyHerd.currentHP[cmd.targetIndex] - damage);
                enemyHUDS[cmd.targetIndex].SetHP(enemyHerd.currentHP[cmd.targetIndex]);

                // Disable enemy button if it's dead
                if (enemyHerd.currentHP[cmd.targetIndex] <= 0)
                    enemySheep[cmd.targetIndex].interactable = false;
                break;

            case SheepAction.DEFEND:
                // TODO: implement defend mechanics (e.g., reduce damage next hit)
                break;

            case SheepAction.APPEAL:
                int heal = 5;
                playerHerd.currentHP[cmd.sheepIndex] = Mathf.Min(playerHerd.maxHP[cmd.sheepIndex], playerHerd.currentHP[cmd.sheepIndex] + heal);
                playerHUDS[cmd.sheepIndex].SetHP(playerHerd.currentHP[cmd.sheepIndex]);
                break;
        }

        // refresh buttons in case HP changed
        RefreshSheepButtons();
    }

    IEnumerator ExecuteEnemyAction(SheepCommand cmd)
    {
        // Skip dead attackers
        if (enemyHerd.currentHP[cmd.sheepIndex] <= 0)
            yield break;

        // Skip if all players are dead
        if (playerHerd.currentHP.All(hp => hp <= 0))
            yield break;

        // If target died before execution, retarget
        if (cmd.action == SheepAction.ATTACK && (cmd.targetIndex < 0 || playerHerd.currentHP[cmd.targetIndex] <= 0))
        {
            List<int> alivePlayers = Enumerable.Range(0, playerHerd.currentHP.Length)
                                               .Where(i => playerHerd.currentHP[i] > 0)
                                               .ToList();

            if (alivePlayers.Count == 0)
                yield break;

            cmd.targetIndex = alivePlayers[Random.Range(0, alivePlayers.Count)];
        }

        string name = enemyHerd.sheepNames[cmd.sheepIndex];
        string actionText = cmd.action == SheepAction.ATTACK ? $"attacked {playerHerd.sheepNames[cmd.targetIndex]}" : cmd.action.ToString();
        battleText.text = $"{name} {actionText}!";
        yield return new WaitForSeconds(1.2f);

        switch (cmd.action)
        {
            case SheepAction.ATTACK:
                int damage = enemyHerd.sheepDamage[cmd.sheepIndex];
                playerHerd.currentHP[cmd.targetIndex] = Mathf.Max(0, playerHerd.currentHP[cmd.targetIndex] - damage);
                playerHUDS[cmd.targetIndex].SetHP(playerHerd.currentHP[cmd.targetIndex]);

                // Disable button if target dies
                if (playerHerd.currentHP[cmd.targetIndex] <= 0)
                    playerSheep[cmd.targetIndex].interactable = false;
                break;

            case SheepAction.DEFEND:
                // TODO: implement defend
                break;

            case SheepAction.APPEAL:
                int heal = 5;
                enemyHerd.currentHP[cmd.sheepIndex] = Mathf.Min(enemyHerd.maxHP[cmd.sheepIndex], enemyHerd.currentHP[cmd.sheepIndex] + heal);
                enemyHUDS[cmd.sheepIndex].SetHP(enemyHerd.currentHP[cmd.sheepIndex]);
                break;
        }

        RefreshSheepButtons();
    }

    // Make player/enemy buttons interactable only if that sheep is alive.
    void RefreshSheepButtons()
    {
        if (playerHerd != null)
        {
            for (int i = 0; i < playerSheep.Length; i++)
            {
                // Player sheep selection buttons should only be interactable if alive and we're in selection phase
                bool alive = playerHerd.currentHP[i] > 0;
                playerSheep[i].interactable = alive && state == BattleState.ACT;
            }
        }

        if (enemyHerd != null)
        {
            for (int i = 0; i < enemySheep.Length; i++)
            {
                bool alive = enemyHerd.currentHP[i] > 0;
                // Enemy buttons are normally disabled outside of targeting mode
                enemySheep[i].interactable = false;
                // If we explicitly enable them for targeting, the caller will set them to true
            }
        }
    }

    // Enable/disable enemy buttons for player target selection. Only alive enemies become interactable.
    void EnableEnemyButtonsForTargeting(bool enable)
    {
        if (enemyHerd == null || enemySheep == null) return;

        for (int i = 0; i < enemySheep.Length; i++)
        {
            bool alive = enemyHerd.currentHP[i] > 0;
            enemySheep[i].interactable = enable && alive;
        }
    }

    public void OnFleeButton(int sceneBuildIndex)
    {
        SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Single);
    }
}
