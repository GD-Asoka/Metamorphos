using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyAI : MonoBehaviour
{
    public Transform start, end;
    public Vector3 destination;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    public float moveSpeed = 5f, waitTime = 3f, destCheckDist = 0.5f, attackRate = 2f;
    public GameObject fireball;
    private Viewcone viewcone;
    public bool canAttack = true;

    public Vector3 position;
    public float hearingRange;
    public float reactionThreshold;
    private Player player;

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
        sr = GetComponent<SpriteRenderer>();
        transform.position = start.position;
        destination = end.position;
        player = FindObjectOfType<Player>();
        viewcone = GetComponentInChildren<Viewcone>();
    }
    private void Start()
    {
        SetState(State.PATROL);
        Invoke(nameof(SetWaypoint), 0.5f);
    }
    private void Update()
    {
        ChangeState(newState);
    }

    private void SetWaypoint()
    {
        start.transform.position = new Vector2(start.transform.position.x, transform.position.y);
        end.transform.position = new Vector2(end.transform.position.x, transform.position.y);
    }

    private void ChangeState(State newState)
    {
        print(currentState + " " + newState);
        FlipSprite();
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
                StartCoroutine(Chase(viewcone.lastPosition));
                break;
            case State.ATTACK:
                StartCoroutine(Attack());
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
        while(isPatrolling && distance > destCheckDist)
        {
            moveDir = destination.x - transform.position.x > 0 ? 1 : -1;
            distance = Vector3.Distance(transform.position, destination);
            if(distance <= destCheckDist && rb.velocity.x != 0)
            {
                rb.velocity = Vector2.zero;
                transform.position = destination;
                destination = destination == start.position ? end.position : start.position;
                distance = Vector3.Distance(transform.position, destination);
                yield return new WaitForSeconds(waitTime);
            }
            rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);
            yield return null;
        }
    }
    private IEnumerator Attack()
    {
        rb.velocity = Vector2.zero;
        while(isAttacking)
        {
            FlipSprite(true);
            yield return null;
            if (canAttack)
            {
                Instantiate(fireball, transform.position, Quaternion.identity);
                yield return new WaitForSeconds(attackRate);
            }
        }
    }
    private IEnumerator Chase(Vector2 target)
    {
        rb.velocity = Vector2.zero;
        var dir = target - (Vector2)transform.position;
        while(isChasing)
        {
            rb.velocity = dir.normalized * moveSpeed;
            if (Vector2.Distance(transform.position, target) < 0.5f)
            {
                rb.velocity = Vector2.zero;
                yield return new WaitForSeconds(waitTime);
                newState = State.PATROL;
            }
            yield return null;
        }
    }

    private void FlipSprite(bool lookAtPlayer = false)
    {
        if(lookAtPlayer)
        {
            sr.flipX = player.transform.position.x - transform.position.x > 0 ? false : true;
        }
        if (rb.velocity.x > 0)
        {
            sr.flipX = false;
        }
        else if (rb.velocity.x < 0)
        {
            sr.flipX = true;
        }
        if(sr.flipX)
        {
            viewcone.transform.localScale = new Vector3(-1f, 1, 1);
        }
        else
        {
            viewcone.transform.localScale = new Vector3(1f, 1, 1);
        }
    }

    public bool CanHearSound(SoundSource sound)
    {
        float distance = Vector3.Distance(position, sound.position);

        // Check if the sound is within range
        if (distance > hearingRange)
            return false;

        // Calculate perceived loudness
        float perceivedLoudness = sound.loudness / (1 + distance * distance);

        // Check if the perceived loudness meets the reaction threshold
        return perceivedLoudness >= reactionThreshold;
    }

    public void ReactToSound(SoundSource sound)
    {
        if (CanHearSound(sound))
        {
            Debug.Log($"Enemy at {position} reacts to sound from {sound.position}");
            // Implement reaction logic here
        }
    }
}

