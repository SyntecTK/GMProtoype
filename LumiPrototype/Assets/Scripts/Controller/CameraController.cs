using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float smoothTime;
    [SerializeField] private float cameraOffsetX = 5f;
    PlayerAnimationController player;
    Vector3 basePosition;
    Vector3 velocity = Vector3.zero;

    private void Start()
    {
        player = FindAnyObjectByType<PlayerAnimationController>();
        basePosition = transform.position;
    }

    private void Update()
    {
        FollowPlayer();
    }
    private void FollowPlayer()
    {
        Vector3 targetPosition = new Vector3(player.transform.position.x + cameraOffsetX, basePosition.y, basePosition.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
