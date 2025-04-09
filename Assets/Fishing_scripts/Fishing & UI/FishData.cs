using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "FishData", menuName = "FishData", order = 1)]
public class FishData : ScriptableObject
{
    public string fishName;
    public GameObject inventoryItem;
    public int probability; // this is for percentage to catch the fish
}
