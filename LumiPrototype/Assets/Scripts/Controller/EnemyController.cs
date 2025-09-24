using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public float jumpForce = 2f;
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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();

        currentColor = vulnerableColor;
        rend.material.color = currentColor;

        StartCoroutine(ShieldCycleRoutine());
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
        yield return new WaitForSeconds(flashDuration);

        rend.material.color = currentColor;
        isFlashing = false;
    }

    private IEnumerator ShieldCycleRoutine()
    {
        while (true)
        {
            // Zuerst verwundbar für eine Sekunde
            SetVulnerable(true);
            yield return new WaitForSeconds(vulnerableDuration);

            // Dann Invulnerable für random Zeit
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
}
