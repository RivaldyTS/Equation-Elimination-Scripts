using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public BaseState activeState;
    public PatrolState patrolState = new PatrolState();
    public SearchState searchState = new SearchState();
    public AttackState attackState = new AttackState();

    public void Initialise()
    {
        patrolState.stateMachine = this;
        searchState.stateMachine = this;
        attackState.stateMachine = this;

        ChangeState(patrolState);
    }

    void Update()
    {
        if (activeState != null)
        {
            activeState.Perform();
        }
    }

    public void ChangeState(BaseState newState)
    {
        if (activeState != null)
        {
            activeState.Exit();
        }
        activeState = newState;
        if (activeState != null)
        {
            activeState.stateMachine = this;
            activeState.enemy = GetComponent<Enemy>();
            activeState.Enter();
        }
    }

    public void ReturnToPatrol()
    {
        ChangeState(patrolState);
    }
}
