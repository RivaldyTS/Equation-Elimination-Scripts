using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchState : BaseState
{
    private float searchTimer;
    private float searchDuration = 25f; // Adjust the search duration as needed

    public override void Enter()
    {
        Debug.Log("Entering SearchState");

        searchTimer = 0f; // Reset the search timer
        enemy.Agent.SetDestination(enemy.LastKnownPos);
    }

    public override void Perform()
    {
        if (enemy.CanSeePlayer())
        {
            stateMachine.ChangeState(stateMachine.attackState);
            return;
        }

        searchTimer += Time.deltaTime;
        if (searchTimer > searchDuration)
        {
            stateMachine.ChangeState(stateMachine.patrolState);
            return;
        }

        if (enemy.Agent.remainingDistance < enemy.Agent.stoppingDistance)
        {
            // Continue searching
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting SearchState");
    }
}
