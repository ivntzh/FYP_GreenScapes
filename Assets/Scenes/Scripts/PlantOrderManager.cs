using UnityEngine;
using TMPro;

public class PlantOrderManager : MonoBehaviour
{
    public string[] possibleOrders = { "Plant", "Cactus"};
    public string currentOrderID;
    public TextMeshProUGUI orderText;

    void Start()
    {
        GenerateNewOrder();
    }

    public void GenerateNewOrder()
    {
        currentOrderID = possibleOrders[Random.Range(0, possibleOrders.Length)];
        if (orderText != null)
            orderText.text = $"📝 Order: {currentOrderID}";
    }

    public bool CheckPlantMatch(string plantID)
    {
        return plantID == currentOrderID;
    }
}