using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class OrbStateController : MonoBehaviour
{
    private enum OrbitState
    {
        Orbiting,
        Attacking,
        Parrying,
        Returning
    }

    [Header("Orbit Settings")]
    public Transform orbitTarget;     // Der Charakter
    public float orbitRadius = 2f;
    public float orbitSpeed = 90f;

    [Header("Bobbing Settings")]
    public float bobAmplitude = 0.3f;
    public float bobFrequency = 1f;

    [Header("Attack Settings")]
    public float attackSpeed = 10f;
    public float maxAttackDistance = 10f;
    public float returnSpeed = 12f;

    private Vector3 attackDirection;
    private Vector3 attackStartPosition;
    private OrbitState currentState = OrbitState.Orbiting;
    private float orbitAngle = 0f;

    [Header("ParrySettings")]
    [SerializeField] private float parryRadius = 3f;
    [SerializeField] private float parrySpeed = 360f;   // Grad pro Sekunde
    [SerializeField] private float parryDuration = 0.6f;
    [SerializeField] private float slowMoFactor = 0.3f;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float approachDuration = 0.15f;

    private float parryTimer;
    private float parryAngle;
    private Vector3 lastHitDirection;
    private float approachTimer = 0f;
    private bool isApproaching = false;

    private void Update()
    {
        switch (currentState)
        {
            case OrbitState.Orbiting:
                OrbitMovement();
                break;

            case OrbitState.Attacking:
                AttackMovement();
                break;

            case OrbitState.Parrying:
                ParryMovement();
                break;

            case OrbitState.Returning:
                ReturnToOrbit();
                break;
        }
    }

    private void OrbitMovement()
    {
        orbitAngle += orbitSpeed * Time.deltaTime;
        float radians = orbitAngle * Mathf.Deg2Rad;

        float x = Mathf.Cos(radians) * orbitRadius;
        float z = Mathf.Sin(radians) * orbitRadius;
        float y = Mathf.Sin(Time.time * bobFrequency * 2 * Mathf.PI) * bobAmplitude;

        transform.position = orbitTarget.position + new Vector3(x, y, z);
    }

    public void StartAttack()
    {
        if (currentState != OrbitState.Orbiting) return;

        currentState = OrbitState.Attacking;
        attackStartPosition = transform.position;
        attackDirection = orbitTarget.forward.normalized;
    }

    private void AttackMovement()
    {
        transform.position += attackDirection * attackSpeed * Time.deltaTime;

        float distanceTraveled = Vector3.Distance(attackStartPosition, transform.position);
        if (distanceTraveled >= maxAttackDistance)
        {
            StartReturn();
        }
    }

    private void StartReturn()
    {
        currentState = OrbitState.Returning;
    }

    private void ReturnToOrbit()
    {
        Vector3 targetPosition = orbitTarget.position + new Vector3(
            Mathf.Cos(orbitAngle * Mathf.Deg2Rad) * orbitRadius,
            Mathf.Sin(Time.time * bobFrequency * 2 * Mathf.PI) * bobAmplitude,
            Mathf.Sin(orbitAngle * Mathf.Deg2Rad) * orbitRadius
        );

        Vector3 dir = (targetPosition - transform.position).normalized;
        transform.position += dir * returnSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentState = OrbitState.Orbiting;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (currentState == OrbitState.Attacking)
            {
                Debug.Log("Hit enemy!");
                if (enemy != null)
                    enemy.TakeDamage();

                StartReturn();
            }
            else if (currentState == OrbitState.Parrying)
            {
                Debug.Log("Parried Enemy");
                StartCoroutine(HandleParryHit(enemy));
            }
        }
    }

    public void StartParry()
    {
        if (currentState != OrbitState.Orbiting) return;

        currentState = OrbitState.Parrying;
        parryTimer = parryDuration;

        approachTimer = 0f;
        isApproaching = true;

        Vector3 local = transform.position - orbitTarget.position;
        parryAngle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;

        Time.timeScale = slowMoFactor;
    }

    private void ParryMovement()
    {
        // Phase 1: Approach
        if (isApproaching)
        {
            approachTimer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(approachTimer / approachDuration);

            // Berechne Punkt auf der Umlaufbahn mit aktuellem parryAngle
            float rad = parryAngle * Mathf.Deg2Rad;
            Vector3 targetPos = orbitTarget.position + new Vector3(Mathf.Cos(rad) * parryRadius,
                                                                   Mathf.Sin(rad) * parryRadius,
                                                                   0f);

            transform.position = Vector3.Lerp(transform.position, targetPos, t);

            if (t >= 1f)
                isApproaching = false; // Approach abgeschlossen

            return; // noch keine Rotation
        }

        // Phase 2: Rotation auf der Umlaufbahn
        parryTimer -= Time.unscaledDeltaTime;
        parryAngle += parrySpeed * Time.unscaledDeltaTime;

        float radians = parryAngle * Mathf.Deg2Rad;

        float x = Mathf.Cos(radians) * parryRadius;
        float y = Mathf.Sin(radians) * parryRadius;
        float z = 0f;

        transform.position = orbitTarget.position + new Vector3(x, y, z);

        if (parryTimer <= 0f)
        {
            EndParry();
        }
    }

    private void EndParry()
    {
        Time.timeScale = 1f;
        currentState = OrbitState.Returning;
    }

    private IEnumerator HandleParryHit(EnemyController enemy)
    {
        Debug.Log("canHop = false??");
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        bool hadKinematic = false;
        enemy.canHop = false;

        // Temporärer Anchor am Player
        GameObject parryAnchor = new GameObject("ParryAnchor");
        parryAnchor.transform.position = orbitTarget.position;
        parryAnchor.transform.parent = orbitTarget;

        // Gegner an Anchor hängen
        Vector3 startLocalPos = enemy.transform.position - orbitTarget.position;
        enemy.transform.SetParent(parryAnchor.transform, true);

        if (rb != null)
        {
            hadKinematic = rb.isKinematic;
            rb.isKinematic = true; // wir kontrollieren die Position
        }

        enemy.GetComponent<CapsuleCollider>().enabled = false;

        // 1️⃣ Gegner smooth auf Orbit-Radius ziehen
        Vector3 initialLocalPos = enemy.transform.localPosition;
        Vector3 targetLocalPos = enemy.transform.localPosition.normalized * parryRadius; // gewünschter Abstand

        float t = 0f;
        float pullDuration = 0.2f; // Dauer des Ziehens
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / pullDuration;
            enemy.transform.localPosition = Vector3.Lerp(initialLocalPos, targetLocalPos, t);
            yield return null;
        }

        // 2️⃣ Gegner einmal um den Spieler rotieren (360°)
        float angle = 0f;
        float orbitSpeed = 360f; // Grad pro Sekunde
        Vector3 offset = enemy.transform.localPosition; // Start auf korrektem Radius

        while (angle < 360f)
        {
            float deltaAngle = orbitSpeed * Time.unscaledDeltaTime;
            angle += deltaAngle;

            offset = Quaternion.Euler(0f, deltaAngle, 0f) * offset; // Rotation um Y-Achse
            enemy.transform.localPosition = offset;

            yield return null;
        }

        // 3️⃣ Gegner nach vorne wegstoßen
        Vector3 pushDir = startLocalPos.normalized; // Richtung, in die Gegner rausgeschleudert wird
        float pushForce = 10f;

        enemy.transform.SetParent(null); // Anchor entfernen
        GameObject.Destroy(parryAnchor);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
            rb.isKinematic = hadKinematic;
        }
        else
        {
            // fallback: manuell verschieben
            float pushDuration = 0.2f;
            Vector3 pushStart = enemy.transform.position;
            Vector3 pushEnd = pushStart + pushDir * 2f;
            t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / pushDuration;
                enemy.transform.position = Vector3.Lerp(pushStart, pushEnd, t);
                yield return null;
            }
        }

        enemy.GetComponent<CapsuleCollider>().enabled = true;
        yield return new WaitForSeconds(2f);
        enemy.canHop = true;
    }

}
