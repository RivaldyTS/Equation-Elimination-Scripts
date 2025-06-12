using UnityEngine;

public class AttackState : BaseState
{
    private float shootTimer;

    public override void Enter()
    {
        Debug.Log("Entering AttackState");

        shootTimer = 0;
    }

    public override void Perform()
    {
        if (enemy.CanSeePlayer())
        {
            shootTimer += Time.deltaTime;
            enemy.transform.LookAt(enemy.Player.transform);
            if (shootTimer > enemy.fireRate)
            {
                Shoot();
            }
        }
        else
        {
            stateMachine.ChangeState(stateMachine.searchState);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting AttackState");
    }

    private void Shoot()
    {
        // Store reference to the gun barrel
        Transform gunBarrel = enemy.gunBarrel;

        // Instantiate new bullet
        GameObject bullet = GameObject.Instantiate(Resources.Load("Prefabs/Bullet") as GameObject, gunBarrel.position, enemy.transform.rotation);

        // Calculate the direction to player
        Vector3 shootDirection = (enemy.Player.transform.position - gunBarrel.transform.position).normalized;

        // Add force to the rigidbody of the bullet
        bullet.GetComponent<Rigidbody>().linearVelocity = shootDirection * 250;

        // Play shoot sound using enemy's method
        enemy.PlayShootSound();

        shootTimer = 0; // Reset shoot timer
    }
}
