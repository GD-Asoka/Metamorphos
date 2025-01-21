using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyAI : MonoBehaviour
{
    public Transform start, end;
    public Vector3 destination;
    private Rigidbody2D rb;
    public float moveSpeed = 5f, waitTime = 3f;

    public enum State
    {
        PATROL,
        CHASE,
        ATTACK
    }
    public State currentState, newState;

    private bool isChasing, isAttacking, isPatrolling;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.position = start.position;
        destination = end.position;
    }

    private void Start()
    {
        SetState(State.PATROL);
    }

    private void Update()
    {
        ChangeState(newState);
    }

    private void ChangeState(State newState)
    {
        if(currentState == newState)
            return;
        else
            SetState(newState);
    }

    private void SetState(State state)
    {
        currentState = state;
        ChangeState();
        switch(currentState)
        {
            case State.PATROL:
                StartCoroutine(Patrol());
                break;
            case State.CHASE:
                Chase();
                break;
            case State.ATTACK:
                Attack();
                break;
        }
    }
    private void ChangeState()
    {
        isPatrolling = false;
        isChasing = false;
        isAttacking = false;
        switch(currentState)
        {
            case State.PATROL:
                isPatrolling = true;
                break;
            case State.CHASE:
                isChasing = true;
                break;
            case State.ATTACK:
                isAttacking = true;
                break;
        }
    }

    private IEnumerator Patrol()
    {
        if (!isPatrolling)
            yield break;
        float moveDir;
        float distance = Vector3.Distance(transform.position, destination);
        while(distance > 0.1f)
        {
            moveDir = destination.x - transform.position.x > 0 ? 1 : -1;
            distance = Vector3.Distance(transform.position, destination);
            if(distance < 0.1f && rb.velocity.x != 0)
            {
                rb.velocity = Vector2.zero;
                transform.position = destination;
                destination = destination == start.position ? end.position : start.position;
                moveDir *= -1;
                yield return new WaitForSeconds(waitTime);
            }
            rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);
            yield return null;
        }
    }
    private void Attack()
    {
        if(isAttacking)
        {
            // Attack the player
        }
    }
    private void Chase()
    {
        if(isChasing)
        {
            // Move towards the player
        }
    }
}
