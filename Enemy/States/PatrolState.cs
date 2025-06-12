using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : BaseState
{
    private int waypointIndex;
    private float waitTimer;

    public override void Enter()
    {
        

        enemy = stateMachine.GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("Enemy component not found on the StateMachine's GameObject.");
            return;
        }

        waypointIndex = 0; // Reset waypoint index
        waitTimer = 0f; // Reset wait timer

        // Set the first destination
        if (enemy.path != null && enemy.path.waypoints.Count > 0)
        {
            enemy.Agent.SetDestination(enemy.path.waypoints[waypointIndex].position);
            
        }
        else
        {
            Debug.LogError("No waypoints found for patrolling.");
        }
    }

    public override void Perform()
    {
        PatrolCycle();

        if (enemy.CanSeePlayer())
        {
            stateMachine.ChangeState(stateMachine.attackState);
        }
    }

    public override void Exit()
    {
        
    }

    private void PatrolCycle()
    {
        if (enemy == null || enemy.Agent == null || enemy.path == null || enemy.path.waypoints.Count == 0)
        {
            Debug.LogError("PatrolCycle: Required components are not assigned or waypoints are missing.");
            return;
        }

        if (enemy.Agent.remainingDistance < 0.2f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer > 3f)
            {
                waypointIndex = (waypointIndex + 1) % enemy.path.waypoints.Count;
                enemy.Agent.SetDestination(enemy.path.waypoints[waypointIndex].position);
                waitTimer = 0f;
            }
        }
    }
}
