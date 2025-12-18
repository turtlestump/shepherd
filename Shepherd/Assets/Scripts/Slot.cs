using UnityEngine;
public enum SlotType
{
    Party,
    Storage
}

public class Slot : MonoBehaviour
{
    public SlotType slotType;
    public GameObject currentSheep;
    public Sheep sheepData;
}