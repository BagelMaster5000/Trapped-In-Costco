using TMPro;
using UnityEngine;

public class ItemNameDisplayController : MonoBehaviour
{
    TextMeshProUGUI itemText;

    void Awake()
    {
        itemText = GetComponent<TextMeshProUGUI>();

        GameController.staticReference.OnPickup += DisplayItemName;
        GameController.staticReference.OnThrow += HideItemName;
        GameController.staticReference.OnPocket += HideItemName;
        GameController.staticReference.OnSmash += HideItemName;
    }

    private void Start()
    {
        HideItemName();
    }

    void DisplayItemName(string displayItemName) { itemText.text = displayItemName; }
    void HideItemName() { itemText.text = ""; }
}
