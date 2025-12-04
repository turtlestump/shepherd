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

        if (dropSlot != null)
        {
            if (dropSlot.currentSheep != null)
            {

                // Swap items
                dropSlot.currentSheep.transform.SetParent(originalSlot.transform);
                originalSlot.currentSheep = dropSlot.currentSheep;
                dropSlot.currentSheep.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                SheepIcon swappedIcon = dropSlot.currentSheep.GetComponent<SheepIcon>();
                originalSlot.sheepData = swappedIcon.sheepData;

            }
            else
            {
                originalSlot.currentSheep = null;
                originalSlot.sheepData = null;
            }

            // Move item into drop slot
            transform.SetParent(dropSlot.transform);
            dropSlot.currentSheep = gameObject;

            dropSlot.sheepData = sheepIcon.sheepData;
        }
        else
        {
            transform.SetParent(originalParent);
            originalSlot.currentSheep = gameObject;

            originalSlot.sheepData = sheepIcon.sheepData;
        }

        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
