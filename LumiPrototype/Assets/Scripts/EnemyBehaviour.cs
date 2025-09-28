using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Hier kann die Logik für den Kontakt mit dem Spieler implementiert werden

        }
    }
}
