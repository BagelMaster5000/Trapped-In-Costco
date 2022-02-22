using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EmployeeBlockade : MonoBehaviour
{
    [SerializeField] string[] allEmployeeLines;
    [SerializeField] TextMeshProUGUI employeeDialogText;

    [SerializeField] Sprite[] allEmployeeGraphics;
    [SerializeField] SpriteRenderer employeeSpriteRenderer;

    BoxCollider blockingCollider;
    Animator animator;

    private void Awake()
    {
        blockingCollider = GetComponent<BoxCollider>();
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
        GameController.staticReference.ClearBlockade();

        blockingCollider.enabled = false;

        animator.SetTrigger("Killed");
    }
}
