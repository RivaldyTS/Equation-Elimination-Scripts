using UnityEngine;
using System.Collections.Generic;

public class TrailGuideSpawner : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5f;
    public float spawnInterval = 1f;
    [SerializeField]
    private TrailRenderer trailTemplate;
    public LayerMask obstacleLayer;
    public float avoidanceDistance = 1f;
    public bool isActive = false;
    public bool useRangeLimit = false;
    public float triggerDistance = 5f;
    public float hysteresis = 0.5f;
    public bool useDistanceLimit = false;
    public float maxDistance = 10f;
    public bool showGizmos = true;
    public GameObject[] ignoredObjects;
    private float lastSpawnTime;
    private bool isPlayerInside = false;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player reference not set in TrailGuideSpawner script!");
            enabled = false;
            return;
        }

        if (trailTemplate == null)
        {
            GameObject trailObj = new GameObject("TrailTemplate");
            trailObj.transform.SetParent(transform);
            trailTemplate = trailObj.AddComponent<TrailRenderer>();
            trailTemplate.startWidth = 0.2f;
            trailTemplate.endWidth = 0.1f;
            trailTemplate.time = 2f;
            trailTemplate.material = new Material(Shader.Find("Sprites/Default"));
            trailTemplate.startColor = Color.white;
            trailTemplate.endColor = Color.white;
            trailObj.SetActive(false);
        }

        lastSpawnTime = Time.time;
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        if (useRangeLimit)
        {
            float enterThreshold = triggerDistance;
            float exitThreshold = triggerDistance + hysteresis;

            if (distance <= enterThreshold && !isPlayerInside)
            {
                isPlayerInside = true;
                isActive = true;
            }
            else if (distance > exitThreshold && isPlayerInside)
            {
                isPlayerInside = false;
                isActive = false;
            }
        }
        else
        {
            isActive = true;
        }

        if (isActive && Time.time - lastSpawnTime >= spawnInterval)
        {
            SpawnTrailObject();
            lastSpawnTime = Time.time;
        }
    }

    void SpawnTrailObject()
    {
        GameObject trailObj = new GameObject("TrailGuide");
        trailObj.transform.position = player.position;
        TrailRenderer trail = trailObj.AddComponent<TrailRenderer>();
        CopyTrailRendererSettings(trailTemplate, trail);
        TrailMover mover = trailObj.AddComponent<TrailMover>();
        mover.target = transform;
        mover.speed = moveSpeed;
        mover.obstacleLayer = obstacleLayer;
        mover.avoidanceDistance = avoidanceDistance;
        mover.spawnPosition = trailObj.transform.position;
        mover.useDistanceLimit = useDistanceLimit;
        mover.maxDistance = maxDistance;
        mover.ignoredObjects = ignoredObjects;
        if (!useDistanceLimit)
        {
            Destroy(trailObj, trailTemplate.time + 1f);
        }
    }

    private void CopyTrailRendererSettings(TrailRenderer source, TrailRenderer target)
    {
        target.startWidth = source.startWidth;
        target.endWidth = source.endWidth;
        target.time = source.time;
        target.material = source.material;
        target.startColor = source.startColor;
        target.endColor = source.endColor;
        target.widthCurve = source.widthCurve;
        target.colorGradient = source.colorGradient;
        target.minVertexDistance = source.minVertexDistance;
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

    void OnDrawGizmos()
    {
        if (showGizmos && useRangeLimit)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, triggerDistance + hysteresis);
        }
    }
}

public class TrailMover : MonoBehaviour
{
    public Transform target;
    public float speed;
    public LayerMask obstacleLayer;
    public float avoidanceDistance;
    public Vector3 spawnPosition;
    public bool useDistanceLimit;
    public float maxDistance;
    public GameObject[] ignoredObjects;
    private Vector3 velocity;
    private Vector3 currentAvoidanceForce;
    private float raycastAngle = 60f;
    private float collisionRadius = 0.3f;
    private float turnSmoothness = 20f;
    private float disappearDistance = 1f;
    private float predictionDistance = 0.4f;
    private float gapCheckDistance = 0.5f;
    private float gapAvoidanceDistance = 1.5f;
    private TrailRenderer trailRenderer;
    private bool hasStopped = false;
    private int rayCount = 7;
    private float minSpeedFactor = 0.3f;
    private float lookAheadDistance = 2f;
    private float heightAdjustmentRange = 0.5f;
    private float heightAdjustmentSpeed = 1f;
    private List<Vector3> recentDirections = new List<Vector3>();
    private int smoothingWindow = 10;
    private List<float> directionWeights;
    private List<Vector3> obstacleMemory = new List<Vector3>();
    private float obstacleMemoryDuration = 3f;
    private float obstacleMemoryRadius = 1f;
    private float lastObstacleMemoryTime;

    void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        lastObstacleMemoryTime = Time.time;
        directionWeights = new List<float>();
        float totalWeight = 0f;
        for (int i = 0; i < smoothingWindow; i++)
        {
            float weight = Mathf.Lerp(0.5f, 1f, (float)(i + 1) / smoothingWindow);
            directionWeights.Add(weight);
            totalWeight += weight;
        }
        for (int i = 0; i < smoothingWindow; i++)
        {
            directionWeights[i] /= totalWeight;
        }
    }

    void Update()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= disappearDistance && !hasStopped)
        {
            velocity = Vector3.zero;
            trailRenderer.emitting = false;
            hasStopped = true;
            Destroy(gameObject, trailRenderer.time);
        }

        if (useDistanceLimit)
        {
            float distanceTraveled = Vector3.Distance(transform.position, spawnPosition);
            if (distanceTraveled >= maxDistance && !hasStopped)
            {
                velocity = Vector3.zero;
                trailRenderer.emitting = false;
                hasStopped = true;
                Destroy(gameObject, trailRenderer.time);
            }
        }

        if (!hasStopped)
        {
            Vector3 desiredDirection = (target.position - transform.position).normalized;
            Vector3 avoidanceForce = Vector3.zero;
            bool obstacleDetected = false;

            float speedFactor = 1f;
            Vector3 lookAheadPosition = transform.position + desiredDirection * lookAheadDistance;
            Collider[] lookAheadHits = Physics.OverlapSphere(lookAheadPosition, collisionRadius, obstacleLayer, QueryTriggerInteraction.Ignore);
            foreach (Collider hit in lookAheadHits)
            {
                if (!IsIgnoredObject(hit.gameObject))
                {
                    float distanceToObstacle = Vector3.Distance(transform.position, hit.transform.position);
                    speedFactor = Mathf.Lerp(minSpeedFactor, 1f, distanceToObstacle / lookAheadDistance);
                    break;
                }
            }

            Vector3 predictedPosition = transform.position + desiredDirection * predictionDistance;
            Collider[] predictedHits = Physics.OverlapSphere(predictedPosition, collisionRadius, obstacleLayer, QueryTriggerInteraction.Ignore);
            foreach (Collider hit in predictedHits)
            {
                if (!IsIgnoredObject(hit.gameObject))
                {
                    avoidanceForce += (transform.position - predictedPosition).normalized * 0.6f;
                    obstacleDetected = true;
                    if (!IsInObstacleMemory(hit.transform.position))
                    {
                        obstacleMemory.Add(hit.transform.position);
                    }
                    break;
                }
            }

            float angleStep = raycastAngle / (rayCount - 1);
            for (int i = 0; i < rayCount; i++)
            {
                float angle = -raycastAngle / 2 + angleStep * i;
                Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * desiredDirection;
                RaycastHit hit;
                if (Physics.SphereCast
                (transform.position, collisionRadius, rayDirection, out hit, gapAvoidanceDistance, obstacleLayer, QueryTriggerInteraction.Ignore))
                {
                    if (!IsIgnoredObject(hit.collider.gameObject) && !IsInObstacleMemory(hit.point))
                    {
                        if (!IsGapWideEnough(rayDirection))
                        {
                            avoidanceForce += CalculateAvoidanceForce(hit) * 2.5f;
                            obstacleDetected = true;
                            obstacleMemory.Add(hit.point);
                        }
                        else
                        {
                            obstacleDetected = true;
                            float weight = 1f - (Mathf.Abs(angle) / raycastAngle);
                            avoidanceForce += CalculateAvoidanceForce(hit) * weight * 1.5f;
                        }
                    }
                }
            }

            if (Time.time - lastObstacleMemoryTime >= obstacleMemoryDuration)
            {
                obstacleMemory.Clear();
                lastObstacleMemoryTime = Time.time;
            }

            if (obstacleDetected && avoidanceForce.magnitude < 0.1f)
            {
                avoidanceForce = FindEscapeRoute();
            }

            currentAvoidanceForce = Vector3.Lerp(currentAvoidanceForce, avoidanceForce.normalized, Time.deltaTime * turnSmoothness);

            Vector3 finalDirection = obstacleDetected ? 
                (desiredDirection + currentAvoidanceForce).normalized : 
                desiredDirection;

            recentDirections.Add(finalDirection);
            if (recentDirections.Count > smoothingWindow)
            {
                recentDirections.RemoveAt(0);
            }
            Vector3 smoothedDirection = Vector3.zero;
            for (int i = 0; i < recentDirections.Count; i++)
            {
                smoothedDirection += recentDirections[i] * directionWeights[i];
            }
            smoothedDirection = smoothedDirection.normalized;

            float heightAdjustment = 0f;
            RaycastHit heightHit;
            if (Physics.SphereCast(transform.position, collisionRadius, Vector3.down, out heightHit, heightAdjustmentRange, obstacleLayer, QueryTriggerInteraction.Ignore))
            {
                if (!IsIgnoredObject(heightHit.collider.gameObject))
                {
                    float obstacleHeight = heightHit.point.y + collisionRadius;
                    if (obstacleHeight - transform.position.y < heightAdjustmentRange)
                    {
                        heightAdjustment = obstacleHeight - transform.position.y + 0.1f;
                    }
                }
            }
            else if (Physics.SphereCast(transform.position, collisionRadius, Vector3.up, out heightHit, heightAdjustmentRange, obstacleLayer, QueryTriggerInteraction.Ignore))
            {
                if (!IsIgnoredObject(heightHit.collider.gameObject))
                {
                    float ceilingHeight = heightHit.point.y - collisionRadius;
                    if (transform.position.y - ceilingHeight < heightAdjustmentRange)
                    {
                        heightAdjustment = ceilingHeight - transform.position.y - 0.1f;
                    }
                }
            }

            Vector3 targetPosition = transform.position + Vector3.up * heightAdjustment;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * heightAdjustmentSpeed);

            velocity = Vector3.Lerp(velocity, smoothedDirection * speed * speedFactor, Time.deltaTime * turnSmoothness);

            Vector3 newPosition = transform.position + velocity * Time.deltaTime;
            Collider[] hits = Physics.OverlapSphere(newPosition, collisionRadius, obstacleLayer, QueryTriggerInteraction.Ignore);
            bool canMove = true;
            foreach (Collider hit in hits)
            {
                if (!IsIgnoredObject(hit.gameObject))
                {
                    canMove = false;
                    break;
                }
            }

            if (canMove)
            {
                transform.position = newPosition;
            }
            else
            {
                velocity = Vector3.zero;
                avoidanceForce = FindEscapeRoute();
                transform.position += avoidanceForce * Time.deltaTime * speed * speedFactor * 0.5f;
            }

            if (velocity != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(velocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSmoothness);
            }
        }
    }

    private bool IsGapWideEnough(Vector3 direction)
    {
        Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
        float minGapWidth = collisionRadius * 2f;
        float checkDistance = gapCheckDistance;
        RaycastHit hitLeft;
        RaycastHit hitRight;

        bool hitLeftSide = Physics.Raycast(transform.position, -right, out hitLeft, checkDistance, obstacleLayer, QueryTriggerInteraction.Ignore);
        bool hitRightSide = Physics.Raycast(transform.position, right, out hitRight, checkDistance, obstacleLayer, QueryTriggerInteraction.Ignore);

        if (hitLeftSide && hitRightSide)
        {
            if (IsIgnoredObject(hitLeft.collider.gameObject) || IsIgnoredObject(hitRight.collider.gameObject))
            {
                return true;
            }
            float gapWidth = Vector3.Distance(hitLeft.point, hitRight.point);
            return gapWidth >= minGapWidth;
        }
        else if (hitLeftSide || hitRightSide)
        {
            if ((hitLeftSide && IsIgnoredObject(hitLeft.collider.gameObject)) || (hitRightSide && IsIgnoredObject(hitRight.collider.gameObject)))
            {
                return true;
            }
            return false;
        }

        return true;
    }

    private bool IsInObstacleMemory(Vector3 position)
    {
        foreach (Vector3 memoryPos in obstacleMemory)
        {
            if (Vector3.Distance(position, memoryPos) < obstacleMemoryRadius)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsIgnoredObject(GameObject hitObject)
    {
        if (ignoredObjects == null) return false;
        foreach (GameObject ignored in ignoredObjects)
        {
            if (ignored != null && (hitObject == ignored || hitObject.transform.IsChildOf(ignored.transform)))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsIgnoredObjectAtPosition(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, collisionRadius, obstacleLayer, QueryTriggerInteraction.Ignore);
        foreach (Collider col in colliders)
        {
            if (IsIgnoredObject(col.gameObject))
            {
                return true;
            }
        }
        return false;
    }

    private Vector3 CalculateAvoidanceForce(RaycastHit hit)
    {
        Vector3 hitNormal = hit.normal;
        Vector3 avoidanceDir = Vector3.Cross(Vector3.up, hitNormal).normalized;
        
        float leftDot = Vector3.Dot(avoidanceDir, (target.position - transform.position).normalized);
        float rightDot = Vector3.Dot(-avoidanceDir, (target.position - transform.position).normalized);
        return (leftDot > rightDot) ? avoidanceDir : -avoidanceDir;
    }

    private Vector3 FindEscapeRoute()
    {
        float bestScore = float.MinValue;
        Vector3 bestDirection = Vector3.zero;
        
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 testDir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            if (!Physics.SphereCast(transform.position, collisionRadius, testDir, out _, avoidanceDistance, obstacleLayer, QueryTriggerInteraction.Ignore))
            {
                float score = Vector3.Dot(testDir, (target.position - transform.position).normalized);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = testDir;
                }
            }
        }
        
        return bestDirection != Vector3.zero ? bestDirection : Random.onUnitSphere;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 forward = transform.forward;
        Vector3 left = Quaternion.Euler(0, -raycastAngle / 2, 0) * forward;
        Vector3 right = Quaternion.Euler(0, raycastAngle / 2, 0) * forward;
        
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
        Gizmos.DrawRay(transform.position, forward * avoidanceDistance);

        float angleStep = raycastAngle / (rayCount - 1);
        for (int i = 0; i < rayCount; i++)
        {
            float angle = -raycastAngle / 2 + angleStep * i;
            Vector3 rayDir = Quaternion.Euler(0, angle, 0) * forward;
            Gizmos.DrawRay(transform.position, rayDir * avoidanceDistance);
        }
    }
}