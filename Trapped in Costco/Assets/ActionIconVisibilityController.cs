using UnityEngine;

public class ActionIconVisibilityController : MonoBehaviour
{
    [SerializeField] GameObject heldItemActionButtons;
    [SerializeField] GameObject noItemActionButtons;

    private void Start()
    {
        ShowNoItemActionButtons();


        GameController.staticReference.OnPickup += ShowHeldItemActionButtons;

        GameController.staticReference.OnPocket += ShowNoItemActionButtons;
        GameController.staticReference.OnSmash += ShowNoItemActionButtons;
        GameController.staticReference.OnThrow += ShowNoItemActionButtons;
    }

    public void ShowHeldItemActionButtons(string dummy)
    {
        heldItemActionButtons.SetActive(true);
        noItemActionButtons.SetActive(false);
    }
    public void ShowNoItemActionButtons()
    {
        noItemActionButtons.SetActive(true);
        heldItemActionButtons.SetActive(false);
    }
}
