using UnityEngine;

public class CameraPan : MonoBehaviour
{

    private Vector3 movePosition = new Vector3(1, 0, 0);
    private Vector3 startPosition;
    private bool forward = true;
    private float speed = 2;
    private void Start()
    {
        startPosition = transform.position;
    }
    void Update()
    {
        
        if(transform.position.x >= startPosition.x + 5)
        {
            forward = false;
        }
        else if(transform.position.x <= startPosition.x - 5)
        {
            forward = true;
        }

        if (forward) transform.position += movePosition * Time.deltaTime * speed;
        else transform.position -= movePosition * Time.deltaTime * speed;
    }
}
