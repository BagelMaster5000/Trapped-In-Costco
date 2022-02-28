using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneVisibilityController : MonoBehaviour
{
    [SerializeField] Animator phoneAnimator;
    [SerializeField] InputAction phoneToggleInput;

    private void Awake()
    {
        GameController.staticReference.OnGamePause += () =>
        {
            phoneToggleInput.Disable();
        };
        GameController.staticReference.OnGameUnpause += () =>
        {
            phoneToggleInput.Enable();
        };
    }

    private void Start()
    {
        phoneToggleInput.performed += ctx => ToggleVisibility();
        phoneToggleInput.Enable();
    }

    public void ToggleVisibility()
    {
        if (GetIsVisible()) HidePhoneScreen();
        else ShowPhoneScreen();
    }
    public void ShowPhoneScreen() { phoneAnimator.SetBool("Up", true); }
    public void HidePhoneScreen() { phoneAnimator.SetBool("Up", false); }

    public bool GetIsVisible() { return phoneAnimator.GetBool("Up"); }
}
