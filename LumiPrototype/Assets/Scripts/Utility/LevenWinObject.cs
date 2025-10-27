using UnityEngine;

public class LevenWinObject : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.LevelClearedInvoke();
        }
    }
}
