using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] Animator handsHolderAnimator;
    [SerializeField] Animator handsAnimator;

    private void Start()
    {
        GameController.staticReference.OnMoveUp += PlayMoveUp;
        GameController.staticReference.OnMoveRight += PlayMoveRight;
        GameController.staticReference.OnMoveDown += PlayMoveDown;
        GameController.staticReference.OnMoveLeft += PlayMoveLeft;

        GameController.staticReference.OnPickup += PlayGrab;
        GameController.staticReference.OnClap += PlayClap;
        GameController.staticReference.OnThumbsUp += PlayThumbsUp;
        GameController.staticReference.OnAngry += PlayAngry;

        GameController.staticReference.OnPocket += PlayPocket;
        GameController.staticReference.OnSmash += PlaySmash;
        GameController.staticReference.OnSpin += PlaySpin;
        GameController.staticReference.OnThrow += PlayThrow;
    }

    public void PlayMoveUp() { handsHolderAnimator.SetTrigger("Up"); }
    public void PlayMoveRight() { handsHolderAnimator.SetTrigger("Right"); }
    public void PlayMoveDown() { handsHolderAnimator.SetTrigger("Down"); }
    public void PlayMoveLeft() { handsHolderAnimator.SetTrigger("Left"); }

    public void PlayGrab(string itemName) { if (itemName != "") handsAnimator.SetTrigger("Grab"); }
    public void PlayClap() => handsAnimator.SetTrigger("Clap");
    public void PlayThumbsUp() => handsAnimator.SetTrigger("ThumbsUp");
    public void PlayAngry() => handsAnimator.SetTrigger("Angry");

    public void PlayPocket() => handsAnimator.SetTrigger("Pocket");
    public void PlaySmash() => handsAnimator.SetTrigger("Smash");
    public void PlaySpin() => handsAnimator.SetTrigger("Spin");
    public void PlayThrow() => handsAnimator.SetTrigger("Throw");
}
