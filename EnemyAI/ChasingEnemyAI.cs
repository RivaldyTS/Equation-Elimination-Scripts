using UnityEngine;
using UnityEngine.AI;

public class ChasingEnemyAI : MonoBehaviour
{
    public enum AIState { Patrol, Attack, Search, Stunned }
    public AIState currentState = AIState.Patrol;

    public GameObject pathObject;
    public Transform player;
    public float sightRange = 10f;
    public float attackRange = 2f;
    public float searchDuration = 5f;
    public float lookAroundDuration = 3f;
    public float fovAngle = 90f;
    public AudioClip spotSound;
    public AudioClip searchSound;
    public AudioClip hitSound;
    public AudioClip screamSound;
    public AudioClip deathSound;
    public GameObject[] deathEffectPrefabs;

    [Header("Stun Settings")]
    public float stunDuration = 2f;
    public float teleportRadiusMin = 5f;
    public float teleportRadiusMax = 10f;
    public float teleportInterval = 0.5f;
    public int teleportCount = 5;
    public float baseSpeed = 3.5f;
    public float flickerInterval = 0.3f;
    public float flickerDuration = 5f;

    [Header("Attack State Settings")]
    public bool facePlayerImmediately = true;
    public float rotationSpeed = 5f;

    [Header("Health Settings")]
    public float maxHealth = 200f;
    private float currentHealth;
    private float previousHealth;
    private bool hasBeenHit = false;

    private NavMeshAgent agent;
    private Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 originalPosition;
    private float searchTimer = 0f;
    private float lookAroundTimer = 0f;
    private float stunTimer = 0f;
    private float teleportTimer = 0f;
    private int currentTeleportCount = 0;
    private float flickerTimer = 0f;
    private float flickerDurationTimer = 0f;
    private AudioSource audioSource;
    private bool playerInSight = false;
    private bool isLookingAround = false;
    private bool wasHit = false;
    private bool isTeleporting = false;
    private GameObject alienChaserGameObject;
    private bool isVisible = true;
    private bool isFlickering = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        alienChaserGameObject = transform.Find("Alien Chaser")?.gameObject;
        if (alienChaserGameObject == null)
        {
            // Debug removed
        }

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
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
        }

        currentHealth = maxHealth;
        previousHealth = currentHealth;
        agent.speed = baseSpeed;
    }

    void Update()
    {
        if (currentHealth < previousHealth && !hasBeenHit)
        {
            OnHitByPlayer();
            hasBeenHit = true;
        }
        previousHealth = currentHealth;

        playerInSight = CanSeePlayer();

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
            case AIState.Stunned:
                Stunned();
                break;
        }
    }

    public void OnHitByPlayer()
    {
        wasHit = true;
        lastKnownPlayerPosition = player.position;
        originalPosition = transform.position;
        currentState = AIState.Stunned;
        stunTimer = 0f;
        PlaySound(hitSound);
        agent.isStopped = true;
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
        PlaySound(deathSound);
        foreach (GameObject effect in deathEffectPrefabs)
        {
            if (effect != null)
            {
                Instantiate(effect, transform.position, Quaternion.identity);
            }
        }
        Destroy(gameObject);
    }

    void Patrol()
    {
        if (wasHit)
        {
            currentState = AIState.Stunned;
            wasHit = false;
            return;
        }

        if (playerInSight)
        {
            lastKnownPlayerPosition = player.position;
            currentState = AIState.Attack;
            PlaySound(spotSound);
            flickerDurationTimer = 0f;
            isFlickering = true;
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    void Attack()
    {
        agent.isStopped = false;
        if (playerInSight)
        {
            FacePlayer();
        }
        agent.SetDestination(player.position);

        if (alienChaserGameObject != null)
        {
            if (isFlickering)
            {
                flickerDurationTimer += Time.deltaTime;
                if (flickerDurationTimer >= flickerDuration)
                {
                    isFlickering = false;
                    alienChaserGameObject.SetActive(true);
                    isVisible = true;
                }
                else
                {
                    flickerTimer += Time.deltaTime;
                    if (flickerTimer >= flickerInterval)
                    {
                        isVisible = !isVisible;
                        alienChaserGameObject.SetActive(isVisible);
                        flickerTimer = 0f;
                    }
                }
            }
            else if (!isVisible)
            {
                alienChaserGameObject.SetActive(true);
                isVisible = true;
            }
        }

        if (!playerInSight && Vector3.Distance(transform.position, lastKnownPlayerPosition) < 0.5f)
        {
            currentState = AIState.Search;
            agent.speed = baseSpeed;
            flickerTimer = 0f;
            if (alienChaserGameObject != null)
            {
                alienChaserGameObject.SetActive(true);
                isVisible = true;
            }
            PlaySound(searchSound);
        }
    }

    void Search()
    {
        if (wasHit)
        {
            agent.SetDestination(lastKnownPlayerPosition);
            currentState = AIState.Stunned;
            wasHit = false;
            return;
        }

        if (playerInSight)
        {
            lastKnownPlayerPosition = player.position;
            currentState = AIState.Attack;
            PlaySound(spotSound);
            flickerDurationTimer = 0f;
            isFlickering = true;
            return;
        }

        if (alienChaserGameObject != null && !isVisible)
        {
            alienChaserGameObject.SetActive(true);
            isVisible = true;
        }

        agent.isStopped = false;
        FaceLastKnownPosition();
        agent.SetDestination(lastKnownPlayerPosition);

        searchTimer += Time.deltaTime;
        if (!isLookingAround && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            isLookingAround = true;
            lookAroundTimer = 0f;
        }
        else if (isLookingAround)
        {
            lookAroundTimer += Time.deltaTime;
            LookAround();
            if (lookAroundTimer >= lookAroundDuration || searchTimer >= searchDuration)
            {
                isLookingAround = false;
                currentState = AIState.Patrol;
                searchTimer = 0f;
                if (patrolPoints != null && patrolPoints.Length > 0)
                {
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                }
            }
        }
    }

    void Stunned()
    {
        if (!isTeleporting)
        {
            stunTimer += Time.deltaTime;
            FacePlayer(); // Always face the player during stun, regardless of sight

            if (stunTimer >= stunDuration)
            {
                PlaySound(screamSound);
                isTeleporting = true;
                teleportTimer = 0f;
                currentTeleportCount = 0;
            }
        }
        else
        {
            teleportTimer += Time.deltaTime;
            if (teleportTimer >= teleportInterval)
            {
                if (currentTeleportCount < teleportCount)
                {
                    Vector2 randomCircle = Random.insideUnitCircle * (teleportRadiusMax - teleportRadiusMin);
                    Vector3 teleportOffset = new Vector3(randomCircle.x, 0, randomCircle.y);
                    float randomDistance = Random.Range(teleportRadiusMin, teleportRadiusMax);
                    Vector3 teleportTarget = transform.position + teleportOffset.normalized * randomDistance;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(teleportTarget, out hit, teleportRadiusMax, NavMesh.AllAreas))
                    {
                        transform.position = hit.position;
                        FacePlayer(); // Face the player after each teleport
                    }
                    currentTeleportCount++;
                    teleportTimer = 0f;
                }
                else
                {
                    transform.position = originalPosition;
                    FacePlayer(); // Face the player after returning to original position
                    currentState = AIState.Attack;
                    agent.isStopped = false;
                    agent.speed = baseSpeed * 2f;
                    agent.SetDestination(player.position);
                    isTeleporting = false;
                    flickerDurationTimer = 0f;
                    isFlickering = true;
                }
            }
        }
    }

    void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        if (facePlayerImmediately)
        {
            transform.rotation = lookRotation;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void FaceLastKnownPosition()
    {
        Vector3 direction = (lastKnownPlayerPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        if (facePlayerImmediately)
        {
            transform.rotation = lookRotation;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void LookAround()
    {
        transform.Rotate(0, 1f, 0);
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
                return hit.transform == player;
            }
        }
        return false;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(3);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Vector3 fovLine1 = Quaternion.AngleAxis(fovAngle / 2, Vector3.up) * transform.forward * sightRange;
        Vector3 fovLine2 = Quaternion.AngleAxis(-fovAngle / 2, Vector3.up) * transform.forward * sightRange;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);
    }
}