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
    private Animator anim;

    public float moveSpeed = 5f, waitTime = 3f, destCheckDist = 0.5f, attackRate = 2f;
    public GameObject fireball;
    private Viewcone viewcone;
    public bool canAttack = true, isGrounded;

    public Vector3 position;
    public float hearingRange;
    public float reactionThreshold;
    private Player player;

    private readonly int walkHash = Animator.StringToHash("walking");
    private readonly int attackHash = Animator.StringToHash("attacking");
    private readonly int teleportHash = Animator.StringToHash("teleporting");
    private int currentAnimHash;

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
        anim = GetComponent<Animator>();
        //transform.position = start.position;
        destination = end.position;
        player = FindObjectOfType<Player>();
        viewcone = GetComponentInChildren<Viewcone>();
    }
    private void Start()
    {
        //ChangeState(State.PATROL);
        newState = State.PATROL;
        currentState = State.ATTACK;
        Invoke(nameof(SetWaypoint), 0.5f);
    }
    private void Update()
    {
        ChangeState(newState);
        isGrounded = CheckGrounded();
    }
    private bool CheckGrounded()
    {
        Debug.DrawRay(transform.position, Vector2.down * 1.5f, Color.green);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.5f);
        return hit.collider != null;
    }
    private void SetWaypoint()
    {
        start.transform.position = new Vector2(start.transform.position.x, transform.position.y);
        end.transform.position = new Vector2(end.transform.position.x, transform.position.y);
    }   
    private void ChangeState(State newState)
    {
        if(currentState == newState)
            return;
        currentState = newState;
        isPatrolling = false;
        isChasing = false;
        isAttacking = false;
        StopAllCoroutines();
        switch(currentState)
        {
            case State.PATROL:
                isPatrolling = true;
                StartCoroutine(Patrol());
                break;
            case State.CHASE:
                isChasing = true;
                StartCoroutine(Chase(viewcone.lastPosition));
                break;
            case State.ATTACK:
                isAttacking = true;
                StartCoroutine(Attack());
                break;
        }
    }
    private void ChangeAnimation(int animHash)
    {
        if(animHash == currentAnimHash)
            return;
        else
        {
            currentAnimHash = animHash;
            anim.SetBool(walkHash, false);
            anim.SetBool(attackHash, false);
            anim.SetBool(teleportHash, false);
            anim.SetBool(currentAnimHash, true);
        }
    }

    float moveDir = 1;
    bool wait;
    private IEnumerator Patrol()
    {
         if (!isPatrolling)
            yield break;
        float distance = Vector3.Distance(transform.position, destination);
        while(isPatrolling && distance > destCheckDist)
        {
            if(isGrounded)
                ChangeAnimation(walkHash);
            else
                ChangeAnimation(teleportHash);
            //moveDir = destination.x - transform.position.x > 0 ? 1 : -1;
            //distance = Vector3.Distance(transform.position, destination);
            if (wait)
            {
                rb.velocity = Vector2.zero;
                yield return new WaitForSeconds(waitTime);
                wait = false;
            }
             rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);
            FlipSprite();   
            yield return null;
        }
    }
    private IEnumerator Attack()
    {
        rb.velocity = Vector2.zero;
        while(isAttacking)
        {
            ChangeAnimation(attackHash);
            if (player.transform.position.x < transform.position.x)
                flipX = true;
            else if (player.transform.position.x > transform.position.x)
                flipX = false;
            FlipSprite();
            yield return null;
            if (canAttack)
            {
                Instantiate(fireball, transform.position, Quaternion.identity);
                yield return new WaitForSeconds(attackRate);
            }
        }
        //newState = State.PATROL;
    }
    private IEnumerator Chase(Vector2 target)
    {
        rb.velocity = Vector2.zero;
        var dir = target - (Vector2)transform.position;
        var time = 0f;
        while(isChasing)
        {
            time += Time.deltaTime;
            ChangeAnimation(walkHash);
            rb.velocity = dir.normalized * moveSpeed;
            if(player.transform.position.x < transform.position.x)
                flipX = true;
            else if(player.transform.position.x > transform.position.x)
                flipX = false;
            FlipSprite();
            if (Vector2.Distance(transform.position, target) < 1f)
            {
                rb.velocity = Vector2.zero;
                yield return new WaitForSeconds(waitTime);
                newState = State.PATROL;
            }
            if(time >= 5f)
            {
                newState = State.PATROL;
            }
            yield return null;
        }
    }

    private void FlipSprite()
    {
        //if(lookAtPlayer)
        //{
        //    sr.flipX = player.transform.position.x - transform.position.x > 0 ? false : true;
        //}
        if (rb.velocity.y != 0)
        {
            ChangeAnimation(teleportHash);
        }
        if (flipX || rb.velocity.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (!flipX || rb.velocity.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
    bool flipX;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Waypoint"))
        {
            if(collision.transform.position.x > transform.position.x)
            {
                moveDir = -1;
                flipX = true;
            }
            else if (collision.transform.position.x < transform.position.x)
            {
                moveDir = 1;
                flipX = false;
            }
            FlipSprite();
            wait = true;
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

