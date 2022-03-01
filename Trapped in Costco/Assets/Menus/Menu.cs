using System.Collections;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField] bool initialVisibility = false;
    bool visible = true;

    [Header("Animation Properties")]
    [SerializeField] float animationOutY = 60;
    float animationInY;
    [SerializeField] float animationSpeed = 0.75f;
    Coroutine AppearAnimationCoroutine;
    Coroutine DisappearAnimationCoroutine;

    public virtual void Start()
    {
        animationInY = transform.position.y;

        visible = initialVisibility;
        if (!visible)
            transform.position = new Vector3(transform.position.x, animationOutY, transform.position.z);
    }

    public bool GetIsVisible() { return visible; }

    [ContextMenu("Appear")]
    public virtual void Appear()
    {
        if (!visible)
        {
            visible = true;

            if (DisappearAnimationCoroutine != null)
                StopCoroutine(DisappearAnimationCoroutine);
            AppearAnimationCoroutine = StartCoroutine(AppearAnimation());
        }
    }
    IEnumerator AppearAnimation()
    {
        float thresholdToStopAnimation = animationSpeed;
        while (Mathf.Abs(transform.position.y - animationInY) > thresholdToStopAnimation)
        {
            if (transform.position.y < animationInY)
                transform.position += Vector3.up * animationSpeed;
            else
                transform.position -= Vector3.up * animationSpeed;

            yield return new WaitForFixedUpdate();
        }
        transform.position = new Vector3(transform.position.x, animationInY, transform.position.z);
    }

    [ContextMenu("Disappear")]
    public void Disappear()
    {
        if (visible)
        {
            visible = false;

            if (AppearAnimationCoroutine != null)
                StopCoroutine(AppearAnimationCoroutine);
            DisappearAnimationCoroutine = StartCoroutine(DisappearAnimation());
        }
    }
    IEnumerator DisappearAnimation()
    {
        float thresholdToStopAnimation = animationSpeed;
        while (Mathf.Abs(transform.position.y - animationOutY) > thresholdToStopAnimation)
        {
            if (transform.position.y < animationOutY)
                transform.position += Vector3.up * animationSpeed;
            else
                transform.position -= Vector3.up * animationSpeed;

            yield return new WaitForFixedUpdate();
        }
        transform.position = new Vector3(transform.position.x, animationOutY, transform.position.z);
    }
}
