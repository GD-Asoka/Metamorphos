using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private Animator anim;
    private readonly int idleHash = Animator.StringToHash("idling");
    private readonly int walkHash = Animator.StringToHash("walking");
    private readonly int jumpHash = Animator.StringToHash("jumping");
    private readonly int climbHash = Animator.StringToHash("climbing");
    private readonly int summonHash = Animator.StringToHash("summoning");
    private readonly int fireHash = Animator.StringToHash("fire");
    private readonly int waterHash = Animator.StringToHash("water");
    private readonly int birdHash = Animator.StringToHash("bird");
    private readonly int fishHash = Animator.StringToHash("fish");
    private int currentHash;
    private bool flipX;

    private PlayerControls input;
    private InputAction move, jump, interact, fire, altFire, mouse, bird, fish;

    public float moveSpeed = 5f, jumpForce = 100f, groundCheckDist = 2f;
    public Transform groundCheck;
    public float druidG, birdG, fishG;
    private Vector2 moveDirection;
    public LayerMask groundMask, ceilingMask, platformMask, combinedMask;

    public Sprite druid, animal;
    public GameObject tree, vine, summonPrefab;
    private PlayerGhost ghost = null;
    public bool canSummon = true;
    public int druidPower = 5;
    public ParticleSystem fireParticles, waterParticles;

    private bool canJump = true, climbing = false, isJumping, canMove = true;
    private float jumpVal;

    public Sprite[] fireTransform, waterTransform, waterElemental, fireElemental;
    public float animTime = 1f, animSpeed = 0.1f;
    public static event Action Interacted;

    public enum PlayerState
    {
        DRUID,
        FIRE,
        WATER
    }
    public PlayerState currentState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();        
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        sr.sprite = druid;
        input = new PlayerControls();
        druidG = rb.gravityScale;
    }

    private void Start()
    {
        fire.performed += Fire;
        altFire.performed += AltFire;
        fish.performed += FishTransform;
        bird.performed += BirdTransform;
        interact.performed += Interact;
        currentHash = idleHash;
        ChangeAnimation(idleHash);
    }
    private void Fire(CallbackContext ctx)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(mousePos);
        clickPos.z = 0;
        ChangeAnimation(summonHash);
        if (Physics2D.OverlapPoint(clickPos, groundMask))
        {
            Instantiate(tree, clickPos, Quaternion.identity);
        }
        else if (Physics2D.OverlapPoint(clickPos, ceilingMask))
        {
            Instantiate(vine, clickPos, Quaternion.identity);
        }
        ChangeAnimation(idleHash);
    }
    private void AltFire(CallbackContext ctx)
    {
        FlipTime();
    }
    private void FishTransform(CallbackContext ctx)
    {
            ChangeState(PlayerState.WATER);
    }
    private void BirdTransform(CallbackContext ctx)
    {
            ChangeState(PlayerState.FIRE);
    }
    private void Interact(CallbackContext ctx)
    {
        if(currentState == PlayerState.DRUID)
        {

        }
        else if(currentState == PlayerState.FIRE)
        {
            
        }
        else if(currentState == PlayerState.WATER)
        {
            
        }
    }
    private void FlipTime()
    {
        if(Time.timeScale < 1)
        {
            Time.timeScale = 1;
        }
        else
        {
            Time.timeScale = 0.5f;
        }
    }

    private void ChangeState(PlayerState newState)
    {
        currentState = newState;
        switch(currentState)
        {
            case PlayerState.DRUID:
                rb.gravityScale = druidG;
                StartCoroutine(DruidTransform());
                break;
            case PlayerState.FIRE:
                rb.gravityScale = birdG;
                StartCoroutine(FireTransform());
                break;
            case PlayerState.WATER:
                rb.gravityScale = fishG;
                StartCoroutine(WaterTransform());
                break;
        }
    }
    private void SummonGhost()
    {
        ghost = Instantiate(summonPrefab, transform.position, Quaternion.identity).GetComponent<PlayerGhost>();
    }
    private IEnumerator FireTransform()
    {
        SummonGhost();
        transform.position += new Vector3(0, col.bounds.extents.y * 2, 0);
        ChangeAnimation(waterHash);
        yield return new WaitForSeconds(animTime);
        ChangeAnimation(fishHash);
        rb.gravityScale = 0.5f;
    }
    private IEnumerator WaterTransform()
    {
        ChangeAnimation(waterHash);
        yield return new WaitForSeconds(animTime);
        ChangeAnimation(fishHash);
    }
    private IEnumerator DruidTransform()
    {
        sr.sprite = druid;
        ghost.gameObject.SetActive(false);
        ghost = null;
        yield return null;
    }    
    private void UpdateAnimation()
    {
        flipX = rb.velocity.x < 0 ? true : false;
        if (flipX)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }
    private void ChangeAnimation(int animToTrigger)
    {
        if (animToTrigger == currentHash)
            return;
        
        anim.SetBool(idleHash, false);
        anim.SetBool(walkHash, false);
        anim.SetBool(jumpHash, false);
        anim.SetBool(climbHash, false);
        anim.SetBool(summonHash, false);
        anim.SetBool(fireHash, false);
        anim.SetBool(waterHash, false);
        anim.SetBool(birdHash, false);
        anim.SetBool(fishHash, false);
        anim.SetBool(animToTrigger, true);
    }
    private void OnEnable()
    {
        move = input.Player.Move;
        jump = input.Player.Jump;
        fire = input.Player.Fire;
        altFire = input.Player.AltFire;
        interact = input.Player.Interact;
        bird = input.Player.Bird;
        fish = input.Player.Fish;
        jump.started += SetJump;
        jump.canceled += SetJump;
        input.Enable();
    }
    private void OnDisable()
    {
        jump.started -= SetJump;
        jump.canceled -= SetJump;
        input.Disable();
    }

    void Update()
    {        
        CheckJump();
        CheckMovement();
    }
    private void FixedUpdate()
    {
        if(canMove)
        {
            Move();
            Jump();
        }
    }

    private void Move()
    {
        if (currentState == PlayerState.DRUID)
        {
            if (climbing)
            {
                ChangeAnimation(climbHash);
                rb.velocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
            }
            else
            {
                if(rb.velocity.x != 0)
                    ChangeAnimation(walkHash);
                else
                    ChangeAnimation(idleHash);
                rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
                UpdateAnimation();
            }            
        }
        else if(currentState == PlayerState.FIRE)
        {
            rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
            UpdateAnimation();
        }
        else if (currentState == PlayerState.WATER)
        {
            rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
            UpdateAnimation();
        }
        else
        {
            rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
            UpdateAnimation();
        }
    }
    private void Jump()
    {
        if (canJump && isJumping)
        {
            anim.SetBool(jumpHash, true);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJumping = false;
        }
    }
    private void SetJump(CallbackContext ctx)
    {
        if(ctx.started)
        {
            isJumping = true;
        }
        else if(ctx.canceled)
        {
            isJumping = false;
        }
    }

    private void CheckJump()
    {
        Debug.DrawRay(transform.position, Vector2.down * groundCheckDist, Color.red);
        //jumpVal = jump.ReadValue<float>();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDist);
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, groundCheckDist);
        // Check if the raycast hit anything
        if (hits.Length <= 0)
        {
            canJump = false;
            return;
        }
        canJump = false;
        int hitLayer = hit.collider.gameObject.layer;
        if (((1 << hitLayer) & combinedMask) != 0)
        {
            canJump = true;
        }
    }


    private void CheckMovement()
    {
        moveDirection = move.ReadValue<Vector2>();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Vine") && currentState == PlayerState.DRUID)
        {
            climbing = true;
            canJump = false;
            //rb.gravityScale = 0;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Vine") && currentState == PlayerState.DRUID)
        {
            climbing = true;
            canJump = false;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Vine") && currentState == PlayerState.DRUID)
        {
            climbing = false;
            //rb.gravityScale = gravity;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ITriggerable hit;
        if (collision.gameObject.TryGetComponent(out hit))
        {
            hit.Trigger();
        }
        Explode();
    }
    private IEnumerator Explode()
    {
        if(currentState == PlayerState.FIRE)
        {
            fireParticles.Play();
        }
        else if(currentState == PlayerState.WATER)
        {
            waterParticles.Play();
        }
        yield return new WaitForSeconds(0.5f);
        ChangeState(PlayerState.DRUID);
    }
}
