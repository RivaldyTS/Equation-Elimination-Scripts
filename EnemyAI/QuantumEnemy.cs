using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class QuantumEnemy : MonoBehaviour
{
    public Transform player;
    public float detectionAngle = 120f;
    public float minDistance = 0.5f;
    public bool resetSceneOnCollision = false;

    [SerializeField] private float teleportDistance = 3f;
    [SerializeField] private float teleportInterval = 0.5f;
    [SerializeField] private float enrageDuration = 5f;
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private GameObject modelParent;
    [SerializeField] private bool dimLightOnTeleport = false;
    [SerializeField] private Light sceneLight;
    [SerializeField] private bool vanishingActEnabled = false;
    [SerializeField] private float vanishChance = 0.2f;
    [SerializeField] private float vanishDuration = 2f;
    [SerializeField] private float reappearDistance = 2f;
    [SerializeField] private AudioClip reappearSound;
    [SerializeField] private bool endlessLoopEnabled = false;
    [SerializeField] private float loopDuration = 5f;
    [SerializeField] private AudioClip loopSound;
    [SerializeField] private bool genjutsuVoidEnabled = false;
    [SerializeField] private float voidRadius = 15f;
    [SerializeField] private Material voidMaterial;
    [SerializeField] private bool fracturedSightEnabled = true;
    [SerializeField] private float fractureTriggerTime = 3f;
    [SerializeField] private GameObject fractureVolumeObject;
    [SerializeField] private float fractureMaxIntensity = 1f;
    [SerializeField] private float fractureFadeInSpeed = 2f;
    [SerializeField] private float fractureFadeOutSpeed = 2f;
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private bool spawnClonesWhileChasing = true;
    [SerializeField] private float cloneSpawnChanceWhileChasing = 0.05f;
    [SerializeField] private bool spawnClonesWhenObserved = true;
    [SerializeField] private float cloneSpawnChanceWhenObserved = 0.03f;
    [SerializeField] private float cloneSpawnRadius = 2f;
    [SerializeField] private bool clonesFacePlayer = true;
    [SerializeField] private float cloneLifetime = 3f;
    [SerializeField] private float cloneTransparency = 0.5f;

    private CanvasGroup hudCanvasGroup;
    private NavMeshAgent agent;
    private bool isObserved = false;
    private bool isEnraged = false;
    private float enrageTimer = 0f;
    private float teleportTimer = 0f;
    private float normalSpeed = 10f;
    private AudioSource audioSource;
    private float originalLightIntensity;
    private bool isVanishing = false;
    private bool isLooping = false;
    private List<Vector3> teleportHistory = new List<Vector3>();
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private float observationTimer = 0f;
    private float fractureIntensity = 0f;
    private Volume fractureVolume;
    private List<GameObject> activeClones = new List<GameObject>();

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = minDistance;
        normalSpeed = agent.speed;
        agent.angularSpeed = 0f;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (modelParent == null)
        {
            Debug.LogWarning("Model Parent not assigned in QuantumEnemy. Flicker effect won’t work.");
        }

        if (sceneLight != null)
        {
            originalLightIntensity = sceneLight.intensity;
        }
        if (dimLightOnTeleport && sceneLight == null)
        {
            Debug.LogWarning("Dim Light On Teleport is enabled but no Scene Light assigned.");
        }
        if (genjutsuVoidEnabled && voidMaterial == null)
        {
            Debug.LogWarning("Genjutsu Void is enabled but no Void Material assigned. Creating default.");
            voidMaterial = new Material(Shader.Find("Unlit/Color"));
            voidMaterial.color = Color.black;
        }
        if (fracturedSightEnabled && fractureVolumeObject != null)
        {
            fractureVolume = fractureVolumeObject.GetComponent<Volume>();
            if (fractureVolume == null)
            {
                Debug.LogWarning("Fracture Volume Object assigned but no Volume component found.");
            }
        }
        else if (fracturedSightEnabled)
        {
            Debug.LogWarning("Fractured Sight is enabled but no Volume Object assigned. Effect won’t work.");
        }

        if (hudCanvas != null)
        {
            hudCanvasGroup = hudCanvas.GetComponent<CanvasGroup>();
            if (hudCanvasGroup == null)
            {
                hudCanvasGroup = hudCanvas.gameObject.AddComponent<CanvasGroup>();
            }
            hudCanvasGroup.alpha = 1f;
        }

        if (clonePrefab == null)
        {
            Debug.LogWarning("Clone Prefab not assigned in QuantumEnemy. Shadow clones won’t spawn.");
        }
    }

    void Update()
    {
        CheckIfObserved();

        if (fracturedSightEnabled && fractureVolumeObject != null && fractureVolume != null)
        {
            if (!isEnraged && isObserved)
            {
                observationTimer += Time.deltaTime;
                if (observationTimer >= fractureTriggerTime)
                {
                    if (!fractureVolumeObject.activeSelf)
                    {
                        fractureVolumeObject.SetActive(true);
                    }
                    fractureIntensity = Mathf.Lerp(fractureIntensity, fractureMaxIntensity, Time.deltaTime * fractureFadeInSpeed);

                    // Spawn clones when observed
                    if (spawnClonesWhenObserved && clonePrefab != null && Random.value < cloneSpawnChanceWhenObserved * Time.deltaTime)
                    {
                        SpawnShadowClones();
                    }
                }
            }
            else if (isEnraged)
            {
                if (!fractureVolumeObject.activeSelf)
                {
                    fractureVolumeObject.SetActive(true);
                }
                fractureIntensity = Mathf.Lerp(fractureIntensity, fractureMaxIntensity, Time.deltaTime * fractureFadeInSpeed);
            }
            else
            {
                observationTimer = 0f;
                fractureIntensity = Mathf.Lerp(fractureIntensity, 0f, Time.deltaTime * fractureFadeOutSpeed);
                if (fractureIntensity <= 0.01f && fractureVolumeObject.activeSelf)
                {
                    fractureVolumeObject.SetActive(false);
                }
            }

            fractureVolume.weight = fractureIntensity;
            Debug.Log($"Fracture Intensity: {fractureIntensity}");

            if (hudCanvasGroup != null)
            {
                float glitchAmount = fractureIntensity;
                hudCanvasGroup.alpha = 1f - (Mathf.PingPong(Time.time * 10f, 0.2f) * glitchAmount);
            }
        }

        if (isEnraged && !isVanishing && !isLooping)
        {
            agent.speed = normalSpeed;
            agent.stoppingDistance = 0f;
            enrageTimer -= Time.deltaTime;
            teleportTimer -= Time.deltaTime;

            if (teleportTimer <= 0f)
            {
                TeleportTowardPlayer();
                teleportTimer = teleportInterval;
            }

            if (enrageTimer <= 0f)
            {
                isEnraged = false;
                agent.stoppingDistance = minDistance;
                if (dimLightOnTeleport && sceneLight != null)
                {
                    sceneLight.intensity = originalLightIntensity;
                }
                if (genjutsuVoidEnabled)
                {
                    RestoreMaterials();
                }
            }
            return;
        }

        if (!isObserved && IsPlayerVisible())
        {
            MoveTowardPlayer();
        }
        else
        {
            agent.isStopped = true;
        }
    }

    void CheckIfObserved()
    {
        Vector3 directionToEnemy = (transform.position - player.position).normalized;
        Vector3 playerForward = player.forward;
        float angle = Vector3.Angle(playerForward, directionToEnemy);

        if (angle < detectionAngle * 0.5f)
        {
            RaycastHit hit;
            Vector3 rayOrigin = player.position + Vector3.up * 1.5f;
            if (Physics.Raycast(rayOrigin, directionToEnemy, out hit, Mathf.Infinity))
            {
                if (hit.transform == transform)
                {
                    isObserved = true;
                    return;
                }
            }
        }
        isObserved = false;
    }

    bool IsPlayerVisible()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit))
        {
            if (hit.transform == player)
            {
                return true;
            }
            return false;
        }
        return true;
    }

    void MoveTowardPlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);

        // Spawn clones while chasing
        if (spawnClonesWhileChasing && clonePrefab != null && Random.value < cloneSpawnChanceWhileChasing * Time.deltaTime)
        {
            SpawnShadowClones();
        }

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    void SpawnShadowClones()
    {
        int cloneCount = Random.Range(1, 3); // Spawn 1-2 clones
        for (int i = 0; i < cloneCount; i++)
        {
            Vector3 spawnOffset = Random.insideUnitSphere * cloneSpawnRadius;
            spawnOffset.y = 0;
            Vector3 spawnPosition = transform.position + spawnOffset;

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(spawnPosition, out navHit, 5f, NavMesh.AllAreas))
            {
                GameObject clone = Instantiate(clonePrefab, navHit.position, Quaternion.identity);
                activeClones.Add(clone);

                // Make clones face player if enabled
                if (clonesFacePlayer)
                {
                    Vector3 lookDirection = (player.position - clone.transform.position).normalized;
                    lookDirection.y = 0;
                    clone.transform.rotation = Quaternion.LookRotation(lookDirection);
                }

                // Set transparency
                Renderer[] renderers = clone.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers)
                {
                    foreach (Material mat in rend.materials)
                    {
                        mat.SetFloat("_Surface", 1); // Transparent surface
                        mat.SetFloat("_Blend", 0); // Alpha blend mode
                        mat.SetColor("_BaseColor", new Color(mat.color.r, mat.color.g, mat.color.b, cloneTransparency));
                    }
                }

                // Make clone chase player
                NavMeshAgent cloneAgent = clone.GetComponent<NavMeshAgent>();
                if (cloneAgent != null)
                {
                    cloneAgent.SetDestination(player.position);
                }

                StartCoroutine(CloneLifetime(clone));
            }
        }
    }

    IEnumerator CloneLifetime(GameObject clone)
    {
        yield return new WaitForSeconds(cloneLifetime);
        if (clone != null)
        {
            activeClones.Remove(clone);
            Destroy(clone);
        }
    }

    void TeleportTowardPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 targetPosition = transform.position + directionToPlayer * teleportDistance;

        if (!isEnraged)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer - teleportDistance < minDistance)
            {
                targetPosition = player.position - directionToPlayer * minDistance;
            }
        }

        if (vanishingActEnabled && isEnraged && Random.value < vanishChance)
        {
            StartCoroutine(VanishAndReappear());
            return;
        }

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(targetPosition, out navHit, 5f, NavMesh.AllAreas))
        {
            if (isEnraged)
            {
                teleportHistory.Add(navHit.position);
                if (teleportHistory.Count > 5) teleportHistory.RemoveAt(0);
            }

            agent.Warp(navHit.position);
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
            PlayTeleportSound();
            if (modelParent != null)
            {
                StartCoroutine(FlickerEffect());
            }
            if (dimLightOnTeleport && sceneLight != null)
            {
                StartCoroutine(DimLightEffect());
            }
        }
    }

    IEnumerator VanishAndReappear()
    {
        isVanishing = true;
        Vector3 vanishDirection = Random.insideUnitSphere.normalized;
        vanishDirection.y = 0;
        Vector3 vanishPosition = player.position + vanishDirection * 20f;
        NavMeshHit vanishHit;
        if (NavMesh.SamplePosition(vanishPosition, out vanishHit, 25f, NavMesh.AllAreas))
        {
            agent.Warp(vanishHit.position);
            modelParent.SetActive(false);
            yield return new WaitForSeconds(vanishDuration);

            Vector3 reappearPosition = player.position - player.forward * reappearDistance;
            NavMeshHit reappearHit;
            if (NavMesh.SamplePosition(reappearPosition, out reappearHit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(reappearHit.position);
                transform.rotation = Quaternion.LookRotation(player.position - transform.position);
                modelParent.SetActive(true);
                if (reappearSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(reappearSound, 1.5f);
                }
                if (modelParent != null)
                {
                    StartCoroutine(BigFlickerEffect());
                }
            }
        }
        isVanishing = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if (resetSceneOnCollision)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else if (endlessLoopEnabled && !isLooping && isEnraged)
            {
                StartCoroutine(EndlessLoop());
            }
        }
    }

    IEnumerator EndlessLoop()
    {
        isLooping = true;
        float loopEndTime = Time.time + loopDuration;
        if (loopSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(loopSound);
        }

        while (Time.time < loopEndTime && teleportHistory.Count > 0)
        {
            foreach (Vector3 pos in teleportHistory)
            {
                agent.Warp(pos);
                if (modelParent != null)
                {
                    StartCoroutine(FlickerEffect());
                }
                if (teleportSound != null && audioSource != null)
                {
                    audioSource.pitch = Random.Range(0.8f, 1.2f);
                    audioSource.PlayOneShot(teleportSound);
                    audioSource.pitch = 1f;
                }
                yield return new WaitForSeconds(0.5f);
                if (Time.time >= loopEndTime) break;
            }
        }

        isLooping = false;
        if (dimLightOnTeleport && sceneLight != null)
        {
            sceneLight.intensity = originalLightIntensity;
        }
    }

    void PlayTeleportSound()
    {
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }
    }

    IEnumerator FlickerEffect()
    {
        modelParent.SetActive(false);
        yield return new WaitForSeconds(0.05f);
        modelParent.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        modelParent.SetActive(false);
        yield return new WaitForSeconds(0.05f);
        modelParent.SetActive(true);
    }

    IEnumerator BigFlickerEffect()
    {
        modelParent.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        modelParent.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        modelParent.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        modelParent.SetActive(true);
    }

    IEnumerator DimLightEffect()
    {
        float currentIntensity = sceneLight.intensity;
        sceneLight.intensity = 0.2f;
        yield return new WaitForSeconds(0.1f);
        sceneLight.intensity = originalLightIntensity;
    }

    public void OnHitByPlayer()
    {
        isEnraged = true;
        enrageTimer = enrageDuration;
        teleportTimer = 0f;
        teleportHistory.Clear();
        if (genjutsuVoidEnabled && voidMaterial != null)
        {
            ApplyGenjutsuVoid();
        }
        if (fracturedSightEnabled && fractureVolumeObject != null && fractureVolume != null)
        {
            observationTimer = 0f;
        }
    }

    void OnDestroy()
    {
        if (dimLightOnTeleport && sceneLight != null)
        {
            sceneLight.intensity = originalLightIntensity;
        }
        if (genjutsuVoidEnabled)
        {
            RestoreMaterials();
        }
        if (fracturedSightEnabled && fractureVolumeObject != null && fractureVolume != null)
        {
            fractureVolume.weight = 0f;
            fractureVolumeObject.SetActive(false);
        }
        if (hudCanvasGroup != null)
        {
            hudCanvasGroup.alpha = 1f;
        }
        foreach (var clone in activeClones)
        {
            if (clone != null)
            {
                Destroy(clone);
            }
        }
        activeClones.Clear();
    }

    void ApplyGenjutsuVoid()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, voidRadius);
        foreach (Collider col in nearbyObjects)
        {
            if (col.transform == transform || col.transform == player) continue;
            Renderer rend = col.GetComponent<Renderer>();
            if (rend != null && !originalMaterials.ContainsKey(rend))
            {
                originalMaterials[rend] = rend.materials;
                Material[] blackMats = new Material[rend.materials.Length];
                for (int i = 0; i < blackMats.Length; i++)
                {
                    blackMats[i] = voidMaterial;
                }
                rend.materials = blackMats;
            }
        }
    }

    void RestoreMaterials()
    {
        foreach (var pair in originalMaterials)
        {
            if (pair.Key != null)
            {
                pair.Key.materials = pair.Value;
            }
        }
        originalMaterials.Clear();
    }
}