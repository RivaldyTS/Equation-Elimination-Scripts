using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum AIState { Patrol, Attack, Search }
    public AIState currentState = AIState.Patrol;

    public GameObject pathObject;
    public Transform player;
    public float sightRange = 10f;
    public float attackRange = 5f;
    public float searchDuration = 5f;
    public float lookAroundDuration = 3f;
    public float fovAngle = 90f;
    public float shootCooldown = 1f;
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float bulletForce = 20f;
    public float bulletSpread = 5f;
    public AudioClip spotSound;
    public AudioClip shootSound;
    public AudioClip searchSound;
    public AudioClip damageSound;
    public AudioClip deathSound;
    public GameObject[] deathEffectPrefabs;

    [Header("Attack State Settings")]
    public bool facePlayerImmediately = true;
    public float rotationSpeed = 5f;
    public float initialShootDelay = 1f;

    [Header("Patrol Settings")]
    public float pauseDuration = 1.5f;
    public float patrolLookSpeed = 90f;
    public float patrolLookAngle = 45f;
    public float pointOffset = 0.5f;
    public float stuckThreshold = 2f;

    [Header("Alert Settings")]
    public float alertRadius = 10f;

    [Header("Phantom Echo Settings")]
    public GameObject phantomPrefab;
    public int maxPhantoms = 1;
    public float phantomLifetime = 1f;
    public float phantomSpawnCooldown = 5f;
    public float phantomSpawnDelay = 0.5f;
    public AudioClip phantomSpawnSound;
    private float phantomCooldownTimer = 0f;

    [Header("Health Settings")]
    public float maxHealth = 200f; // Ensure this is declared
    private float currentHealth;

    private NavMeshAgent agent;
    private Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    private Vector3 lastKnownPlayerPosition;
    private float searchTimer = 0f;
    private float shootTimer = 0f;
    private float lookAroundTimer = 0f;
    private AudioSource audioSource;
    private bool playerInSight = false;
    private bool isLookingAround = false;
    private bool wasHit = false;
    private bool isPaused = false;
    private float pauseTimer = 0f;
    private bool hasSpottedPlayer = false;
    private float initialShootTimer = 0f;
    private float patrolLookTimer = 0f;
    private Quaternion initialRotation;
    private bool hasLookedAround = false;
    private float timeAtPoint = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth; // Line 78 - Now maxHealth should be recognized

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (pathObject != null)
        {
            EnemyPath path = pathObject.GetComponent<EnemyPath>();
            if (path != null && path.waypoints.Count > 0)
            {
                patrolPoints = path.waypoints.ToArray();
                currentPatrolIndex = Random.Range(0, patrolPoints.Length);
                SetDestinationWithOffset(currentPatrolIndex);
            }
        }
    }

    void Update()
    {
        playerInSight = CanSeePlayer();
        phantomCooldownTimer -= Time.deltaTime;

        switch (currentState)
        {
            case AIState.Patrol:
                Patrol();
                break;
            case AIState.Attack:
                Attack();
                break;
            case AIState.Search:
                Search();
                break;
        }

        if (shootTimer > 0)
        {
            shootTimer -= Time.deltaTime;
        }

        AvoidOtherEnemies();
    }

    public void OnHitByPlayer()
    {
        wasHit = true;
        lastKnownPlayerPosition = player.position;
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (deathEffectPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, deathEffectPrefabs.Length);
            Instantiate(deathEffectPrefabs[randomIndex], transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    void Patrol()
    {
        if (wasHit)
        {
            currentState = AIState.Attack;
            wasHit = false;
            isPaused = false;
            agent.isStopped = false;
            return;
        }

        if (playerInSight)
        {
            lastKnownPlayerPosition = player.position;
            currentState = AIState.Attack;
            PlaySound(spotSound);
            isPaused = false;
            agent.isStopped = false;
            hasSpottedPlayer = true;
            initialShootTimer = initialShootDelay;
            AlertNearbyEnemies();
            StartCoroutine(SpawnPhantomEchoesWithDelay());
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (!isPaused)
            {
                isPaused = true;
                pauseTimer = pauseDuration;
                agent.isStopped = true;
                initialRotation = transform.rotation;
                patrolLookTimer = 0f;
                hasLookedAround = false;
                timeAtPoint = 0f;
            }
            else
            {
                pauseTimer -= Time.deltaTime;
                timeAtPoint += Time.deltaTime;

                if (!hasLookedAround)
                {
                    patrolLookTimer += Time.deltaTime;
                    float lookDuration = (2 * patrolLookAngle) / patrolLookSpeed;
                    float lookProgress = Mathf.Clamp01(patrolLookTimer / lookDuration);

                    if (lookProgress < 0.5f)
                    {
                        float angle = Mathf.Lerp(0f, patrolLookAngle, lookProgress * 2f);
                        transform.rotation = initialRotation * Quaternion.Euler(0, angle, 0);
                    }
                    else
                    {
                        float angle = Mathf.Lerp(patrolLookAngle, -patrolLookAngle, (lookProgress - 0.5f) * 2f);
                        transform.rotation = initialRotation * Quaternion.Euler(0, angle, 0);
                    }

                    if (lookProgress >= 1f)
                    {
                        hasLookedAround = true;
                    }
                }

                if (pauseTimer <= 0f || (hasLookedAround && pauseTimer <= 0.1f) || timeAtPoint >= (pauseDuration + stuckThreshold))
                {
                    MoveToNextPoint();
                }
            }
        }
    }

    void AlertNearbyEnemies()
    {
        EnemyAI[] allEnemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in allEnemies)
        {
            if (enemy != this && enemy.currentState != AIState.Attack)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= alertRadius)
                {
                    enemy.currentState = AIState.Search;
                    enemy.lastKnownPlayerPosition = lastKnownPlayerPosition;
                    enemy.PlaySound(enemy.searchSound);
                }
            }
        }
    }

    IEnumerator SpawnPhantomEchoesWithDelay()
    {
        yield return new WaitForSeconds(phantomSpawnDelay);
        SpawnPhantomEchoes();
    }

    void SpawnPhantomEchoes()
    {
        if (phantomPrefab == null)
        {
            return;
        }
        if (phantomCooldownTimer > 0f)
        {
            return;
        }

        phantomCooldownTimer = phantomSpawnCooldown;
        Vector3 basePosition = transform.position;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            basePosition.y = col.bounds.min.y;
        }

        Vector3 rightOffset = transform.right * 1f;
        Vector3 leftOffset = -transform.right * 1f;
        Vector3[] spawnPositions = { basePosition + rightOffset, basePosition + leftOffset };

        if (phantomSpawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(phantomSpawnSound);
        }

        for (int i = 0; i < maxPhantoms; i++)
        {
            GameObject phantom = Instantiate(phantomPrefab, spawnPositions[i], transform.rotation);
            PhantomEcho echo = phantom.GetComponent<PhantomEcho>();
            if (echo != null)
            {
                Vector3 offset = (i == 0 ? transform.right : -transform.right) * 2f;
                echo.RushPlayer(player, lastKnownPlayerPosition, phantomLifetime, offset);
            }
        }
    }

    void MoveToNextPoint()
    {
        if (Random.value < 0.7f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
        else
        {
            int newIndex = Random.Range(0, patrolPoints.Length);
            while (newIndex == currentPatrolIndex && patrolPoints.Length > 1)
            {
                newIndex = Random.Range(0, patrolPoints.Length);
            }
            currentPatrolIndex = newIndex;
        }
        SetDestinationWithOffset(currentPatrolIndex);
        isPaused = false;
        agent.isStopped = false;
    }

    void SetDestinationWithOffset(int index)
    {
        Vector3 basePoint = patrolPoints[index].position;
        Vector3 offset = Random.insideUnitSphere * pointOffset;
        offset.y = 0;
        Vector3 targetPoint = basePoint + offset;
        agent.SetDestination(targetPoint);
    }

    void AvoidOtherEnemies()
    {
        EnemyAI[] allEnemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in allEnemies)
        {
            if (enemy != this && currentState == AIState.Patrol && Vector3.Distance(transform.position, enemy.transform.position) < 1f)
            {
                MoveToNextPoint();
            }
        }
    }

    void Attack()
    {
        agent.isStopped = true;
        FacePlayer();

        if (playerInSight && Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            if (hasSpottedPlayer)
            {
                initialShootTimer -= Time.deltaTime;
                if (initialShootTimer <= 0f)
                {
                    hasSpottedPlayer = false;
                }
            }
            else if (shootTimer <= 0)
            {
                Shoot();
                shootTimer = shootCooldown;
            }
        }
        else if (!playerInSight)
        {
            currentState = AIState.Search;
            agent.isStopped = false;
            SetSearchDestination(lastKnownPlayerPosition);
            PlaySound(searchSound);
            hasSpottedPlayer = false;
        }
    }

    void Search()
    {
        if (wasHit)
        {
            SetSearchDestination(lastKnownPlayerPosition);
            wasHit = false;
        }

        if (!isLookingAround)
        {
            agent.isStopped = false;
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                isLookingAround = true;
                lookAroundTimer = 0f;
                searchTimer = 0f;
            }
        }
        else
        {
            lookAroundTimer += Time.deltaTime;
            searchTimer += Time.deltaTime;
            LookAround();

            if (searchTimer >= lookAroundDuration && Vector3.Distance(transform.position, player.position) > sightRange)
            {
                isLookingAround = false;
                currentState = AIState.Patrol;
                searchTimer = 0f;
                if (patrolPoints != null && patrolPoints.Length > 0)
                {
                    SetDestinationWithOffset(currentPatrolIndex);
                }
            }
            else if (searchTimer >= searchDuration)
            {
                isLookingAround = false;
                currentState = AIState.Patrol;
                searchTimer = 0f;
                if (patrolPoints != null && patrolPoints.Length > 0)
                {
                    SetDestinationWithOffset(currentPatrolIndex);
                }
            }
        }

        if (playerInSight)
        {
            lastKnownPlayerPosition = player.position;
            currentState = AIState.Attack;
            PlaySound(spotSound);
            hasSpottedPlayer = true;
            initialShootTimer = initialShootDelay;
            AlertNearbyEnemies();
            StartCoroutine(SpawnPhantomEchoesWithDelay());
            return;
        }
    }

    void SetSearchDestination(Vector3 targetPosition)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            float minDistance = float.MaxValue;
            int closestIndex = currentPatrolIndex;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                float distance = Vector3.Distance(transform.position, patrolPoints[i].position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }
            SetDestinationWithOffset(closestIndex);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (currentState == AIState.Patrol && collision.gameObject.GetComponent<EnemyAI>() != null)
        {
            MoveToNextPoint();
        }
    }

    void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;

        if (facePlayerImmediately)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        }
        else
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void LookAround()
    {
        float lookSpeed = 1f;
        transform.Rotate(0, lookSpeed, 0);
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer < fovAngle / 2 && directionToPlayer.magnitude <= sightRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, sightRange))
            {
                if (hit.transform == player)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void Shoot()
    {
        Vector3 shootDirection = (player.position - shootPoint.position).normalized;
        shootDirection = Quaternion.Euler(
            Random.Range(-bulletSpread, bulletSpread),
            Random.Range(-bulletSpread, bulletSpread),
            0
        ) * shootDirection;

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.LookRotation(shootDirection));
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

        if (bulletRb != null)
        {
            bulletRb.linearVelocity = shootDirection * bulletForce;
        }

        PlaySound(shootSound);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, alertRadius);

        Vector3 fovLine1 = Quaternion.AngleAxis(fovAngle / 2, Vector3.up) * transform.forward * sightRange;
        Vector3 fovLine2 = Quaternion.AngleAxis(-fovAngle / 2, Vector3.up) * transform.forward * sightRange;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);
    }
}