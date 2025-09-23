using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public float jumpForce = 2f;
    public float flashDuration = 0.1f;

    public Color vulnerableColor = Color.blue;
    public Color shieldColor = Color.red;
    public Color hitFlashColor = Color.white;

    public float shieldMinInterval = 2f;
    public float shieldMaxInterval = 3f;

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

        StartCoroutine(ShieldToggleRoutine());
    }

    public void TakeDamage()
    {
        if (isInvulnerable) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

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

    private IEnumerator ShieldToggleRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(shieldMinInterval, shieldMaxInterval);
            yield return new WaitForSeconds(waitTime);

            ToggleShield();
        }
    }

    private void ToggleShield()
    {
        isInvulnerable = !isInvulnerable;

        if (isInvulnerable)
        {
            currentColor = shieldColor;
        }
        else
        {
            currentColor = vulnerableColor;
        }

        // Nur Farbe wechseln, wenn nicht gerade geflasht wird
        if (!isFlashing)
        {
            rend.material.color = currentColor;
        }
    }
}
