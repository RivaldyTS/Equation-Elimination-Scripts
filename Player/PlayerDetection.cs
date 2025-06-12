using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    public float detectionRange = 10f; // Range to detect enemies
    public TextMeshProUGUI dialogueText; // Reference to the UI text for dialogue

    void Update()
    {
        // Check for enemies in range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange);
        bool enemyInRange = false;
        bool enemyVisible = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                enemyInRange = true;

                // Check if the enemy is visible
                if (IsEnemyVisible(hitCollider.transform))
                {
                    enemyVisible = true;
                    break; // No need to check further if an enemy is visible
                }
            }
        }

        // Update dialogue text based on detection
        if (enemyVisible)
        {
            dialogueText.text = "Musuh Terdeteksi!";
        }
        else if (enemyInRange)
        {
            dialogueText.text = "Area sekitar tidak aman!";
        }
        else
        {
            dialogueText.text = "Area sekitar terdeteksi aman!";
        }
    }

    bool IsEnemyVisible(Transform enemy)
    {
        // Calculate the direction to the enemy
        Vector3 directionToEnemy = (enemy.position - transform.position).normalized;

        // Perform a raycast to check for obstacles
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToEnemy, out hit, detectionRange))
        {
            // Check if the raycast hit the enemy
            if (hit.collider.CompareTag("Enemy"))
            {
                return true; // Enemy is visible
            }
        }

        return false; // Enemy is blocked by an obstacle
    }
}