using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Gun : MonoBehaviour
{
    public LayerMask layerMask;
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    public float impactForce = 30f;

    public int maxAmmo = 10;
    private int currentAmmo;
    public float maxAmmoReserve = 50f;
    private float currentAmmoReserve;
    public float reloadTime = 1f;
    private bool isReloading = false;

    public Camera fpsCamera;
    public ParticleSystem muzzleflashPrefab;
    public GameObject impactEffectPrefab;
    public Transform muzzleTransform;

    public AudioSource shootingAudioSource;
    public AudioClip shootingSound;
    public float shootingVolume = 1f;
    public AudioClip reloadSound;
    public float reloadVolume = 1f;
    public AudioClip emptyAmmoSound;
    public float emptyAmmoVolume = 1f;
    public AudioClip cockingSound;
    public float cockingVolume = 1f;
    public AudioClip[] bulletFallSounds;
    public float bulletFallVolume = 1f;
    private bool hasPlayedEmptyAmmoSound = false;

    public Animator animator;
    public Text ammoDisplay; // 2D UI ammo display (includes reserve)
    public TextMeshPro weaponAmmoDisplay; // 3D TextMeshPro display on the weapon (current/max only)

    [Header("Recoil Settings")]
    public float horizontalRecoil = 0.1f;
    public float verticalRecoil = 0.1f;
    public float recoilKickback = 0.1f;
    public float recoilReturnSpeed = 2f;
    public float fovKickAmount = 5f; // FOV kick when shooting
    public float fovReturnSpeed = 5f; // Speed at which FOV returns to normal
    private Vector3 originalPosition;
    private float originalFOV; // Store the camera's original FOV
    private float targetFOV; // The FOV to lerp towards

    [Header("Shotgun Settings")]
    public bool isShotgun = false;
    public int pelletsPerShot = 8;
    public float spreadAngle = 10f;
    public float shotgunReloadTime = 0.5f;

    [Header("Sniper Settings")]
    public bool isSniper = false;

    private float nextTimeToFire = 0f;

    public Transform playerTransform;
    public float movementThreshold = 0.01f;
    private Vector3 previousPosition;

    [Header("Scope Settings")]
    public bool isScoped = false;

    [Header("Accuracy Settings")]
    public float baseAccuracy = 1.0f;
    public float accuracyDecreaseRate = 0.1f;
    public float burstAccuracyDecreaseRate = 0.05f;
    public float accuracyRecoveryRate = 0.2f;
    public float stationaryBonus = 0.1f;
    private float currentAccuracy;
    private float lastShotTime;

    [Header("Recoil Pattern Settings")]
    public Vector2[] recoilPattern;
    public float patternScale = 1.0f;
    public float patternResetTime = 1.0f;
    private int currentPatternIndex = 0;
    private float lastPatternResetTime;

    [Header("Firing Mode Settings")]
    public bool canUseAuto = true;
    public bool canUseBurst = true;
    public bool canUseSingle = true;
    public int burstCount = 3;
    public float burstFireRateMultiplier = 2f;
    private enum FireMode { Auto, Burst, Single }
    private FireMode currentFireMode;
    private bool isBursting = false;

    [Header("UI Settings")]
    public TMP_Text fireModeDisplay;
    public float displayDuration = 2f;
    public Image hitMarkerImage;
    public float hitMarkerDuration = 0.2f;
    public AudioClip[] hitMarkerSounds; // New: Hitmarker audio clip
    public float hitMarkerVolume = 0.5f; // New: Volume for hitmarker sound

    [Header("Visual Effects")]
    public TrailRenderer bulletTrailPrefab;
    public Transform laserOrigin;
    public float laserMaxLength = 50f;
    private LineRenderer laserLine;
    private bool isLaserActive = false;
    private bool wasLaserActiveBeforeReload = false;
    public ParticleSystem modeEffectPrefab;
    private ParticleSystem modeEffect;
    public ParticleSystem lastShotSmokePrefab;

    [Header("Reload Minigame Settings")]
    public bool useReloadMinigame = false;
    public KeyCode[] reloadSequence = { KeyCode.Q, KeyCode.E };
    public float reloadTimeWindow = 1f;
    public AudioClip reloadSuccessSound;
    public float reloadSuccessVolume = 1f;
    public AudioClip reloadFailSound;
    public float reloadFailVolume = 1f;
    private bool isReloadMinigameActive = false;
    private int reloadSequenceIndex = 0;

    [Header("Thermal Vision Settings")]
    public bool useThermalVision = true;
    public string[] thermalVisionTags = { "Enemy" };
    [SerializeField] private List<Outline> enemyOutlines = new List<Outline>();

    [Header("Gravity Pulse Settings")]
    public bool useGravityPulse = true;
    public float gravityPulseRange = 10f;
    public float gravityPulseForce = 1000f;
    public float gravityPulseCooldown = 2f;
    private float nextGravityPulseTime = 0f;
    private bool isGravityPull = true;
    public string[] pullableTags = { "Enemy", "Prop" };
    public Transform holdPosition; // Empty object to hold pulled objects
    private List<Rigidbody> heldObjects = new List<Rigidbody>(); // Track held objects
    public float pullSpeed = 5f; // Speed of the pull (adjust in Inspector for smoothness)
    public float throwForceMultiplier = 1.5f; // Multiplier for the throw force

    [Header("Alternate Fire Settings")]
    public bool canUseAltFire = false;
    private bool isAltFireActive = false;
    public int maxAltAmmo = 2;
    private int currentAltAmmo;
    public float maxAltAmmoReserve = 6f;
    private float currentAltAmmoReserve;
    public float altFireRate = 1f;
    public float explosionRadius = 5f;
    public float clusterExplosionRadius = 10f;
    public float explosionForce = 700f;
    public GameObject grenadePrefab;
    public GameObject explosionEffectPrefab;
    public AudioClip altFireSound;
    public float altFireVolume = 1f;
    public AudioClip explosionSound;
    public float explosionVolume = 1f;
    public float launchForce = 20f;
    public float upwardForce = 5f;
    public AudioClip altReloadSound;
    public float altReloadVolume = 1f;
    public ParticleSystem smokeParticlePrefab;
    public TrailRenderer grenadeTrailPrefab;
    public int maxBounces = 2;
    public bool isClusterMode = false;
    public int clusterCount = 4;
    public float clusterSpread = 2f;
    public float clusterDelay = 0.5f;
    public bool useRandomClusterDelay = false;
    public float minClusterDelay = 0.3f;
    public float maxClusterDelay = 1f;

    [Header("Ricochet Settings")]
    public bool useRicochet = false;
    public int maxRicochetBounces = 2;
    public float ricochetDamageReduction = 0.5f;

    [Header("Charged Shot Settings")]
    public bool useChargedShot = false;
    public float maxChargeTime = 1.5f;
    public float chargedDamageMultiplier = 2f;
    public float chargedForceMultiplier = 2f;
    private float chargeTime = 0f;
    private bool isCharging = false;
    public AudioClip chargeHumSound;
    public float chargeHumVolume = 0.5f;
    private AudioSource chargeAudioSource;
    public float explosionSlowMoDuration = 0.5f;
    public float explosionSlowMoScale = 0.3f;
    private Scope scope;

    void Start()
    {
        currentAmmo = maxAmmo;
        currentAmmoReserve = maxAmmoReserve;
        currentAccuracy = baseAccuracy;

        if (canUseAltFire)
        {
            currentAltAmmo = maxAltAmmo;
            currentAltAmmoReserve = maxAltAmmoReserve;
        }

        if (shootingAudioSource == null)
            shootingAudioSource = GetComponent<AudioSource>();

        chargeAudioSource = gameObject.AddComponent<AudioSource>();
        chargeAudioSource.clip = chargeHumSound;
        chargeAudioSource.volume = chargeHumVolume;
        chargeAudioSource.loop = true;
        chargeAudioSource.playOnAwake = false;

        laserLine = GetComponent<LineRenderer>();
        if (laserLine == null)
        {
            laserLine = gameObject.AddComponent<LineRenderer>();
            laserLine.positionCount = 2;
            laserLine.startWidth = 0.02f;
            laserLine.endWidth = 0.02f;
            laserLine.material = new Material(Shader.Find("Sprites/Default"));
            laserLine.startColor = Color.red;
            laserLine.endColor = Color.red;
        }
        laserLine.enabled = false;

        modeEffect = muzzleTransform?.GetComponentInChildren<ParticleSystem>();
        if (modeEffect != null)
        {
            modeEffect.Stop();
        }

        if (muzzleTransform == null)
        {
            muzzleTransform = transform.Find("Muzzle")?.transform;
        }

        UpdateAmmoUI();
        originalPosition = transform.localPosition;

        if (playerTransform != null)
        {
            previousPosition = playerTransform.position;
        }
        else
        {
            Debug.LogError("Player Transform not assigned in the Gun script.");
        }

        if (recoilPattern == null || recoilPattern.Length == 0)
        {
            recoilPattern = new Vector2[] {
                new Vector2(0f, 0.1f),
                new Vector2(0.05f, 0.08f),
                new Vector2(-0.03f, 0.06f),
                new Vector2(0.02f, 0.04f)
            };
        }

        if (canUseAuto) currentFireMode = FireMode.Auto;
        else if (canUseBurst) currentFireMode = FireMode.Burst;
        else if (canUseSingle) currentFireMode = FireMode.Single;

        if (fireModeDisplay != null) fireModeDisplay.gameObject.SetActive(false);
        if (hitMarkerImage != null) hitMarkerImage.gameObject.SetActive(false);

        if (useThermalVision)
        {
            foreach (string tag in thermalVisionTags)
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
                {
                    Outline outline = obj.GetComponent<Outline>();
                    if (outline != null)
                    {
                        enemyOutlines.Add(outline);
                        outline.enabled = false;
                    }
                }
            }
        }

        // Initialize FOV settings
        if (fpsCamera != null)
        {
            originalFOV = fpsCamera.fieldOfView;
        }

        // Find the Scope script on the Weapon Holder
        scope = GetComponentInParent<Scope>();

        // Ensure the 3D ammo display is set up
        if (weaponAmmoDisplay != null)
        {
            weaponAmmoDisplay.gameObject.SetActive(true);
        }

        // Ensure holdPosition is assigned
        if (holdPosition == null)
        {
            Debug.LogError("Hold Position Transform not assigned in the Gun script!");
        }
    }

    void Update()
    {
        if (isReloading && currentAmmo > 0 && (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0)))
        {
            CancelReload();
        }

        if (isReloading || isBursting)
            return;

        if (currentAmmo <= 0 && currentAmmoReserve > 0 && !hasPlayedEmptyAmmoSound)
        {
            if (emptyAmmoSound != null && shootingAudioSource != null)
            {
                shootingAudioSource.PlayOneShot(emptyAmmoSound, emptyAmmoVolume);
                hasPlayedEmptyAmmoSound = true;
            }
            // Force unscope before auto-reloading
            if (isScoped)
            {
                isScoped = false;
                scope.isScoped = false;
                scope.animator.SetBool("Scoped", false);
                StartCoroutine(scope.ZoomOut());
                if (useThermalVision)
                {
                    ToggleThermalVision(false);
                    UpdateModeEffect(Color.white, false);
                }
            }
            StartCoroutine(Reload());
            return;
        }

        if (currentAmmo > 0)
        {
            hasPlayedEmptyAmmoSound = false;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            SwitchFireMode();
        }

        if (Input.GetKeyDown(KeyCode.Z) && canUseAltFire && !isReloading && !isScoped)
        {
            SwitchAltFire();
        }

        if (Input.GetMouseButtonDown(2))
        {
            ToggleLaser();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            useRicochet = !useRicochet;
            ShowFireModeText("Ricochet: " + (useRicochet ? "On" : "Off"));
        }

        // Simplified toggle logic for Gravity Pulse mode
        if (useGravityPulse && Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("H key pressed - Toggling Gravity Pulse mode");
            isGravityPull = !isGravityPull;
            ShowFireModeText("Gravity Pulse: " + (isGravityPull ? "Pull" : "Push"));
            UpdateModeEffect(isGravityPull ? Color.blue : Color.green, false);
            Debug.Log("Gravity Pulse mode set to: " + (isGravityPull ? "Pull" : "Push"));
        }

        if (useGravityPulse && Input.GetKeyDown(KeyCode.Y) && Time.time >= nextGravityPulseTime)
        {
            Debug.Log("Y key pressed - Executing Gravity Pulse, mode: " + (isGravityPull ? "Pull" : "Push"));
            GravityPulse();
            nextGravityPulseTime = Time.time + gravityPulseCooldown;
            UpdateModeEffect(isGravityPull ? Color.blue : Color.green, false);
        }

        if (Time.time >= nextTimeToFire)
        {
            float effectiveFireRate = (currentFireMode == FireMode.Burst) ? fireRate * burstFireRateMultiplier : fireRate;

            if (isAltFireActive)
            {
                if (Input.GetMouseButtonDown(0) && currentAltAmmo > 0)
                {
                    nextTimeToFire = Time.time + 1f / altFireRate;
                    FireGrenade();
                }
            }
            else
            {
                if (useChargedShot && currentFireMode != FireMode.Burst)
                {
                    if (Input.GetMouseButtonDown(0) && !isCharging && currentAmmo > 0)
                    {
                        isCharging = true;
                        chargeTime = 0f;
                        if (chargeAudioSource != null && chargeHumSound != null)
                        {
                            chargeAudioSource.Play();
                        }
                        UpdateModeEffect(Color.yellow, false);
                    }
                    if (isCharging && Input.GetMouseButton(0))
                    {
                        chargeTime += Time.deltaTime;
                        if (chargeTime >= maxChargeTime) chargeTime = maxChargeTime;
                    }
                    if (isCharging && Input.GetMouseButtonUp(0) && currentAmmo > 0)
                    {
                        nextTimeToFire = Time.time + 1f / effectiveFireRate;
                        ShootCharged();
                        isCharging = false;
                        chargeTime = 0f;
                        if (chargeAudioSource != null)
                        {
                            chargeAudioSource.Stop();
                        }
                        UpdateModeEffect(Color.white, false);
                    }
                }

                if (!isCharging)
                {
                    if (currentFireMode == FireMode.Auto && Input.GetMouseButton(0) && currentAmmo > 0)
                    {
                        nextTimeToFire = Time.time + 1f / effectiveFireRate;
                        Shoot();
                        UpdateModeEffect(Color.white, true);
                    }
                    else if (currentFireMode == FireMode.Auto && !Input.GetMouseButton(0) && Time.time < nextTimeToFire + 0.5f)
                    {
                        SpawnLastShotSmoke();
                    }
                    else if (currentFireMode == FireMode.Burst && Input.GetMouseButtonDown(0) && currentAmmo > 0)
                    {
                        StartCoroutine(BurstFire(effectiveFireRate));
                        UpdateModeEffect(Color.white, false);
                    }
                    else if (currentFireMode == FireMode.Single && Input.GetMouseButtonDown(0) && currentAmmo > 0)
                    {
                        nextTimeToFire = Time.time + 1f / effectiveFireRate;
                        Shoot();
                        UpdateModeEffect(Color.white, true);
                        SpawnLastShotSmoke();
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (isAltFireActive && currentAltAmmo < maxAltAmmo && currentAltAmmoReserve > 0)
            {
                // Force unscope before reloading alt ammo
                if (isScoped)
                {
                    isScoped = false;
                    scope.isScoped = false;
                    scope.animator.SetBool("Scoped", false);
                    StartCoroutine(scope.ZoomOut());
                    if (useThermalVision)
                    {
                        ToggleThermalVision(false);
                        UpdateModeEffect(Color.white, false);
                    }
                }
                StartCoroutine(ReloadAlt());
            }
            else if (!isAltFireActive && currentAmmo < maxAmmo && currentAmmoReserve > 0)
            {
                // Force unscope before reloading primary ammo
                if (isScoped)
                {
                    isScoped = false;
                    scope.isScoped = false;
                    scope.animator.SetBool("Scoped", false);
                    StartCoroutine(scope.ZoomOut());
                    if (useThermalVision)
                    {
                        ToggleThermalVision(false);
                        UpdateModeEffect(Color.white, false);
                    }
                }
                if (useReloadMinigame)
                {
                    StartCoroutine(ReloadMinigame());
                }
                else
                {
                    StartCoroutine(Reload());
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            // Prevent scoping while reloading
            if (isReloading)
            {
                return;
            }

            isScoped = !isScoped;
            animator.SetBool("IsScoped", isScoped);
            Debug.Log("Scoped toggled to: " + isScoped + ", MuzzleTransform: " + (muzzleTransform != null ? muzzleTransform.name : "null") + ", MuzzleflashPrefab: " + (muzzleflashPrefab != null));

            // Sync with Scope script
            if (scope != null)
            {
                scope.isScoped = isScoped;
                scope.animator.SetBool("Scoped", isScoped);
                if (isScoped)
                {
                    StartCoroutine(scope.ZoomIn());
                }
                else
                {
                    StartCoroutine(scope.ZoomOut());
                }
            }

            if (useThermalVision)
            {
                ToggleThermalVision(isScoped);
                UpdateModeEffect(isScoped ? Color.red : Color.white, false);
            }
        }

        if (useThermalVision)
        {
            foreach (Outline outline in enemyOutlines)
            {
                if (outline != null && outline.enabled != isScoped)
                {
                    outline.enabled = isScoped;
                    Debug.Log("Synced outline on " + outline.gameObject.name + " to " + isScoped);
                }
            }
        }

        if (playerTransform != null)
        {
            float distanceMoved = Vector3.Distance(previousPosition, playerTransform.position);
            bool isMoving = distanceMoved > movementThreshold;
            animator.SetBool("IsMoving", isMoving);
            previousPosition = playerTransform.position;

            if (!isMoving)
            {
                currentAccuracy = Mathf.MoveTowards(currentAccuracy, baseAccuracy + stationaryBonus, accuracyRecoveryRate * Time.deltaTime);
            }
            else if (Time.time > lastShotTime + 0.5f)
            {
                currentAccuracy = Mathf.MoveTowards(currentAccuracy, baseAccuracy, accuracyRecoveryRate * Time.deltaTime);
            }
            currentAccuracy = Mathf.Clamp(currentAccuracy, 0f, 1f + stationaryBonus);
        }

        if (Time.time > lastPatternResetTime + patternResetTime)
        {
            currentPatternIndex = 0;
            lastPatternResetTime = Time.time;
        }

        // Smoothly return gun position
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * recoilReturnSpeed);

        // Smoothly return FOV to originalFOV (only when not scoped and not zooming)
        if (fpsCamera != null && !isScoped && !scope.IsZooming() && fpsCamera.fieldOfView != originalFOV)
        {
            fpsCamera.fieldOfView = Mathf.Lerp(fpsCamera.fieldOfView, originalFOV, Time.deltaTime * fovReturnSpeed);
            if (Mathf.Abs(fpsCamera.fieldOfView - originalFOV) < 0.1f)
            {
                fpsCamera.fieldOfView = originalFOV; // Snap to original to avoid floating point drift
            }
        }

        UpdateLaser();
    }

    void OnEnable()
    {
        isReloading = false;
        animator.SetBool("Reloading", false);
        if (wasLaserActiveBeforeReload)
        {
            isLaserActive = true;
            laserLine.enabled = true;
        }
        if (weaponAmmoDisplay != null)
        {
            weaponAmmoDisplay.gameObject.SetActive(true);
        }
    }

    void OnDisable()
    {
        // Release all held objects when the gun is disabled
        ReleaseAllHeldObjects();
        if (weaponAmmoDisplay != null)
        {
            weaponAmmoDisplay.gameObject.SetActive(false);
        }
    }

    void Shoot()
    {
        if (currentAmmo <= 0)
            return;

        UpdateModeEffect(Color.white, true);

        currentAmmo--;
        lastShotTime = Time.time;

        if (currentFireMode == FireMode.Auto)
        {
            currentAccuracy -= accuracyDecreaseRate;
        }
        else if (currentFireMode == FireMode.Burst)
        {
            currentAccuracy -= burstAccuracyDecreaseRate;
        }
        currentAccuracy = Mathf.Max(currentAccuracy, 0f);

        if (shootingAudioSource != null && shootingSound != null)
        {
            shootingAudioSource.PlayOneShot(shootingSound, shootingVolume);
        }

        if (isShotgun)
        {
            for (int i = 0; i < pelletsPerShot; i++)
            {
                FirePellet();
            }
            if (cockingSound != null)
            {
                shootingAudioSource.PlayOneShot(cockingSound, cockingVolume);
            }
            ApplyShotgunRecoil();
        }
        else if (isSniper)
        {
            FireSingleBullet(1f, 1f, false);
            if (cockingSound != null)
            {
                shootingAudioSource.PlayOneShot(cockingSound, cockingVolume);
            }
            animator.SetTrigger("SniperFire");
        }
        else
        {
            FireSingleBullet(1f, 1f, false);
            ApplyRecoil();
        }

        ApplyGunRecoil();
        UpdateAmmoUI();

        currentPatternIndex = (currentPatternIndex + 1) % recoilPattern.Length;
        StartCoroutine(PlayBulletFallSound());
    }

    void UpdateModeEffect(Color color, bool playOnShoot = false)
    {
        if (modeEffect != null)
        {
            ParticleSystem.MainModule main = modeEffect.main;
            main.startColor = color;
            if (playOnShoot && !modeEffect.isPlaying)
            {
                modeEffect.Play();
                Debug.Log("ModeEffect played on shoot");
            }
            else if (!playOnShoot && modeEffect.isPlaying && color == Color.white)
            {
                modeEffect.Stop();
                Debug.Log("ModeEffect stopped");
            }
        }
    }

    void SpawnLastShotSmoke()
    {
        if (lastShotSmokePrefab != null && muzzleTransform != null)
        {
            ParticleSystem smoke = Instantiate(lastShotSmokePrefab, muzzleTransform.position, muzzleTransform.rotation);
            smoke.Play();
            Debug.Log("Last shot smoke spawned at " + muzzleTransform.position);
            Destroy(smoke.gameObject, smoke.main.duration);
        }
        else
        {
            Debug.LogWarning("LastShotSmokePrefab or MuzzleTransform is null!");
        }
    }

    void ToggleThermalVision(bool enable)
    {
        Debug.Log("ToggleThermalVision called - Enable: " + enable);
        foreach (Outline outline in enemyOutlines)
        {
            if (outline != null)
            {
                outline.enabled = enable;
                Debug.Log("Outline on " + outline.gameObject.name + " set to " + enable);
            }
        }
    }

    void GravityPulse()
    {
        if (isGravityPull)
        {
            // Pull objects to the hold position
            Collider[] colliders = Physics.OverlapSphere(muzzleTransform.position, gravityPulseRange, layerMask);
            foreach (Collider hit in colliders)
            {
                if (System.Array.Exists(pullableTags, tag => tag == hit.tag))
                {
                    Rigidbody rb = hit.GetComponent<Rigidbody>();
                    if (rb != null && !heldObjects.Contains(rb))
                    {
                        StartCoroutine(PullObjectToHoldPosition(rb));
                    }
                }
            }
        }
        else
        {
            // Push/throw held objects
            if (heldObjects.Count > 0)
            {
                foreach (Rigidbody rb in heldObjects.ToArray()) // Use ToArray to avoid modifying the list while iterating
                {
                    if (rb != null)
                    {
                        // Unparent the object and re-enable physics
                        rb.transform.SetParent(null);
                        rb.isKinematic = false;
                        rb.useGravity = true;

                        // Apply a throw force in the direction the player is facing
                        Vector3 throwDirection = fpsCamera.transform.forward;
                        rb.AddForce(throwDirection * gravityPulseForce * throwForceMultiplier, ForceMode.Impulse);

                        // Remove from held objects
                        heldObjects.Remove(rb);
                    }
                }
            }
        }

        if (shootingAudioSource != null && explosionSound != null)
        {
            shootingAudioSource.PlayOneShot(explosionSound, explosionVolume * 0.5f);
        }
    }

    IEnumerator PullObjectToHoldPosition(Rigidbody rb)
    {
        if (holdPosition == null || rb == null)
        {
            Debug.LogWarning("Hold Position or Rigidbody is null, cannot pull object.");
            yield break;
        }

        // Disable physics while pulling
        rb.isKinematic = true;
        rb.useGravity = false;

        Vector3 startPosition = rb.transform.position;
        float elapsedTime = 0f;
        float journeyLength = Vector3.Distance(startPosition, holdPosition.position);

        // Smoothly move the object to the hold position
        while (elapsedTime < journeyLength / pullSpeed)
        {
            if (rb == null) yield break; // In case the object is destroyed

            elapsedTime += Time.deltaTime;
            float fractionOfJourney = (elapsedTime * pullSpeed) / journeyLength;
            rb.transform.position = Vector3.Lerp(startPosition, holdPosition.position, fractionOfJourney);
            yield return null;
        }

        // Snap to the exact position and parent the object to holdPosition
        if (rb != null)
        {
            rb.transform.position = holdPosition.position;
            rb.transform.SetParent(holdPosition);
            heldObjects.Add(rb);
            Debug.Log("Object " + rb.gameObject.name + " is now held at " + holdPosition.position);
        }
    }

    void ReleaseAllHeldObjects()
    {
        foreach (Rigidbody rb in heldObjects.ToArray())
        {
            if (rb != null)
            {
                rb.transform.SetParent(null);
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }
        heldObjects.Clear();
    }

    void SwitchFireMode()
    {
        switch (currentFireMode)
        {
            case FireMode.Auto:
                if (canUseBurst) currentFireMode = FireMode.Burst;
                else if (canUseSingle) currentFireMode = FireMode.Single;
                break;
            case FireMode.Burst:
                if (canUseSingle) currentFireMode = FireMode.Single;
                else if (canUseAuto) currentFireMode = FireMode.Auto;
                break;
            case FireMode.Single:
                if (canUseAuto) currentFireMode = FireMode.Auto;
                else if (canUseBurst) currentFireMode = FireMode.Burst;
                break;
        }
        ShowFireModeText(currentFireMode.ToString());
    }

    void SwitchAltFire()
    {
        isAltFireActive = !isAltFireActive;
        ShowFireModeText(isAltFireActive ? "Grenade" : currentFireMode.ToString());
        UpdateAmmoUI(); // Update the UI to reflect the new fire mode
    }

    void ShowFireModeText(string mode)
    {
        if (fireModeDisplay != null)
        {
            fireModeDisplay.text = mode;
            fireModeDisplay.gameObject.SetActive(true);
            StartCoroutine(HideFireModeText());
        }
    }

    IEnumerator HideFireModeText()
    {
        yield return new WaitForSeconds(displayDuration);
        if (fireModeDisplay != null) fireModeDisplay.gameObject.SetActive(false);
    }

    IEnumerator BurstFire(float effectiveFireRate)
    {
        isBursting = true;
        int shotsToFire = Mathf.Min(burstCount, currentAmmo);
        for (int i = 0; i < shotsToFire; i++)
        {
            Shoot();
            if (i == shotsToFire - 1)
            {
                SpawnLastShotSmoke();
            }
            if (i < shotsToFire - 1)
            {
                yield return new WaitForSeconds(1f / effectiveFireRate);
            }
        }
        nextTimeToFire = Time.time + 1f / fireRate;
        isBursting = false;
    }

    IEnumerator PlayBulletFallSound()
    {
        yield return new WaitForSeconds(0.5f);
        if (shootingAudioSource != null && bulletFallSounds != null && bulletFallSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, bulletFallSounds.Length);
            shootingAudioSource.PlayOneShot(bulletFallSounds[randomIndex], bulletFallVolume);
        }
    }

    void CancelReload()
    {
        if (isReloading && currentAmmo > 0)
        {
            StopCoroutine(Reload());
            isReloading = false;
            animator.SetBool("Reloading", false);
            if (wasLaserActiveBeforeReload)
            {
                isLaserActive = true;
                laserLine.enabled = true;
            }
            Debug.Log("Reload cancelled!");
            UpdateAmmoUI();
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        wasLaserActiveBeforeReload = isLaserActive;
        if (isLaserActive)
        {
            isLaserActive = false;
            laserLine.enabled = false;
        }
        Debug.Log("Reloading...");
        animator.SetBool("Reloading", true);

        if (shootingAudioSource != null && reloadSound != null)
        {
            shootingAudioSource.PlayOneShot(reloadSound, reloadVolume);
        }

        int initialAmmo = currentAmmo;
        int initialReserve = (int)currentAmmoReserve;

        if (isShotgun)
        {
            while (currentAmmo < maxAmmo && currentAmmoReserve > 0)
            {
                yield return new WaitForSeconds(shotgunReloadTime);
                if (!isReloading) yield break;
                currentAmmo++;
                currentAmmoReserve--;
                UpdateAmmoUI();
                if (shootingAudioSource != null && reloadSound != null)
                {
                    shootingAudioSource.PlayOneShot(reloadSound, reloadVolume);
                }
            }
        }
        else
        {
            yield return new WaitForSeconds(reloadTime - 0.25f);
            if (!isReloading)
            {
                currentAmmo = initialAmmo;
                currentAmmoReserve = initialReserve;
                yield break;
            }
            int ammoNeeded = maxAmmo - currentAmmo;
            float ammoToAdd = Mathf.Min(ammoNeeded, currentAmmoReserve);
            currentAmmo += (int)ammoToAdd;
            currentAmmoReserve -= ammoToAdd;
            yield return new WaitForSeconds(0.25f);
        }

        animator.SetBool("Reloading", false);
        isReloading = false;
        if (wasLaserActiveBeforeReload)
        {
            isLaserActive = true;
            laserLine.enabled = true;
        }
        UpdateAmmoUI();
    }

    IEnumerator ReloadAlt()
    {
        isReloading = true;
        wasLaserActiveBeforeReload = isLaserActive;
        if (isLaserActive)
        {
            isLaserActive = false;
            laserLine.enabled = false;
        }
        Debug.Log("Reloading Grenades...");
        animator.SetBool("Reloading", true);

        if (shootingAudioSource != null && altReloadSound != null)
        {
            shootingAudioSource.PlayOneShot(altReloadSound, altReloadVolume);
        }

        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = maxAltAmmo - currentAltAmmo;
        float ammoToAdd = Mathf.Min(ammoNeeded, currentAltAmmoReserve);
        currentAltAmmo += (int)ammoToAdd;
        currentAltAmmoReserve -= ammoToAdd;

        animator.SetBool("Reloading", false);
        isReloading = false;
        if (wasLaserActiveBeforeReload)
        {
            isLaserActive = true;
            laserLine.enabled = true;
        }
        UpdateAmmoUI();
    }

    void ShootCharged()
    {
        if (currentAmmo <= 0)
            return;

        if (muzzleflashPrefab != null && muzzleTransform != null)
        {
            ParticleSystem muzzleflash = Instantiate(muzzleflashPrefab, muzzleTransform.position, muzzleTransform.rotation);
            muzzleflash.Play();
            Destroy(muzzleflash.gameObject, muzzleflash.main.duration);
        }

        currentAmmo--;
        lastShotTime = Time.time;

        if (currentFireMode == FireMode.Auto)
        {
            currentAccuracy -= accuracyDecreaseRate;
        }
        currentAccuracy = Mathf.Max(currentAccuracy, 0f);

        float chargeMultiplier = Mathf.Lerp(1f, chargedDamageMultiplier, chargeTime / maxChargeTime);
        float forceMultiplier = Mathf.Lerp(1f, chargedForceMultiplier, chargeTime / maxChargeTime);
        float volumeMultiplier = chargeTime >= maxChargeTime ? 1.5f : 1f;

        if (shootingAudioSource != null && shootingSound != null)
        {
            shootingAudioSource.PlayOneShot(shootingSound, shootingVolume * volumeMultiplier);
        }

        if (isShotgun)
        {
            for (int i = 0; i < pelletsPerShot; i++)
            {
                FirePellet();
            }
            if (cockingSound != null)
            {
                shootingAudioSource.PlayOneShot(cockingSound, cockingVolume);
            }
            ApplyShotgunRecoil();
        }
        else if (isSniper)
        {
            FireSingleBullet(chargeMultiplier, forceMultiplier, chargeTime >= maxChargeTime);
            if (cockingSound != null)
            {
                shootingAudioSource.PlayOneShot(cockingSound, cockingVolume);
            }
            animator.SetTrigger("SniperFire");
        }
        else
        {
            FireSingleBullet(chargeMultiplier, forceMultiplier, chargeTime >= maxChargeTime);
            ApplyRecoil();
        }

        ApplyGunRecoil();
        UpdateAmmoUI();

        currentPatternIndex = (currentPatternIndex + 1) % recoilPattern.Length;
        StartCoroutine(PlayBulletFallSound());
    }

    void FireGrenade()
    {
        if (currentAltAmmo <= 0)
            return;

        currentAltAmmo--;
        UpdateAmmoUI();

        if (shootingAudioSource != null && altFireSound != null)
        {
            shootingAudioSource.PlayOneShot(altFireSound, altFireVolume);
        }

        GameObject grenade = Instantiate(grenadePrefab, muzzleTransform.position, muzzleTransform.rotation);
        grenade.transform.Rotate(0f, 90f, 0f, Space.Self);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Vector3 launchDirection = fpsCamera.transform.forward * launchForce + Vector3.up * upwardForce;
            rb.AddForce(launchDirection, ForceMode.Impulse);
        }

        ParticleSystem smoke = null;
        if (smokeParticlePrefab != null)
        {
            smoke = Instantiate(smokeParticlePrefab, grenade.transform.position, Quaternion.identity);
            smoke.transform.SetParent(grenade.transform);
            smoke.Play();
        }

        if (grenadeTrailPrefab != null)
        {
            TrailRenderer trail = Instantiate(grenadeTrailPrefab, grenade.transform.position, Quaternion.identity);
            trail.transform.SetParent(grenade.transform);
        }

        StartCoroutine(ExplodeGrenade(grenade, 2f, smoke));
    }

    IEnumerator ExplodeGrenade(GameObject grenade, float delay, ParticleSystem smoke)
    {
        float elapsedTime = 0f;
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        float initialDelay = 0.9f;
        int bounceCount = 0;

        yield return new WaitForSeconds(initialDelay);

        while (elapsedTime < delay && grenade != null && bounceCount <= maxBounces)
        {
            elapsedTime += Time.deltaTime;
            if (Physics.CheckSphere(grenade.transform.position, 0.1f, layerMask))
            {
                bounceCount++;
                if (bounceCount > maxBounces)
                {
                    break;
                }
            }
            yield return null;
        }

        if (grenade == null) yield break;

        Vector3 explosionPos = grenade.transform.position;

        // Initial explosion damage (uses explosionRadius)
        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(200f);
                Debug.Log("Initial grenade damaged Target: " + hit.name + " for 200");
                ShowHitMarker();
            }
            EnemyAI enemy = hit.transform.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.OnHitByPlayer();
            }
            Rigidbody hitRb = hit.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                hitRb.AddExplosionForce(explosionForce, explosionPos, explosionRadius);
            }
        }

        if (isClusterMode)
        {
            Debug.Log("Cluster mode active, spawning " + clusterCount + " clusters");
            for (int i = 0; i < clusterCount; i++)
            {
                Vector3 clusterOffset = Random.insideUnitSphere * clusterSpread;
                GameObject clusterGrenade = Instantiate(grenadePrefab, explosionPos + clusterOffset, Quaternion.identity);
                Debug.Log("Spawned cluster " + i + " at " + clusterGrenade.transform.position);
                Rigidbody clusterRb = clusterGrenade.GetComponent<Rigidbody>();
                if (clusterRb != null)
                {
                    clusterRb.AddForce(clusterOffset.normalized * 5f, ForceMode.Impulse);
                }
                if (grenadeTrailPrefab != null)
                {
                    TrailRenderer trail = Instantiate(grenadeTrailPrefab, clusterGrenade.transform.position, Quaternion.identity);
                    trail.transform.SetParent(clusterGrenade.transform);
                }
                float explodeDelay = useRandomClusterDelay ? Random.Range(minClusterDelay, maxClusterDelay) : clusterDelay;
                StartCoroutine(ExplodeCluster(clusterGrenade, explodeDelay));
            }
        }

        if (explosionEffectPrefab != null)
        {
            GameObject explosionEffect = Instantiate(explosionEffectPrefab, explosionPos, Quaternion.identity);
            Destroy(explosionEffect, 2f);
        }

        if (shootingAudioSource != null && explosionSound != null)
        {
            shootingAudioSource.PlayOneShot(explosionSound, explosionVolume);
        }

        if (smoke != null)
        {
            smoke.transform.SetParent(null);
            ParticleSystem.MainModule main = smoke.main;
            Destroy(smoke.gameObject, main.duration + 3f);
        }

        Destroy(grenade);
    }

    IEnumerator ExplodeCluster(GameObject clusterGrenade, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (clusterGrenade == null)
        {
            Debug.Log("Cluster grenade destroyed before explosion");
            yield break;
        }
        Vector3 explosionPos = clusterGrenade.transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, clusterExplosionRadius); // Use new cluster radius
        Debug.Log("Cluster exploded at " + explosionPos + ", detected " + colliders.Length + " colliders, radius: " + clusterExplosionRadius);
        foreach (Collider hit in colliders)
        {
            Debug.Log("Cluster hit: " + hit.name);
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(200f);
                Debug.Log("Damaged Target: " + hit.name + " for 200");
                ShowHitMarker();
            }
            EnemyAI enemy = hit.transform.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.OnHitByPlayer();
            }
            Rigidbody hitRb = hit.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                hitRb.AddExplosionForce(explosionForce * 0.5f, explosionPos, clusterExplosionRadius); // Update force radius too
            }
        }
        if (explosionEffectPrefab != null)
        {
            GameObject explosionEffect = Instantiate(explosionEffectPrefab, explosionPos, Quaternion.identity);
            Destroy(explosionEffect, 2f);
        }
        if (shootingAudioSource != null && explosionSound != null)
        {
            shootingAudioSource.PlayOneShot(explosionSound, explosionVolume * 0.5f);
        }
        Destroy(clusterGrenade);
    }

    void FireSingleBullet(float damageMultiplier, float forceMultiplier, bool isFullyCharged)
    {
        Vector3 direction = CalculateNormalDirection();
        if (bulletTrailPrefab != null && muzzleTransform != null)
        {
            TrailRenderer trail = Instantiate(bulletTrailPrefab, muzzleTransform.position, Quaternion.identity);
            if (trail != null)
            {
                StartCoroutine(MoveTrail(trail, direction));
            }
        }

        if (useRicochet)
        {
            StartCoroutine(RicochetShot(fpsCamera.transform.position, direction, damage * damageMultiplier, impactForce * forceMultiplier, maxRicochetBounces));
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(fpsCamera.transform.position, direction, out hit, range, layerMask, QueryTriggerInteraction.Ignore))
            {
                Target targetComponent = hit.transform.GetComponent<Target>();
                if (targetComponent != null)
                {
                    targetComponent.TakeDamage(damage * damageMultiplier);
                    ShowHitMarker();
                }

                EnemyAI enemy = hit.transform.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.OnHitByPlayer();
                    ShowHitMarker();
                }

                RaycastShootablePuzzleTrigger puzzleTrigger = hit.transform.GetComponent<RaycastShootablePuzzleTrigger>();
                if (puzzleTrigger != null)
                {
                    puzzleTrigger.HandlePuzzleHit();
                }

                AimLabTarget aimLabTarget = hit.transform.GetComponent<AimLabTarget>();
                if (aimLabTarget != null)
                {
                    aimLabTarget.OnHit();
                    ShowHitMarker();
                }

                QuantumEnemy quantumEnemy = hit.transform.GetComponent<QuantumEnemy>();
                if (quantumEnemy != null)
                {
                    quantumEnemy.OnHitByPlayer();
                    ShowHitMarker();
                }

                MainShield mainShield = hit.transform.GetComponentInParent<MainShield>();
                if (mainShield != null && hit.transform.CompareTag("MiniShield"))
                {
                    mainShield.CheckMiniShieldHit(hit.transform.gameObject);
                    ShowHitMarker();
                }

                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * impactForce * forceMultiplier);
                }

                if (impactEffectPrefab != null)
                {
                    GameObject impactGO = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactGO, 2f);
                }

                if (isFullyCharged && explosionEffectPrefab != null)
                {
                    GameObject explosionEffect = Instantiate(explosionEffectPrefab, hit.point, Quaternion.identity);
                    Destroy(explosionEffect, 2f);
                    StartCoroutine(ExplosionSlowMo());
                    Collider[] explosionHits = Physics.OverlapSphere(hit.point, explosionRadius, layerMask);
                    foreach (Collider explosionHit in explosionHits)
                    {
                        Target explosionTarget = explosionHit.GetComponent<Target>();
                        if (explosionTarget != null)
                        {
                            explosionTarget.TakeDamage(damage * damageMultiplier * 0.5f);
                        }
                        Rigidbody rb = explosionHit.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.AddExplosionForce(explosionForce * 0.5f, hit.point, explosionRadius);
                        }
                    }
                }
            }
        }
    }

    Vector3 CalculateNormalDirection()
    {
        float accuracyModifier = (currentFireMode == FireMode.Single && !useChargedShot) ? 0f : 1f - currentAccuracy;
        Vector2 patternRecoil = recoilPattern[currentPatternIndex] * patternScale;
        Vector3 recoil = new Vector3(
            patternRecoil.x + (Random.Range(-horizontalRecoil, horizontalRecoil) * accuracyModifier),
            patternRecoil.y + (Random.Range(0f, verticalRecoil) * accuracyModifier),
            0);

        // Reduce spread when scoped (for all guns)
        if (isScoped)
        {
            recoil *= 0.5f; // Reduce spread by 50% when scoped
        }
        // Additional reduction for snipers when scoped
        if (isSniper && fpsCamera.fieldOfView < 30f)
        {
            recoil *= 0.2f; // Further reduce spread by 80% for snipers (cumulative: 0.5 * 0.2 = 0.1)
        }

        return fpsCamera.transform.forward + recoil;
    }

    IEnumerator MoveTrail(TrailRenderer trail, Vector3 direction)
    {
        float time = 0f;
        Vector3 startPosition = trail.transform.position;
        while (time < 1f)
        {
            trail.transform.position = Vector3.Lerp(startPosition, startPosition + direction * range, time);
            time += Time.deltaTime / trail.time;
            yield return null;
        }
        Destroy(trail.gameObject);
    }

    IEnumerator ReloadMinigame()
    {
        isReloading = true;
        wasLaserActiveBeforeReload = isLaserActive;
        if (isLaserActive)
        {
            isLaserActive = false;
            laserLine.enabled = false;
        }
        Debug.Log("Reload Minigame Started - Press " + reloadSequence[0] + " then " + reloadSequence[1]);
        animator.SetBool("Reloading", true);

        if (shootingAudioSource != null && reloadSound != null)
        {
            shootingAudioSource.PlayOneShot(reloadSound, reloadVolume);
        }

        isReloadMinigameActive = true;
        reloadSequenceIndex = 0;
        float timeLeft = reloadTimeWindow;

        while (timeLeft > 0 && reloadSequenceIndex < reloadSequence.Length)
        {
            timeLeft -= Time.deltaTime;
            if (Input.GetKeyDown(reloadSequence[reloadSequenceIndex]))
            {
                reloadSequenceIndex++;
                if (reloadSequenceIndex < reloadSequence.Length)
                {
                    Debug.Log("Next key: " + reloadSequence[reloadSequenceIndex]);
                }
            }
            yield return null;
        }

        int initialAmmo = currentAmmo;
        int initialReserve = (int)currentAmmoReserve;
        float ammoToAdd = Mathf.Min(maxAmmo - currentAmmo, currentAmmoReserve);

        if (reloadSequenceIndex >= reloadSequence.Length)
        {
            currentAmmo += (int)ammoToAdd;
            currentAmmoReserve -= ammoToAdd;
            if (shootingAudioSource != null && reloadSuccessSound != null)
            {
                shootingAudioSource.PlayOneShot(reloadSuccessSound, reloadSuccessVolume);
            }
            Debug.Log("Reload Success!");
        }
        else
        {
            yield return new WaitForSeconds(reloadTime - 0.25f);
            if (!isReloading)
            {
                currentAmmo = initialAmmo;
                currentAmmoReserve = initialReserve;
            }
            else
            {
                currentAmmo += (int)ammoToAdd;
                currentAmmoReserve -= ammoToAdd;
            }
            if (shootingAudioSource != null && reloadFailSound != null)
            {
                shootingAudioSource.PlayOneShot(reloadFailSound, reloadFailVolume);
            }
            Debug.Log("Reload Failed - Too slow!");
        }

        animator.SetBool("Reloading", false);
        isReloading = false;
        isReloadMinigameActive = false;
        if (wasLaserActiveBeforeReload)
        {
            isLaserActive = true;
            laserLine.enabled = true;
        }
        UpdateAmmoUI();
    }

    IEnumerator ExplosionSlowMo()
    {
        Time.timeScale = explosionSlowMoScale;
        yield return new WaitForSecondsRealtime(explosionSlowMoDuration);
        Time.timeScale = 1f;
    }

    IEnumerator RicochetShot(Vector3 position, Vector3 direction, float currentDamage, float currentForce, int bouncesLeft)
    {
        RaycastHit hit;
        if (Physics.Raycast(position, direction, out hit, range, layerMask, QueryTriggerInteraction.Ignore))
        {
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(currentDamage);
                ShowHitMarker();
            }

            EnemyAI enemy = hit.transform.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.OnHitByPlayer();
                ShowHitMarker();
            }

            RaycastShootablePuzzleTrigger puzzleTrigger = hit.transform.GetComponent<RaycastShootablePuzzleTrigger>();
            if (puzzleTrigger != null)
            {
                puzzleTrigger.HandlePuzzleHit();
            }

            AimLabTarget aimLabTarget = hit.transform.GetComponent<AimLabTarget>();
            if (aimLabTarget != null)
            {
                aimLabTarget.OnHit();
                ShowHitMarker();
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * currentForce);
            }

            if (impactEffectPrefab != null)
            {
                GameObject impactGO = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }

            if (bouncesLeft > 0 && hit.collider != null && !hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Enemy"))
            {
                Vector3 reflectDir = Vector3.Reflect(direction.normalized, hit.normal);

                if (bulletTrailPrefab != null)
                {
                    TrailRenderer trail = Instantiate(bulletTrailPrefab, hit.point, Quaternion.identity);
                    StartCoroutine(MoveTrail(trail, reflectDir));
                }

                yield return new WaitForEndOfFrame();
                yield return StartCoroutine(RicochetShot(hit.point + hit.normal * 0.01f, reflectDir, currentDamage * ricochetDamageReduction, currentForce * ricochetDamageReduction, bouncesLeft - 1));
            }
        }
    }

    void FirePellet()
    {
        Vector3 shootDirection = fpsCamera.transform.forward;
        float accuracyModifier = (currentFireMode == FireMode.Single) ? 0f : 1f - currentAccuracy;
        float modifiedSpread = spreadAngle * (1f + accuracyModifier);

        shootDirection.x += Random.Range(-modifiedSpread, modifiedSpread) / 100f;
        shootDirection.y += Random.Range(-modifiedSpread, modifiedSpread) / 100f;

        Vector2 patternRecoil = recoilPattern[currentPatternIndex] * patternScale;
        Vector3 recoil = new Vector3(
            patternRecoil.x + (Random.Range(-horizontalRecoil, horizontalRecoil) * accuracyModifier),
            patternRecoil.y + (Random.Range(-verticalRecoil, verticalRecoil) * accuracyModifier),
            0);

        shootDirection += recoil;

        Debug.Log("FirePellet called - BulletTrailPrefab: " + (bulletTrailPrefab != null) + ", MuzzleTransform: " + (muzzleTransform != null ? muzzleTransform.position.ToString() : "null"));

        if (bulletTrailPrefab != null && muzzleTransform != null)
        {
            TrailRenderer trail = Instantiate(bulletTrailPrefab, muzzleTransform.position, Quaternion.identity);
            Debug.Log("Pellet trail instantiated at " + muzzleTransform.position);
            StartCoroutine(MoveTrail(trail, shootDirection));
        }
        else
        {
            Debug.LogWarning("BulletTrailPrefab or MuzzleTransform is null in FirePellet!");
        }

        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, shootDirection, out hit, range, layerMask, QueryTriggerInteraction.Ignore))
        {
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
                ShowHitMarker();
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }

            if (impactEffectPrefab != null)
            {
                GameObject impactGO = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }
        }
    }

    void ShowHitMarker()
    {
        if (hitMarkerImage != null)
        {
            hitMarkerImage.color = Color.red;
            hitMarkerImage.gameObject.SetActive(true);
            StartCoroutine(FadeHitMarker());
        }
        // Play random hitmarker sound
        if (shootingAudioSource != null && hitMarkerSounds != null && hitMarkerSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, hitMarkerSounds.Length);
            shootingAudioSource.PlayOneShot(hitMarkerSounds[randomIndex], hitMarkerVolume);
            Debug.Log("Hitmarker sound played: " + hitMarkerSounds[randomIndex].name + " at volume: " + hitMarkerVolume);
        }
        else
        {
            Debug.LogWarning("Hitmarker sound not played - AudioSource: " + (shootingAudioSource != null) + ", HitMarkerSounds: " + (hitMarkerSounds != null ? hitMarkerSounds.Length.ToString() : "null"));
        }
    }

    IEnumerator FadeHitMarker()
    {
        float elapsedTime = 0f;
        Color startColor = Color.red;
        Color endColor = Color.white;

        while (elapsedTime < hitMarkerDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / hitMarkerDuration;
            hitMarkerImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        if (hitMarkerImage != null) hitMarkerImage.gameObject.SetActive(false);
    }

    void ToggleLaser()
    {
        isLaserActive = !isLaserActive;
        laserLine.enabled = isLaserActive;
    }

    void UpdateLaser()
    {
        if (isLaserActive && laserLine != null && laserOrigin != null)
        {
            laserLine.SetPosition(0, laserOrigin.position);

            RaycastHit hit;
            Vector3 laserDirection = transform.forward;
            if (Physics.Raycast(laserOrigin.position, laserDirection, out hit, laserMaxLength, layerMask))
            {
                laserLine.SetPosition(1, hit.point);
                if (hit.transform.GetComponent<Target>() != null ||
                    hit.transform.GetComponent<EnemyAI>() != null ||
                    hit.transform.GetComponent<RaycastShootablePuzzleTrigger>() != null)
                {
                    laserLine.startColor = Color.green;
                    laserLine.endColor = Color.green;
                }
                else
                {
                    laserLine.startColor = Color.red;
                    laserLine.endColor = Color.red;
                }
            }
            else
            {
                laserLine.SetPosition(1, laserOrigin.position + laserDirection * laserMaxLength);
                laserLine.startColor = Color.red;
                laserLine.endColor = Color.red;
            }
        }
    }

    void ApplyRecoil()
    {
        Vector2 recoil = recoilPattern[currentPatternIndex];
        Vector3 recoilVector = new Vector3(recoil.x, recoil.y, -recoilKickback);
        transform.localPosition += recoilVector;
        currentPatternIndex = (currentPatternIndex + 1) % recoilPattern.Length;
        lastPatternResetTime = Time.time;
    }

    void ApplyGunRecoil()
    {
        Vector3 gunRecoil = new Vector3(0, 0, recoilKickback);
        transform.localPosition -= gunRecoil;

        // Apply FOV kick (only when not scoped and not zooming)
        if (fpsCamera != null && !isScoped && !scope.IsZooming())
        {
            fpsCamera.fieldOfView = originalFOV + fovKickAmount; // Immediate kick
            Debug.Log("FOV kicked to: " + fpsCamera.fieldOfView);
        }
    }

    void ApplyShotgunRecoil()
    {
        float accuracyModifier = (currentFireMode == FireMode.Single) ? 0f : 1f - currentAccuracy;
        Vector2 patternRecoil = recoilPattern[currentPatternIndex] * patternScale;
        Vector3 shotgunRecoil = new Vector3(
            (patternRecoil.x + Random.Range(-horizontalRecoil, horizontalRecoil)) * 2 * (1f + accuracyModifier),
            (patternRecoil.y + Random.Range(0f, verticalRecoil)) * 2 * (1f + accuracyModifier),
            recoilKickback * 2);

        transform.localPosition -= shotgunRecoil;
    }

    void UpdateAmmoUI()
    {
        // Update the 2D UI ammo display (includes reserve ammo)
        if (ammoDisplay != null)
        {
            if (isAltFireActive)
            {
                ammoDisplay.text = "Grenade: " + currentAltAmmo.ToString() + " / " + Mathf.FloorToInt(currentAltAmmoReserve).ToString();
            }
            else
            {
                ammoDisplay.text = currentAmmo.ToString() + " / " + Mathf.FloorToInt(currentAmmoReserve).ToString();
            }
        }

        // Update the 3D TextMeshPro ammo display on the weapon (current/max only)
        if (weaponAmmoDisplay != null)
        {
            string ammoText;
            if (isAltFireActive)
            {
                ammoText = currentAltAmmo.ToString() + "/" + maxAltAmmo.ToString();
            }
            else
            {
                ammoText = currentAmmo.ToString() + "/" + maxAmmo.ToString();
            }

            // Determine the ammo condition and set the color
            float ammoPercentage = isAltFireActive ? (float)currentAltAmmo / maxAltAmmo : (float)currentAmmo / maxAmmo;
            Color textColor;

            if (ammoPercentage > 0.66f)
            {
                textColor = Color.green; // Green: Above 66%
                weaponAmmoDisplay.text = ammoText; // No "Reload" prompt
            }
            else if (ammoPercentage > 0.33f)
            {
                textColor = Color.yellow; // Yellow: 33% to 66%
                weaponAmmoDisplay.text = ammoText; // No "Reload" prompt
            }
            else
            {
                textColor = Color.red; // Red: Below 33%
                weaponAmmoDisplay.text = ammoText + "\nReload"; // Add "Reload" on a new line
            }

            weaponAmmoDisplay.color = textColor;
        }
    }

    public void RefillAmmo(float ammoAmount)
    {
        if (isAltFireActive)
        {
            currentAltAmmoReserve += ammoAmount;
            currentAltAmmoReserve = Mathf.Clamp(currentAltAmmoReserve, 0, maxAltAmmoReserve);
        }
        else
        {
            currentAmmoReserve += ammoAmount;
            currentAmmoReserve = Mathf.Clamp(currentAmmoReserve, 0, maxAmmoReserve);
        }
        UpdateAmmoUI();
    }

    public float GetCurrentReserveAmmo()
    {
        return isAltFireActive ? currentAltAmmoReserve : currentAmmoReserve;
    }

    public bool IsReloading()
    {
        return isReloading;
    }
}