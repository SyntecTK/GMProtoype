using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using NUnit.Framework;

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

    [Header("Jumper Settings")]
    public float jumpForce = 1.2f;          // Sprungh�he
    public float jumpDistance = 2f;       // Vorw�rtsdistanz pro Sprung
    public float jumpInterval = 1.6f;     // Zeit zwischen Spr�ngen
    public float sideOffset = 1f;         // seitlicher Versatz links/rechts
    public float flashDuration = 0.1f;

    [Header ("Colors")]
    public Color vulnerableColor = Color.blue;
    public Color shieldColor = Color.red;
    public Color hitFlashColor = Color.white;
    public Color parryColor = Color.orange;

    [Header("Shield Enemy Settings")]
    public float shieldMinInterval = 1f; // Minimale Invulnerable-Zeit
    public float shieldMaxInterval = 1.5f; // Maximale Invulnerable-Zeit
    public float vulnerableDuration = 3f; // Fix 1 Sekunde verwundbar

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

    private bool startedBehaviour = false;
    [Header("Activation")]
    [Tooltip("How close the player must be before this enemy starts its behaviours.")]
    [SerializeField] private float activationRange = 10f;

    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();

        currentColor = vulnerableColor;
        rend.material.color = currentColor;

        currentHealth = maxHealth;

        // Don't automatically start behaviour coroutines here. These coroutines
        // should start when the player is near (to improve performance) — we'll
        // trigger them from Update() once per enemy.

    }

    private void Update()
    {
        // Activation: start coroutines the first time the player enters activationRange
        if (!startedBehaviour)
        {
            Vector3 playerPos = GameManager.Instance.GetPlayerPosition();
            float distance = Vector3.Distance(transform.position, playerPos);
            if (distance <= activationRange)
            {
                startedBehaviour = true;
                // Jumper enemies run both hop and shield cycles
                if (enemyType == EnemyType.Jumper)
                {
                    StartCoroutine(HopRoutine());
                    StartCoroutine(ShieldCycleRoutine());
                }
                // Shield-only enemies need only the shield routine
                else if (enemyType == EnemyType.Shield)
                {
                    StartCoroutine(ShieldCycleRoutine());
                }
            }
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
            if (!isAttacking)
            {
                SetVulnerable(true);
                yield return new WaitForSeconds(vulnerableDuration);

                float invulnerableTime = Random.Range(shieldMinInterval, shieldMaxInterval);
                SetVulnerable(false);
                yield return new WaitForSeconds(invulnerableTime);
            }
            else
            {
                // If the enemy is currently attacking, we must yield to avoid a busy CPU loop.
                // The coroutine will check again next frame to resume the shield cycle.
                yield return null;
            }
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
                    isAttacking = true;
                    Debug.Log("ATTACK");
                    GameManager.Instance.StartParryWindow();
                    rend.material.color = parryColor;
                    currentColor = parryColor;

                    Vector3 playerDir = (GameManager.Instance.GetPlayerPosition() - transform.position).normalized;
                    playerDir.y = 0f;

                    Vector3 jumpVec = Vector3.up * (jumpForce * 1.6f) + playerDir * (jumpDistance * 4f);

                    yield return new WaitForSeconds(1f);

                    rb.linearVelocity = Vector3.zero;
                    rb.AddForce(jumpVec, ForceMode.VelocityChange);

                    yield return new WaitForSeconds(2f);
                    rb.linearVelocity = Vector3.zero;
                    rend.material.color = currentColor = isInvulnerable ? shieldColor : vulnerableColor;
                    GameManager.Instance.EndParryWindow();
                    isAttacking = false;
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
