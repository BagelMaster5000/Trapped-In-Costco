using System.Collections;
using TMPro;
using UnityEngine;

public class EmployeeBlockade : MonoBehaviour
{
    [SerializeField] string[] allEmployeeLines;
    [SerializeField] TextMeshProUGUI employeeDialogText;

    [SerializeField] Sprite[] allEmployeeGraphics;
    [SerializeField] SpriteRenderer employeeSpriteRenderer;

    BoxCollider[] blockingColliders;
    Animator animator;

    private void Awake()
    {
        blockingColliders = GetComponents<BoxCollider>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        employeeDialogText.text = allEmployeeLines[Random.Range(0, allEmployeeLines.Length)];
        employeeSpriteRenderer.sprite = allEmployeeGraphics[Random.Range(0, allEmployeeGraphics.Length)];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CostcoProduct"))
            HitByItem();
    }

    void HitByItem()
    {
        GameController.staticReference.ClearBlockedDirections();

        foreach (BoxCollider bc in blockingColliders)
            bc.enabled = false;

        animator.SetTrigger("Killed");
    }

    public bool ReadyToBeDestroyed() { return !blockingColliders[0].enabled; }
}
