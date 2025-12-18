using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    Transform originalParent;
    CanvasGroup canvasGroup;
    SheepIcon sheepIcon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        sheepIcon = GetComponent<SheepIcon>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;      // Save original parent
        transform.SetParent(transform.root);    // Set above other canvases
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;               // Makes sheep semi-transparent during drag
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;    // Follows the mouse
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        canvasGroup.blocksRaycasts = true;   // Clickable again
        canvasGroup.alpha = 1f;

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();   // Slot where item is dropped

        if (dropSlot == null)
        {

            GameObject dropSheep = eventData.pointerEnter;
            if (dropSheep != null)
            {

                dropSlot = dropSheep.GetComponentInParent<Slot>();

            }

        }

        Slot originalSlot = originalParent.GetComponent<Slot>();

        // Cache sheep BEFORE changing anything
        Sheep movingSheep = sheepIcon.sheepData;
        Sheep targetSheep = dropSlot != null ? dropSlot.sheepData : null;

        if (dropSlot != null)
        {
            // --- VISUAL SWAP ---
            if (dropSlot.currentSheep != null)
            {
                // Move target sheep back to original slot
                dropSlot.currentSheep.transform.SetParent(originalSlot.transform);
                dropSlot.currentSheep.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                originalSlot.currentSheep = dropSlot.currentSheep;
                originalSlot.sheepData = targetSheep;
            }
            else
            {
                originalSlot.currentSheep = null;
                originalSlot.sheepData = null;
            }

            // Move dragged sheep into drop slot
            transform.SetParent(dropSlot.transform);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            dropSlot.currentSheep = gameObject;
            dropSlot.sheepData = movingSheep;

            // --- DATA UPDATE ---
            UpdateGameManagerData(originalSlot, dropSlot, movingSheep, targetSheep);
        }
        else
        {
            // Invalid drop - snap back
            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        SheepIcon.OnSheepSelected?.Invoke(movingSheep);

    }
    void UpdateGameManagerData(
    Slot from,
    Slot to,
    Sheep movingSheep,
    Sheep swappedSheep
)
    {
        var gm = GameManager.Instance;

        // Remove moving sheep from its original list
        if (from.slotType == SlotType.Party)
            gm.playerHerd.Remove(movingSheep);
        else
            gm.campStorage.Remove(movingSheep);

        // Add moving sheep to destination list
        if (to.slotType == SlotType.Party)
            gm.playerHerd.Add(movingSheep);
        else
            gm.campStorage.Add(movingSheep);

        // If this was a swap, move the other sheep back
        if (swappedSheep != null)
        {
            if (to.slotType == SlotType.Party)
                gm.playerHerd.Remove(swappedSheep);
            else
                gm.campStorage.Remove(swappedSheep);

            if (from.slotType == SlotType.Party)
                gm.playerHerd.Add(swappedSheep);
            else
                gm.campStorage.Add(swappedSheep);
        }
    }
}
