using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public delegate void EnemyDeathHandler();
    public event EnemyDeathHandler OnDeath; // Declare the event

    private StateMachine stateMachine;
    private NavMeshAgent agent;
    private GameObject player;
    private Vector3 lastKnownPos;
    public NavMeshAgent Agent { get => agent; }
    public GameObject Player { get => player; }
    public Vector3 LastKnownPos { get => lastKnownPos; set => lastKnownPos = value; }

    public Path path;
    public GameObject debugSphere;

    public AudioSource audioSource;
    public AudioClip shootSound;

    [Header("Sight Value")]
    public float sightDistance = 20f;
    public float fieldOfView = 85f;
    public float eyeHeight;
    [Header("Weapon Values")]
    public Transform gunBarrel;
    [Range(0.1f, 10f)]
    public float fireRate;
    [SerializeField]
    private string currentState;

    void Start()
    {
        stateMachine = GetComponent<StateMachine>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");

        if (stateMachine == null)
        {
            Debug.LogError("StateMachine component not found on the GameObject.");
            return;
        }

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on the GameObject.");
            return;
        }

        stateMachine.Initialise();
    }

    void Update()
    {
        if (CanSeePlayer())
        {
            lastKnownPos = player.transform.position;
        }
        currentState = stateMachine.activeState.ToString();
        debugSphere.transform.position = lastKnownPos;
    }

    public bool CanSeePlayer()
    {
        if (player != null)
        {
            if (Vector3.Distance(transform.position, player.transform.position) < sightDistance)
            {
                Vector3 targetDirection = player.transform.position - transform.position - (Vector3.up * eyeHeight);
                float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);
                if (angleToPlayer >= -fieldOfView && angleToPlayer <= fieldOfView)
                {
                    Ray ray = new Ray(transform.position + (Vector3.up * eyeHeight), targetDirection);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo, sightDistance))
                    {
                        if (hitInfo.transform.gameObject == player)
                        {
                            Debug.DrawRay(ray.origin, ray.direction * sightDistance, Color.red);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public void PlayShootSound()
    {
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }

    public void ReturnToPatrolState()
    {
        stateMachine.ReturnToPatrol();
    }

    // Call this method to handle the enemy's death
    public void Die()
    {
        Debug.Log("Enemy is dying");
        OnDeath?.Invoke(); // Trigger the OnDeath event
        Destroy(gameObject); // Destroy the enemy game object
    }

    public void SetPath(Path newPath)
    {
        path = newPath;
        // Setup the enemy to follow the path here
    }
}
