using UnityEngine;

public class SheepManager : MonoBehaviour
{

    public GameObject inactiveSheepPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject sheepIconPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Clear existing slots
        foreach (Transform child in inactiveSheepPanel.transform)
            Destroy(child.gameObject);

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
    }

}