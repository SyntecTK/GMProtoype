using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject raycastCenter;
    [SerializeField] private SpiritorbController spiritOrb;
    [Header("Movement Settings")]
    [SerializeField] private float transitionSpeed = 0.2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [Header("Attack Settings")]
    [SerializeField] private float flowCost = 10f;
    [SerializeField] private float flowGain = 15f;
    [SerializeField] private float energyCost = 10f;
    [SerializeField] private float energyGain = 15f;



    private CharacterController controller;
    private Vector2 moveVector;
    private string currentAnim = "";

    private bool isGrounded;
    private float gravity = -9.81f;
    private float velocityY = 0f;
    private Rigidbody rb;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleMovement();
    }
    private void PlayAnimation(string stateName)
    {
        if (currentAnim == stateName) return;

        animator.CrossFade(stateName, transitionSpeed);
        currentAnim = stateName;
    }

    public void OnMove(InputValue value)
    {
        moveVector = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (isGrounded)
        {
            rb.AddForce(new Vector3(0f, jumpForce, 0f));
            PlayAnimation("Jump");
        }
    }

    public void OnAttack01(InputValue value)
    {
        if (GameManager.Instance.UseFlow(flowCost))
        {
            spiritOrb.StartAttack();
        }
        else
        {
            Debug.Log("Not enough Flow!");
        }
    }

    private void HandleMovement()
    {
        float horizontalMove = moveVector.x;

        if (Mathf.Abs(horizontalMove) > 0.01f)
        {
            transform.rotation = Quaternion.Euler(0, horizontalMove > 0 ? 90 : 270, 0);

            transform.position += new Vector3(horizontalMove * moveSpeed * Time.deltaTime, 0f, 0f);

            if(isGrounded) PlayAnimation("Walk");
        }
        else
        {
            if(isGrounded) PlayAnimation("Idle");
        }

        isGrounded = Physics.Raycast(raycastCenter.transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

}
