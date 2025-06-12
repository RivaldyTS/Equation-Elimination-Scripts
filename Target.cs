using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Target : MonoBehaviour
{
    public float health = 50f;
    private float maxHP;

    public AudioSource audioSource;
    public AudioClip damageSound;
    public AudioClip deathSound;

    public GameObject[] deathEffectPrefabs;
    public GameObject damageEffectPrefab;

    [Header("UI")]
    public Transform uiCanvas; // Assigned Canvas
    public TextMeshProUGUI damageTextPrefab; // Assigned TextMeshProUGUI prefab for damage
    public TextMeshProUGUI hpTextPrefab; // Assigned TextMeshProUGUI prefab for HP info

    // Static fields to manage the single shared HP text instance
    private static TextMeshProUGUI sharedHPTextInstance;
    private static HPInfoText sharedHPInfoTextComponent;

    [Header("Damage Accumulation")]
    public float damageWindow = 0.01f; // Very short window to catch rapid hits (e.g., shotgun pellets)

    [Header("Events")]
    public UnityEvent onEnemyDeath;

    [Header("Shield")]
    public MainShield mainShield; // Assign a GameObject with MainShield component in Inspector
    public bool spawnShield = true; // Toggle to enable/disable shield spawning
    [SerializeField] private float shieldSpawnChance = 0.5f; // 50% chance to spawn shield (0.0 to 1.0)

    private ChasingEnemyAI enemyAI;
    private float accumulatedDamage = 0f;
    private float lastDamageTime = 0f;
    private bool hasPendingDamage = false;
    private bool hasDied = false;
    private bool firstHit = true;

    void Start()
    {
        enemyAI = GetComponent<ChasingEnemyAI>();
        maxHP = health;

        // Initialize the shared HP text instance if it hasn’t been created yet
        if (sharedHPTextInstance == null && uiCanvas != null && hpTextPrefab != null)
        {
            sharedHPTextInstance = Instantiate(hpTextPrefab, uiCanvas);
            sharedHPInfoTextComponent = sharedHPTextInstance.gameObject.AddComponent<HPInfoText>();
            Color color = sharedHPTextInstance.color;
            color.a = 0f;
            sharedHPTextInstance.color = color;
        }

        // Ensure shield starts inactive if assigned
        if (mainShield != null)
        {
            mainShield.gameObject.SetActive(false);
        }
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        if (hasDied) return;
        float previousHealth = health;
        health -= amount;
        float actualDamage = Mathf.Min(amount, previousHealth);
        if (Time.time - lastDamageTime <= damageWindow)
        {
            accumulatedDamage += actualDamage;
            hasPendingDamage = true;
        }
        else
        {
            accumulatedDamage = actualDamage;
            hasPendingDamage = true;
        }
        lastDamageTime = Time.time;
        if (firstHit && spawnShield && mainShield != null)
        {
            firstHit = false;
            if (Random.value <= shieldSpawnChance)
            { 
                mainShield.transform.SetParent(transform); // Attach to this enemy
                mainShield.transform.localPosition = Vector3.zero; // Center on enemy
                mainShield.ActivateShield();
            }
        }
        if (enemyAI != null)
        {
            enemyAI.TakeDamage(amount);
        }

        if (damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        }
        SpawnHPInfoText();
        if (health <= 0f)
        {
            if (hasPendingDamage)
            {
                SpawnDamageText(accumulatedDamage);
                SpawnLastHitText();
                accumulatedDamage = 0f;
                hasPendingDamage = false;
            }
            Die();
        }
        else if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
    }

    void SpawnDamageText(float damageAmount)
    {
        if (uiCanvas == null || damageTextPrefab == null)
        {
            return;
        }

        TextMeshProUGUI textMesh = Instantiate(damageTextPrefab, uiCanvas);
        textMesh.text = Mathf.FloorToInt(damageAmount).ToString();

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        textMesh.transform.position = screenPos;

        FloatingText floatingText = textMesh.gameObject.AddComponent<FloatingText>();
        floatingText.SetText(textMesh);
    }

    void SpawnLastHitText()
    {
        if (uiCanvas == null || damageTextPrefab == null)
        {
            return;
        }

        Vector3 matiPosition = transform.position + Vector3.up * 1f;
        LastHitText.UpdateSharedText(damageTextPrefab, uiCanvas, matiPosition);
    }

    void SpawnHPInfoText()
    {
        if (sharedHPTextInstance == null || sharedHPInfoTextComponent == null)
        {
            return;
        }

        sharedHPInfoTextComponent.SetText(sharedHPTextInstance, gameObject.name, health, maxHP);
    }

    void Die()
    {
        if (hasDied) return;
        hasDied = true;

        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (deathEffectPrefabs.Length > 0)
        {
            foreach (GameObject effect in deathEffectPrefabs)
            {
                if (effect != null)
                {
                    Instantiate(effect, transform.position, Quaternion.identity);
                }
            }
        }

        onEnemyDeath.Invoke();
        Destroy(gameObject);
    }

    void LateUpdate()
    {
        if (hasPendingDamage)
        {
            SpawnDamageText(accumulatedDamage);
            accumulatedDamage = 0f;
            hasPendingDamage = false;
        }
    }
}