using TMPro;
using UnityEngine;

public class SheepManager : MonoBehaviour
{

    public GameObject inactiveSheepPanel;
    public GameObject partySheepPanel;
    public GameObject slotPrefab;
    public GameObject partySlotPrefab;
    public int slotCount;
    public int partyCount;
    public GameObject sheepIconPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Clear existing slots
        foreach (Transform child in inactiveSheepPanel.transform)
            Destroy(child.gameObject);
        foreach (Transform child in partySheepPanel.transform)
            Destroy(child.gameObject);

        // Populate inactive panel
        for (int i = 0; i < slotCount; i++)
        {
            // Create slot
            Slot slot = Instantiate(slotPrefab, inactiveSheepPanel.transform).GetComponent<Slot>();

            // If we have a sheep for this slot, add the icon
            if (i < GameManager.Instance.campStorage.Count)
            {
                Sheep s = GameManager.Instance.campStorage[i];

                GameObject icon = Instantiate(sheepIconPrefab, slot.transform);
                icon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                SheepIcon sheepIcon = icon.GetComponent<SheepIcon>();
                sheepIcon.sheepData = s;

                slot.currentSheep = icon;
                slot.sheepData = s;
            }
            else
            {
                // Optional: mark empty slot with null
                slot.currentSheep = null;
                slot.sheepData = null;
            }
        }

        // Populate active panel
        for (int i = 0; i < partyCount; i++)
        {

            // Create party slot
            Slot partySlot = Instantiate(partySlotPrefab, partySheepPanel.transform)
                .GetComponent<Slot>();

            // If we have a party sheep for this slot, add icon
            if (i < GameManager.Instance.playerHerd.Count)
            {
                Sheep s = GameManager.Instance.playerHerd[i];
                TMP_Text text = partySlot.GetComponentInChildren<TMP_Text>();
                text.SetText(s.name);

                GameObject icon = Instantiate(sheepIconPrefab, partySlot.transform);
                icon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                SheepIcon sheepIcon = icon.GetComponent<SheepIcon>();
                sheepIcon.sheepData = s;

                partySlot.currentSheep = icon;
                partySlot.sheepData = s;
            }
            else
            {
                partySlot.currentSheep = null;
                partySlot.sheepData = null;
            }
        }

    }

}