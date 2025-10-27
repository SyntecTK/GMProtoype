using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public enum EnemyType { Shield, Jumper }
    [Header("Settings")]
    public EnemyType enemyType;
    [SerializeField] private float damage = 2f;
    [SerializeField] private float maxHealth = 3f;


    [Header ("Dependencies")]
    public GameObject eyesOpen;
    public GameObject eyesClosed;

    public GameObject parryObject;

    [Header("Jumper Settings")]
    public float jumpForce = 1.2f;          // Sprunghöhe
    public float jumpDistance = 2f;       // Vorwärtsdistanz pro Sprung
    public float jumpInterval = 1.6f;     // Zeit zwischen Sprüngen
    public float sideOffset = 1f;         // seitlicher Versatz links/rechts
    public float flashDuration = 0.1f;

    [Header ("Colors")]
    public Color vulnerableColor = Color.blue;
    public Color shieldColor = Color.red;
    public Color hitFlashColor = Color.white;
    public Color parryColor = Color.orange;

    [Header("Shield Enemy Settings")]
    public float shieldMinInterval = 2f; // Minimale Invulnerable-Zeit
    public float shieldMaxInterval = 3f; // Maximale Invulnerable-Zeit
    public float vulnerableDuration = 1f; // Fix 1 Sekunde verwundbar

    private Rigidbody rb;
    private Renderer rend;
    private Color currentColor;
    private bool isInvulnerable = false;
    private bool isFlashing = false;
    private bool jumpLeft = true;
    public bool canHop = true;

    //health and damage
    private float currentHealth;
    public bool canAttack = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();

        currentColor = vulnerableColor;
        rend.material.color = currentColor;

        currentHealth = maxHealth;

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

    public void TakeDamage(float damage)
    {
        if (isInvulnerable) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        if (!isFlashing)
        {
            StartCoroutine(FlashOnHit());
        }

        currentHealth -= damage;
        if(currentHealth <= 0)
        {
            Destroy(gameObject);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && canAttack)
        {
            Debug.Log("Player Take Damage");
            GameManager.Instance.DamagePlayer(damage);
        }
    }
    private IEnumerator HopRoutine()
    {
        while (true)
        {
            float rand = Random.value;
            bool doAttack = rand < 0.3f;
            if (canHop)
            {

                if(doAttack)
                {
                    Debug.Log("ATTACK");
                    GameManager.Instance.StartParryWindow();
                    rend.material.color = parryColor;
                    currentColor = parryColor;

                    Vector3 playerDir = (GameManager.Instance.GetPlayerPosition() - transform.position).normalized;
                    playerDir.y = 0f;

                    Vector3 jumpVec = Vector3.up * (jumpForce * 1.6f) + playerDir * (jumpDistance * 4f);

                    rb.linearVelocity = Vector3.zero;
                    rb.AddForce(jumpVec, ForceMode.VelocityChange);

                    yield return new WaitForSeconds(0.6f);
                    rb.linearVelocity = Vector3.zero;
                    rend.material.color = currentColor = isInvulnerable ? shieldColor : vulnerableColor;
                    GameManager.Instance.EndParryWindow();
                }
                else
                {
                    Vector3 forward = transform.right * jumpDistance;
                    Vector3 side = (jumpLeft ? -transform.right : transform.right) * sideOffset;
                    Vector3 jumpTarget = transform.position + forward + side;

                    Vector3 jumpVector = jumpTarget - transform.position;
                    jumpVector.y = jumpForce; 

                    rb.linearVelocity = Vector3.zero; 
                    rb.AddForce(jumpVector, ForceMode.VelocityChange);

                    jumpLeft = !jumpLeft;
                    Debug.Log("JUMP");
                }
            }
            yield return new WaitForSeconds(jumpInterval);
        }
    }
}
