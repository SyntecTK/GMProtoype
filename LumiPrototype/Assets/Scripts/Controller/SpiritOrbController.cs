using UnityEngine;
using UnityEngine.InputSystem;

public class SpiritorbController : MonoBehaviour
{
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
        if (currentState != OrbitState.Attacking) return;

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Hit enemy!");
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.TakeDamage();

            StartReturn();
        }
    }

    private enum OrbitState
    {
        Orbiting,
        Attacking,
        Returning
    }
}
