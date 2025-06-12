using UnityEngine;
using TMPro;

public class FixedArtilleryStrike : MonoBehaviour
{
    [Header("Artillery Strike Settings")]
    public GameObject artilleryShellPrefab;     
    public GameObject strikeTarget;             
    public int numberOfShells = 10;            
    public float shellInterval = 0.5f;         
    public float strikeDelay = 2f;             
    public float cooldownDuration = 30f;       
    public float aoeRadius = 10f;              
    public float shellSpawnHeight = 50f;       

    [Header("Skill Button")]
    public KeyCode skillButton = KeyCode.E;    

    [Header("UI Settings")]
    public TextMeshProUGUI cooldownText;       
    public string readyText = "Fixed Strike Ready, Press E"; 
    public Color readyColor = Color.green;     
    public Color cooldownColor = Color.white;  

    [Header("Sound Effects")]
    public AudioSource audioSource;            
    public AudioClip strikeRequestSound;       
    public AudioClip strikeInboundSound;       
    public AudioClip cooldownStartSound;       
    public AudioClip cooldownEndSound;         

    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;

    // Event for external scripts to listen to strike success/failure
    public event System.Action<bool> OnStrikeTriggered;

    void Start()
    {
        UpdateCooldownUI();
        if (strikeTarget == null)
        {
            Debug.LogError("Strike Target not assigned!");
        }
    }

    void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                UpdateCooldownUI();
            }
            else
            {
                UpdateCooldownUI();
            }
        }

        if (Input.GetKeyDown(skillButton) && !isOnCooldown && strikeTarget != null)
        {
            LaunchArtilleryStrike();
        }
    }

    // Public void method for Unity Events and external triggering
    public void TriggerStrike()
    {
        if (!isOnCooldown && strikeTarget != null)
        {
            LaunchArtilleryStrike();
            OnStrikeTriggered?.Invoke(true); // Notify listeners of success
        }
        else
        {
            Debug.LogWarning("Cannot trigger strike - on cooldown or no target!");
            OnStrikeTriggered?.Invoke(false); // Notify listeners of failure
        }
    }

    // Public method to set a new target location
    public void SetStrikeTarget(GameObject newTarget)
    {
        strikeTarget = newTarget;
        if (strikeTarget == null)
        {
            Debug.LogWarning("New strike target set to null!");
        }
    }

    // Public method to check availability
    public bool IsStrikeAvailable()
    {
        return !isOnCooldown && strikeTarget != null;
    }

    private void LaunchArtilleryStrike()
    {
        PlaySound(strikeRequestSound);
        StartCoroutine(ArtilleryStrike(strikeTarget.transform.position));
        isOnCooldown = true;
        cooldownTimer = cooldownDuration;
        UpdateCooldownUI();
        PlaySound(cooldownStartSound);
    }

    private System.Collections.IEnumerator ArtilleryStrike(Vector3 targetPosition)
    {
        yield return new WaitForSeconds(strikeDelay);
        PlaySound(strikeInboundSound);

        for (int i = 0; i < numberOfShells; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * aoeRadius;
            Vector3 shellPosition = targetPosition + new Vector3(randomCircle.x, shellSpawnHeight, randomCircle.y);
            GameObject shell = Instantiate(artilleryShellPrefab, shellPosition, Quaternion.identity);
            
            Rigidbody shellRb = shell.GetComponent<Rigidbody>();
            if (shellRb != null)
            {
                Vector3 drift = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
                shellRb.linearVelocity = Vector3.down * 20f + drift;
            }
            yield return new WaitForSeconds(shellInterval);
        }
    }

    void UpdateCooldownUI()
    {
        if (cooldownText != null)
        {
            cooldownText.text = isOnCooldown 
                ? $"Cooldown: {Mathf.CeilToInt(cooldownTimer)} Seconds" 
                : readyText;
            cooldownText.color = isOnCooldown ? cooldownColor : readyColor;
            if (!isOnCooldown) PlaySound(cooldownEndSound);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (strikeTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(strikeTarget.transform.position, aoeRadius);
        }
    }
}