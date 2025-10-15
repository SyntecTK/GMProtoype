using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public enum EnemyType { Shield, Jumper }

    public EnemyType enemyType;

    public GameObject eyesOpen;
    public GameObject eyesClosed;

    public GameObject parryObject;

    public float jumpForce = 2f;          // Sprunghöhe
    public float jumpDistance = 2f;       // Vorwärtsdistanz pro Sprung
    public float jumpInterval = 0.8f;     // Zeit zwischen Sprüngen
    public float sideOffset = 1f;         // seitlicher Versatz links/rechts
    public float flashDuration = 0.1f;

    public Color vulnerableColor = Color.blue;
    public Color shieldColor = Color.red;
    public Color hitFlashColor = Color.white;

    public float shieldMinInterval = 2f; // Minimale Invulnerable-Zeit
    public float shieldMaxInterval = 3f; // Maximale Invulnerable-Zeit
    public float vulnerableDuration = 1f; // Fix 1 Sekunde verwundbar

    private Rigidbody rb;
    private Renderer rend;
    private Color currentColor;
    private bool isInvulnerable = false;
    private bool isFlashing = false;
    private bool jumpLeft = true; // toggle für seitliche Sprünge
    public bool canHop = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();

        currentColor = vulnerableColor;
        rend.material.color = currentColor;

        if (enemyType == EnemyType.Shield)
        {
            StartCoroutine(ShieldCycleRoutine());
        }
        else if (enemyType == EnemyType.Jumper)
        {
            StartCoroutine(HopRoutine());
            StartCoroutine(ShieldCycleRoutine());
        }
    }

    public void TakeDamage()
    {
        if (isInvulnerable) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        GameManager.Instance.GainEnergy(10f);

        if (!isFlashing)
        {
            StartCoroutine(FlashOnHit());
        }
    }

    private IEnumerator FlashOnHit()
    {
        isFlashing = true;
        Color original = rend.material.color;

        rend.material.color = hitFlashColor;
        eyesOpen.SetActive(false);
        eyesClosed.SetActive(true);
        yield return new WaitForSeconds(flashDuration);

        eyesOpen.SetActive(true);
        eyesClosed.SetActive(false);
        rend.material.color = currentColor;
        isFlashing = false;
    }

    private IEnumerator ShieldCycleRoutine()
    {
        while (true)
        {
            SetVulnerable(true);
            yield return new WaitForSeconds(vulnerableDuration);

            float invulnerableTime = Random.Range(shieldMinInterval, shieldMaxInterval);
            SetVulnerable(false);
            yield return new WaitForSeconds(invulnerableTime);
        }
    }

    private void SetVulnerable(bool vulnerable)
    {
        isInvulnerable = !vulnerable;
        currentColor = vulnerable ? vulnerableColor : shieldColor;

        if (!isFlashing)
        {
            rend.material.color = currentColor;
        }
    }

    private IEnumerator ParryCycleRoutine()
    {
        while (true)
        {
            ActivateParryWindow();
            yield return new WaitForSeconds(vulnerableDuration);
            DeactivateParryWindow();
            yield return new WaitForSeconds(Random.Range(shieldMinInterval, shieldMaxInterval));
        }
    }

    private void ActivateParryWindow()
    {
        parryObject.SetActive(true);
    }

    private void DeactivateParryWindow()
    {
        parryObject.SetActive(false);
    }

    // Neue Methode für hüpfendes Verhalten
    private IEnumerator HopRoutine()
    {
        while (true)
        {
            if (canHop)
            {
                Vector3 forward = transform.right * jumpDistance;
                Vector3 side = (jumpLeft ? -transform.right : transform.right) * sideOffset;
                Vector3 jumpTarget = transform.position + forward + side;

                Vector3 jumpVector = jumpTarget - transform.position;
                jumpVector.y = jumpForce; // Y-Komponente für Höhe

                rb.linearVelocity = Vector3.zero; // alte Geschwindigkeit löschen
                rb.AddForce(jumpVector, ForceMode.VelocityChange);

                jumpLeft = !jumpLeft; // nächste Richtung wechseln
            }
            yield return new WaitForSeconds(jumpInterval);
        }
    }
}
