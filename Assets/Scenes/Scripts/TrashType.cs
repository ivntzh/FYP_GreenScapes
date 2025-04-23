using UnityEngine;

public enum TrashCategory
{
    Paper,
    Glass,
    PlasticMetal,
    NonRecyclable
}

public class TrashType : MonoBehaviour
{
    public TrashCategory trashCategory; // Set this in Inspector per trash prefab
}