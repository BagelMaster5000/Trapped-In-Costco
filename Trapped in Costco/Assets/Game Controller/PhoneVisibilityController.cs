using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneVisibilityController : MonoBehaviour
{
    [SerializeField] Animator phoneAnimator;
    [SerializeField] InputAction phoneToggleInput;

    public Action OnPhoneVisible;
    public Action OnPhoneInvisible;

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

        GameController.staticReference.OnGameRestart += () =>
        {
            OnPhoneVisible = null;
            OnPhoneInvisible = null;
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
    public void ShowPhoneScreen()
    { 
        phoneAnimator.SetBool("Up", true);

        OnPhoneVisible?.Invoke();
    }
    public void HidePhoneScreen()
    {
        phoneAnimator.SetBool("Up", false);

        OnPhoneInvisible?.Invoke();
    }

    public bool GetIsVisible() { return phoneAnimator.GetBool("Up"); }
}
