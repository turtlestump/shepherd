using UnityEngine;
using UnityEngine.EventSystems;

public class SheepIcon : MonoBehaviour, IPointerClickHandler
{
    public Sheep sheepData;

    public static System.Action<Sheep> OnSheepSelected;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSheepSelected?.Invoke(sheepData);
    }
}