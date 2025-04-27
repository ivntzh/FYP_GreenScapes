using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlantOrderManager : MonoBehaviour
{
    public string[] possibleOrders = { "Plant", "Cactus" };
    public string currentOrderID;

    [Header("UI Order Images")]
    public RawImage plantOrderImage;
    public RawImage cactusOrderImage;

    void Start()
    {
        GenerateNewOrder();
    }

    public void GenerateNewOrder()
    {
        currentOrderID = possibleOrders[Random.Range(0, possibleOrders.Length)];

        UpdateOrderUI();
    }

    void UpdateOrderUI()
    {
        if (plantOrderImage != null) plantOrderImage.enabled = false;
        if (cactusOrderImage != null) cactusOrderImage.enabled = false;

        if (currentOrderID == "Plant")
        {
            if (plantOrderImage != null) plantOrderImage.enabled = true;
        }
        else if (currentOrderID == "Cactus")
        {
            if (cactusOrderImage != null) cactusOrderImage.enabled = true;
        }
    }

    public bool CheckPlantMatch(string plantID)
    {
        return plantID == currentOrderID;
    }
}