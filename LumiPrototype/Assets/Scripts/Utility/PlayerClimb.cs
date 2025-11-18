using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerClimb : MonoBehaviour
{
	[Header("Climb Settings")]
	[Tooltip("Speed to move up/down the rope in units/sec")]
	public float climbSpeed = 3f;

	[Tooltip("How fast the player snaps horizontally to the rope when starting to climb")]
	public float snapSpeed = 20f;

	private Rigidbody _rb;
	private CapsuleCollider _col;

	// Set of rope segments the player is currently touching
	private readonly HashSet<RopeJoint> _ropeContacts = new HashSet<RopeJoint>();

	private Vector3 _ropeAnchorXZ;

	// remember gravity and constraints
	private RigidbodyConstraints _savedConstraints;
	private bool _savedUseGravity;

	private void Start()
	{
		_rb = GetComponent<Rigidbody>();
		_col = GetComponent<CapsuleCollider>();

		_savedConstraints = _rb.constraints;
		_savedUseGravity = _rb.useGravity;
	}

	private void OnDisable()
	{
		// restore physics state
		if (_rb != null)
		{
			_rb.constraints = _savedConstraints;
			_rb.useGravity = _savedUseGravity;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		// detect rope segments by RopeJoint component
		var rope = other.GetComponentInParent<RopeJoint>();
		if (rope == null) return;

		if (_ropeContacts.Add(rope))
		{
			UpdateRopeAnchor();
			// if this is the first contact, engage climbing
			if (_ropeContacts.Count == 1)
			{
				StartClimbing();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		var rope = other.GetComponentInParent<RopeJoint>();
		if (rope == null) return;

		if (_ropeContacts.Remove(rope))
		{
			UpdateRopeAnchor();
			if (_ropeContacts.Count == 0)
			{
				StopClimbing();
			}
		}
	}

	private void UpdateRopeAnchor()
	{
		// average the XZ coordinates of all contacts
		if (_ropeContacts.Count == 0) return;

		Vector2 accum = Vector2.zero;
		foreach (var r in _ropeContacts)
		{
			Vector3 p = r.transform.position;
			accum += new Vector2(p.x, p.z);
		}

		accum /= _ropeContacts.Count;
		_ropeAnchorXZ = new Vector3(accum.x, 0f, accum.y);
	}

	private void StartClimbing()
	{
		// Snap horizontally to the rope anchor and disable gravity
		_rb.useGravity = false;
        _rb.isKinematic = true;
		_rb.linearVelocity = Vector3.zero;
		// Freeze X/Z position so player can't move away horizontally; allow Y movement
		_rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
		// Block other movement from the GameManager so lateral player controls are disabled
		if (GameManager.Instance != null) GameManager.Instance.BlockMovement();
	}

	private void StopClimbing()
	{
		_rb.useGravity = _savedUseGravity;
        _rb.isKinematic = false;
		_rb.constraints = _savedConstraints;
		// Release movement block
		if (GameManager.Instance != null) GameManager.Instance.ReleaseMovementBlock();
	}

	private float _verticalInput;

	/// <summary>
	/// Optional API for the new Input System (if you prefer to forward values from your input handler).
	/// Call this every frame from your input logic with a value in range [-1,1].
	/// </summary>
	public void SetClimbInput(float vertical)
	{
		_verticalInput = Mathf.Clamp(vertical, -1f, 1f);
	}

	/// <summary>
	/// Whether the player is currently attached to the rope.
	/// </summary>
	public bool IsClimbing => _ropeContacts.Count > 0;

	private void FixedUpdate()
	{
		if (_ropeContacts.Count == 0) return;

		// Get vertical input. Default reads old input system; override by calling SetClimbInput for the new system.
		float vertical = _verticalInput != 0f ? _verticalInput : Input.GetAxis("Vertical");

		// Horizontal snap
		Vector3 current = transform.position;
		Vector3 targetXZ = new Vector3(_ropeAnchorXZ.x, current.y, _ropeAnchorXZ.z);
		Vector3 newPos = Vector3.MoveTowards(current, targetXZ, snapSpeed * Time.fixedDeltaTime);

		// Vertical movement along rope
		float dy = vertical * climbSpeed * Time.fixedDeltaTime;
		newPos.y = current.y + dy;

		_rb.MovePosition(newPos);
	}

	private void Update()
	{
		// If climbing and press Jump, release rope
		if (_ropeContacts.Count > 0 && Input.GetButtonDown("Jump"))
		{
			_ropeContacts.Clear();
			StopClimbing();
		}
	}
}
