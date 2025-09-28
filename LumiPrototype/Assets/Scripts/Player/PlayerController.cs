using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3;
    [SerializeField] private InputActionAsset actionMap;
    [SerializeField] private Animator animator;
    [SerializeField] private List<Hitbox> hitboxes;

    [SerializeField] private TMP_Text comboTXT;

    private InputAction moveAction;
    private InputAction attackAction;
    private CharacterController controller;
    private Vector2 movementInput;
    private bool isAttacking;

    // Attack System
    private int comboCount = 0;
    private string[] comboAttacks = { "Attack01", "Attack02", "Attack03" };
    private bool hasHitEnemy = false;

    // Animation State Tracking
    private string currentAnimationState = "";

    //Physics
    private float gravity = -9.81f;
    private float verticalVelocity = 0f;
    private float animTimer = 0f;
    private Vector3 baseRotation;

    private void OnEnable()
    {
        moveAction = actionMap.FindActionMap("Player").FindAction("Move");
        moveAction.performed += OnMovementPerformed;
        moveAction.canceled += OnMovementCanceled;
        moveAction.Enable();

        attackAction = actionMap.FindActionMap("Player").FindAction("Attack");
        attackAction.performed += OnAttackPerformed;
        attackAction.Enable();
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMovementPerformed;
        moveAction.canceled -= OnMovementCanceled;
        moveAction.Disable();
        attackAction.performed -= OnAttackPerformed;
        attackAction.Disable();
    }

    private void Update()
    {
        HandleAttack();
        HandleMovement();
        ApplyGravity();

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        movementInput = Vector2.zero;
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        // Wenn attacking und Gegner getroffen wurde, Animation canceln und nächsten Angriff starten
        if (isAttacking && hasHitEnemy)
        {
            CancelCurrentAttack();
            StartNextAttack();
            return;
        }

        // Neuer Angriff starten (nur wenn nicht attacking)
        if (!isAttacking)
        {
            StartAttack();
        }
    }

    private void StartAttack()
    {
        comboCount = 1;
        isAttacking = true;
        hasHitEnemy = false;
        UpdateComboText();
        ExecuteAttack();
    }

    private void StartNextAttack()
    {
        comboCount++;
        if (comboCount > comboAttacks.Length)
        {
            comboCount = 1; // Zurück zum ersten Angriff
        }

        UpdateComboText();

        hasHitEnemy = false;
        ExecuteAttack();
    }

    private void ExecuteAttack()
    {
        string attackAnim = comboAttacks[comboCount - 1];
        animator.Play(attackAnim);

        currentAnimationState = attackAnim;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        animTimer = state.length;
    }

    private void CancelCurrentAttack()
    {
        animTimer = 0f;
    }

    public void OnEnemyHit(GameObject enemy)
    {
        hasHitEnemy = true;
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (comboCount == 3)
        {

            if (rb != null)
            {
                Vector3 knockbackDir = new Vector3(1, 1, 0).normalized;
                float knockbackForce = 4f;

                rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
            }
        }
        else
        {
            if (rb != null)
            {
                Vector3 knockbackDir = new Vector3(0, 1, 0).normalized;
                float knockbackForce = 2f;

                rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
            }
        }
    }


    private void HandleAttack()
    {
        if (isAttacking)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName(currentAnimationState) && state.normalizedTime >= 1f)
            {
                isAttacking = false;
                hasHitEnemy = false;
                comboCount = 0;
                UpdateComboText();
            }
        }
    }

    private void HandleMovement()
    {
        if (isAttacking) return;

        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
        move *= moveSpeed;

        string newAnimationState;
        if (move != Vector3.zero)
        {
            newAnimationState = "Step";
            Quaternion targetRotation = Quaternion.LookRotation(-move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        else
        {
            newAnimationState = "Idle";
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        // Animation nur abspielen wenn sich der State geändert hat
        if (currentAnimationState != newAnimationState)
        {
            currentAnimationState = newAnimationState;
            animator.Play(newAnimationState);
        }

        if (move != Vector3.zero)
        {

        }

        controller.Move(move * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 gravityMove = new Vector3(0, verticalVelocity, 0);
        controller.Move(gravityMove * Time.deltaTime);
    }

    private void UpdateComboText()
    {
        if (comboTXT != null)
        {
            comboTXT.text = comboCount.ToString();
        }
    }

    #region public methods
    public void EnableHitboxes()
    {
        foreach (var hitbox in hitboxes)
        {
            hitbox.EnableHitbox();
        }
    }

    public void DisableHitboxes()
    {
        foreach (var hitbox in hitboxes)
        {
            hitbox.Disable();
        }
    }
    #endregion
}
