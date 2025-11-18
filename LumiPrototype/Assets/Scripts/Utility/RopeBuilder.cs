using UnityEngine;

public class RopeBuilder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform parentContainer;

    [Header("Rope Settings")]
    [SerializeField] private int segmentCount = 10;
    [SerializeField] private float segmentSpacing = 0.5f;

    private void Start()
    {
        Rigidbody previousRb = null;
        RopeJoint previousJoint = null;
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject segment = Instantiate(segmentPrefab, startPoint.position + Vector3.down * i * segmentSpacing, Quaternion.identity);
            Rigidbody rb = segment.GetComponent<Rigidbody>();
            HingeJoint joint = segment.GetComponent<HingeJoint>();
            RopeJoint ropeJoint = segment.GetComponent<RopeJoint>();
            segment.transform.parent = parentContainer;

            if(i == 0)
            {
                joint.connectedBody = startPoint.GetComponent<Rigidbody>();
            }
            else
            {
                joint.connectedBody = previousRb;
            }

            if(previousJoint != null)
            {
                previousJoint.below = rb;
                ropeJoint.above = previousJoint.GetComponent<Rigidbody>();
            }

            previousRb = rb;
        }
    }
}
