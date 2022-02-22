using TMPro;
using UnityEngine;

public class ItemNameDisplayController : MonoBehaviour
{
    GameController gameController;
    TextMeshProUGUI itemText;

    void Awake()
    {
        gameController = FindObjectOfType<GameController>();
        itemText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        HideItemName();

        gameController.OnPickup += DisplayItemName;
        gameController.OnThrow += HideItemName;
        gameController.OnPocket += HideItemName;
        gameController.OnSmash += HideItemName;
    }

    void DisplayItemName(GameObject dummy, string displayItemName) { itemText.text = displayItemName; }
    void HideItemName() { itemText.text = ""; }
}
