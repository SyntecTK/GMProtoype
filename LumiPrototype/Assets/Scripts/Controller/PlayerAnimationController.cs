using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject raycastCenter;
    [SerializeField] private OrbStateController spiritOrb;
    [Header("Movement Settings")]
    [SerializeField] private float transitionSpeed = 0.2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [Header("Attack Settings")]
    [SerializeField] private float energyCost = 10f;

    private Vector2 moveVector;
    private string currentAnim = "";

    private bool isGrounded;
    private Rigidbody rb;

    private GameObject draggableObject;
    private bool isDragging = false;

    // Dragging
    private readonly HashSet<Draggable> _nearbyDraggables = new HashSet<Draggable>();
    private Draggable _currentDragged;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(GameManager.Instance.PlayerCanMove)
        {
            Debug.Log("MOVE");
            HandleMovement();
        }
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
        if (isGrounded && GameManager.Instance.PlayerCanMove)
        {
            rb.AddForce(new Vector3(0f, jumpForce, 0f));
            PlayAnimation("Jump");
        }
    }

    public void OnGrab(InputValue value)
    {
        if(draggableObject != null && !isDragging)
        {
            Debug.Log("Start Drag");
            var d = draggableObject.GetComponent<Draggable>();
            if (d != null)
            {
                d.StartDrag(transform);
                // Move object to player's front so player visually grabs it
                draggableObject.transform.position = transform.position + transform.forward * 1f;
            }
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            isDragging = true;
        }
        else if(isDragging)
        {
            Debug.Log("End Drag");
            draggableObject?.GetComponent<Draggable>().StopDrag();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            isDragging = false;
        }
    }

    public void OnAttack01(InputValue value)
    {
        bool modified = Keyboard.current.leftShiftKey.isPressed;

        if (Gamepad.current != null)
        {
            modified |= Gamepad.current.rightTrigger.ReadValue() > 0.5f;
        }

        if (modified)
        {
            spiritOrb.StartParry();
        }
        else
        {
            if (GameManager.Instance.UseEnergy(energyCost))
            {
                spiritOrb.StartAttack();
            }
            else
            {
                Debug.Log("Not enough Flow!");
            }
        }
        
    }
    private void HandleMovement()
    {
        float horizontalMove = moveVector.x;

        if (Mathf.Abs(horizontalMove) > 0.01f)
        {
            if(!isDragging)
            {
                transform.rotation = Quaternion.Euler(0, horizontalMove > 0 ? 90 : 270, 0);
            }

            transform.position += new Vector3(horizontalMove * moveSpeed * Time.deltaTime, 0f, 0f);

            if(isGrounded) PlayAnimation("Walk");
        }
        else
        {
            if(isGrounded) PlayAnimation("Idle");
        }

        isGrounded = Physics.Raycast(raycastCenter.transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    /// <summary>
    /// Whether the player is currently grounded. This is exposed so that other systems
    /// (eg. Camera) can avoid following the player while they are airborne (jumping).
    /// </summary>
    public bool IsGrounded => isGrounded;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Draggable d))
        {
            draggableObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Draggable d))
        {
            draggableObject = null;
        }
    }

    public void PlayParryMissAnimation()
    {
        StartCoroutine(ParryMissSequence());
    }

    private System.Collections.IEnumerator ParryMissSequence()
    {
        Debug.Log("Parry Miss Animation");
        // Store original state
        Vector3 originalPosition = transform.position;
        Quaternion originalRotation = transform.rotation;
        bool wasKinematic = rb.isKinematic;
        RigidbodyConstraints originalConstraints = rb.constraints;
        
        // Disable player control
        bool previousCanMove = GameManager.Instance.PlayerCanMove;
        GameManager.Instance.PlayerCanMove = false;

        // Rotate player 90 degrees on global X axis (fall down)
        transform.rotation = Quaternion.Euler(90f, transform.rotation.eulerAngles.y, 0f);
        
        // Enable gravity and physics
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        
        // Wait for 1 second while falling
        yield return new WaitForSeconds(1f);
        
        // Restore original state
        rb.isKinematic = true;
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        rb.isKinematic = wasKinematic;
        rb.constraints = originalConstraints;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Re-enable player control
        GameManager.Instance.PlayerCanMove = previousCanMove;
    }

}
