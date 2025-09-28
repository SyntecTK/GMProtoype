using UnityEngine;
using System.Collections;

public class Hitbox : MonoBehaviour
{
    private PlayerController playerController;
    private Collider hitboxCollider;

    [SerializeField] private GameObject openEyes;
    [SerializeField] private GameObject hitEyes;

    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        hitboxCollider = GetComponent<Collider>();
        openEyes.SetActive(true);
        hitEyes.SetActive(false);
        // Hitbox standardmäßig deaktivieren
        hitboxCollider.enabled = false;
    }

    // Diese Methoden können über Animation Events aufgerufen werden
    public void EnableHitbox()
    {
        hitboxCollider.enabled = true;
        Debug.Log("Hitbox activated");
    }

    public void Disable()
    {
        hitboxCollider.enabled = false;
        Debug.Log("Hitbox deactivated");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // PlayerController über den Hit informieren und Gegner-Reference übergeben
            playerController?.OnEnemyHit(other.gameObject);
            Debug.Log($"Enemy hit: {other.name}");

            // Farbwechsel für 0.5 Sekunden
            StartCoroutine(ChangeColorTemporarily(other));
        }
    }

    private IEnumerator ChangeColorTemporarily(Collider enemy)
    {
        Renderer renderer = enemy.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
            hitEyes.SetActive(true);
            openEyes.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            renderer.material.color = Color.white;
            yield return new WaitForSeconds(0.4f);
            hitEyes.SetActive(false);
            openEyes.SetActive(true);
        }

    }
}
