using UnityEngine;


public class Draggable : MonoBehaviour
{
    private Rigidbody _rb;
    private bool _isDragging;
    private Transform _player;
    private Vector3 _playerPrevPos;
    private Vector3 _offsetFromPlayer;
    private float _startY;

    [Header("Drag Options")]
    [Tooltip("If true, the object keeps its original world Y position while being dragged.")]
    public bool maintainY = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Start dragging, the object will follow the player's movement delta so it moves at the same speed as the player.
    /// </summary>
    /// <param name="player">Player transform (used to compute delta movement)</param>
    public void StartDrag(Transform player)
    {
        _isDragging = true;
        _rb.isKinematic = true;
        _player = player;
        _playerPrevPos = player.position;
        _offsetFromPlayer = transform.position - player.position;
        _startY = transform.position.y;
    }

    public void StopDrag()
    {
        _isDragging = false;
        _rb.isKinematic = false;
        _player = null;
    }

    private void Update()
    {
        if (!_isDragging || _player == null) return;

        // Move by the same delta the player moved this frame on XZ plane; keep own Y if requested.
        Vector3 playerDelta = _player.position - _playerPrevPos;
        // ignore vertical movement from the player when dragging
        playerDelta.y = 0f;

        Vector3 newPos = transform.position + playerDelta;
        if (maintainY)
        {
            newPos.y = _startY;
        }

        transform.position = newPos;
        _playerPrevPos = _player.position;

    }
}

