using UnityEngine;

public class Interact : MonoBehaviour
{
    public KeyCode interact = KeyCode.E;
    public DialogueManager dialogueManager;
    private NPC npc;

    void Update()
    {
        
        if (npc != null && Input.GetKeyDown(interact))
        {

            dialogueManager.ToggleDialogue(npc.dialogue, npc.portrait);

        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        NPC currentNPC = collision.GetComponent<NPC>();
        if (currentNPC != null)
            npc = currentNPC;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        npc = null;
        dialogueManager.HideDialogue();
    }

}
