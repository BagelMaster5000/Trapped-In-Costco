using TMPro;
using UnityEngine;

public class FreeSamplesStand : MonoBehaviour
{
    [SerializeField] string[] allFreeSampleLines;
    [SerializeField] TextMeshProUGUI freeSampleDialogText;

    [SerializeField] Sprite[] allFreeSampleGraphics;
    [SerializeField] SpriteRenderer freeSampleSpriteRenderer;

    BoxCollider[] blockingColliders;
    Animator animator;

    private void Awake()
    {
        blockingColliders = GetComponents<BoxCollider>();
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
            Killed();
    }

    public void Attacked()
    {
        animator.SetTrigger("Attacked");
    }

    public void Killed()
    {
        GameController.staticReference.ClearBlockedDirections();

        foreach (BoxCollider bc in blockingColliders)
            bc.enabled = false;

        animator.SetTrigger("Killed");
    }

    public bool ReadyToBeDestroyed() { return !blockingColliders[0].enabled; }
}
