using TMPro;
using UnityEngine;

public class FreeSamplesStand : MonoBehaviour
{
    [SerializeField] string[] allFreeSampleLines;
    [SerializeField] TextMeshProUGUI freeSampleDialogText;

    [SerializeField] Sprite[] allFreeSampleGraphics;
    [SerializeField] SpriteRenderer freeSampleSpriteRenderer;

    BoxCollider blockingCollider;
    Animator animator;

    private void Awake()
    {
        blockingCollider = GetComponent<BoxCollider>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        freeSampleDialogText.text = allFreeSampleLines[Random.Range(0, allFreeSampleLines.Length)];
        freeSampleSpriteRenderer.sprite = allFreeSampleGraphics[Random.Range(0, allFreeSampleGraphics.Length)];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CostcoProduct"))
            HitByItem();
    }

    void HitByItem()
    {
        GameController.staticReference.ClearBlockade();

        blockingCollider.enabled = false;

        animator.SetTrigger("Killed");
    }

    public bool ReadyToBeDestroyed() { return !blockingCollider.enabled; }
}
