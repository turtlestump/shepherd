using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public Image portraitImage;

    private bool isActive = false;

    public void ShowDialogue(string text, Sprite portrait)
    {

        dialoguePanel.SetActive(true);
        dialogueText.text = text;
        portraitImage.sprite = portrait;
        isActive = true;

    }

    public void HideDialogue()
    {

        dialoguePanel.SetActive(false);
        isActive = false;

    }

    public void ToggleDialogue(string text, Sprite portrait)
    {

        if (isActive)
            HideDialogue();
        else
        {
            ShowDialogue(text, portrait);
            Debug.Log($"Test");
        }
            


    }

}
