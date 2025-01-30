using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
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
    private readonly int attackHash = Animator.StringToHash("attacking");
    private readonly int splashHash = Animator.StringToHash("splashing");
    private readonly int hideHash = Animator.StringToHash("hiding");
    private int currentHash, prevHash;
    private bool flipX;

    private PlayerControls input;
    private InputAction move, jump, interact, fire, altFire, mouse, bird, fish;

    public float moveSpeed = 5f, jumpForce = 100f, groundCheckDist = 2f, attackForce = 1, splashForce = 1;
    public Transform groundCheck;
    public float druidG, birdG, fishG;
    private Vector2 moveDirection;
    public LayerMask groundMask, ceilingMask, platformMask, combinedMask;

    public Sprite druid, animal;
    public GameObject tree, vine, summonPrefab;
    private PlayerGhost ghost = null;
    public int druidPower = 5;
    public ParticleSystem fireParticles, waterParticles, waterPower;

    private bool canJump = true, isClimbing, isJumping, canMove = true, canHide, isHiding, canSummon = true, isGrounded, isBusy;
    public float animTime = 1f, animSpeed = 0.1f, summonTime = 5f;

    public enum PlayerState
    {
        DRUID,
        FIRE,
        WATER
    }
    public PlayerState currentState;
    public PlayerState prevState;

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
    void Update()
    {
        CheckJump();
        CheckMovement();
        if (currentState != PlayerState.DRUID)
            anim.SetBool(idleHash, false);
    }
    private void FixedUpdate()
    {
        if (canMove)
        {
            Move();
            Jump();
        }
    }

    private void Fire(CallbackContext ctx)
    {
        if(currentState != PlayerState.DRUID)
            return;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(mousePos);
        clickPos.z = 0;
        if (Physics2D.OverlapPoint(clickPos, groundMask) && !isBusy)
        {
            Instantiate(tree, clickPos, Quaternion.identity);
            StartCoroutine(QueueAnimation(summonHash, idleHash));
        }
        else if (Physics2D.OverlapPoint(clickPos, ceilingMask) && !isBusy)
        {
            StartCoroutine(QueueAnimation(summonHash, idleHash));
            Instantiate(vine, clickPos, Quaternion.identity);
        }
    }
    private void AltFire(CallbackContext ctx)
    {
        FlipTime();
    }
    private void FishTransform(CallbackContext ctx)
    {
        if (currentState == PlayerState.DRUID)
            ChangeState(PlayerState.WATER);
        else if (currentState == PlayerState.WATER)
            ChangeState(PlayerState.DRUID);
    }
    private void BirdTransform(CallbackContext ctx)
    {
        if (currentState == PlayerState.DRUID)
            ChangeState(PlayerState.FIRE);
        else if (currentState == PlayerState.FIRE)
            ChangeState(PlayerState.DRUID);
    }
    private void Interact(CallbackContext ctx)
    {
        print(currentState);
        if (currentState == PlayerState.FIRE)
        {
            rb.AddForce(flipX ? Vector2.left * attackForce : Vector2.right * attackForce, ForceMode2D.Impulse);
            ChangeAnimation(attackHash);
        }
        else if (currentState == PlayerState.WATER)
        {
            if (waterPower.isPlaying)
                return;
            rb.AddForce(flipX ? new Vector2(-1,1) * splashForce :Vector2.one * splashForce, ForceMode2D.Impulse);
            //ChangeAnimation(splashHash);
            waterPower.Play();
        }
        else
        {
            StartCoroutine(Hide());
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
        if(currentState == newState || isBusy)
            return;
        currentState = newState;
        switch(currentState)
        {
            case PlayerState.DRUID:
                rb.gravityScale = druidG;
                DruidTransform();
                break;
            case PlayerState.FIRE:
                rb.gravityScale = birdG;
                FireTransform();
                break;
            case PlayerState.WATER:
                rb.gravityScale = fishG;
                WaterTransform();
                break;
        }
    }
    private void SummonGhost()
    {
        if (!ghost)
        {
            ghost = Instantiate(summonPrefab, transform.position, Quaternion.identity).GetComponent<PlayerGhost>();
        }
        else
        {
            ghost.gameObject.SetActive(false);
            ghost = null;
            ChangeState(PlayerState.DRUID);
        }
    }
    private void FireTransform()
    {
        SummonGhost();
        //if (currentState == PlayerState.FIRE)
        //{
        //    StartCoroutine(QueueAnimation(fireHash, idleHash));
        //    rb.gravityScale = druidG;
        //    return;
        //}
        //transform.position += new Vector3(0, col.bounds.extents.y * 2, 0);
        StartCoroutine(QueueAnimation(fireHash, birdHash));
        rb.gravityScale = birdG;
    }
    private void WaterTransform()
    {
        SummonGhost();
        //if (currentState == PlayerState.WATER)
        //{
        //    StartCoroutine(QueueAnimation(waterHash, idleHash));
        //    rb.gravityScale = druidG;
        //    return;
        //}
        StartCoroutine(QueueAnimation(waterHash, fishHash));
        rb.gravityScale = fishG;
    }
    private void DruidTransform()
    {
        if(currentState == PlayerState.FIRE)
        {
            StartCoroutine(QueueAnimation(fireHash, idleHash));
        }
        else if(currentState == PlayerState.WATER)
        {
            StartCoroutine(QueueAnimation(waterHash, idleHash));
        }
        else
        {
            ChangeAnimation(idleHash);
        }
        rb.gravityScale = druidG;
    }    
    private void UpdateAnimation()
    {
        //flipX = rb.velocity.x < 0 ? true : false;
        if (flipX)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }
    private void ChangeAnimation(int animToTrigger)
    {
        if (animToTrigger == currentHash || isBusy)
            return;
        if(currentState != PlayerState.DRUID && (animToTrigger == idleHash || animToTrigger == walkHash || animToTrigger == jumpHash || animToTrigger == climbHash || animToTrigger == hideHash || animToTrigger == summonHash))
            return;
        
        currentHash = animToTrigger;        
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
    private IEnumerator QueueAnimation(int initialAnimHash, int finalAnimHash)
    {
        isBusy = true;
        anim.SetBool(idleHash, false);
        anim.SetBool(walkHash, false);
        anim.SetBool(jumpHash, false);
        anim.SetBool(climbHash, false);
        anim.SetBool(summonHash, false);
        anim.SetBool(fireHash, false);
        anim.SetBool(waterHash, false);
        anim.SetBool(birdHash, false);
        anim.SetBool(fishHash, false);
        anim.SetBool(initialAnimHash, true);
        currentHash = initialAnimHash;
        yield return new WaitForSeconds(animTime);
        anim.SetBool(initialAnimHash, false);
        anim.SetBool(finalAnimHash, true);
        currentHash = finalAnimHash;
        isBusy = false;
    }
    private void Move()
    {
        if (isBusy)
            return;
        if (currentState == PlayerState.DRUID)
        {
            if (isClimbing)
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
                //UpdateAnimation();
            }            
        }
        else if (currentState == PlayerState.FIRE)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
            UpdateAnimation();
        }
        else if (currentState == PlayerState.WATER)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
        }
        UpdateAnimation();
    }
    private void Jump()
    {
        if(isBusy) 
            return;
        if (canJump && isJumping && currentState == PlayerState.DRUID)
        {
            ChangeAnimation(jumpHash);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJumping = false;
        }
        else if(isJumping && currentState == PlayerState.FIRE)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
            isJumping = false;
        }
        else if (canJump && isJumping && currentState == PlayerState.WATER)
        {
            rb.AddForce(flipX ? Vector2.one * jumpForce : -1 * Vector2.one * jumpForce, ForceMode2D.Force);
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
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDist);
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, groundCheckDist);
        if (hits.Length <= 0)
        {
            canJump = false;
            return;
        }
        int hitLayer = hit.collider.gameObject.layer;
        if (((1 << hitLayer) & combinedMask) != 0)
        {
            canJump = true;
            isGrounded = true;
        }
        else
        {
            canJump = false;
            isGrounded = false;
        }
        if(!isGrounded && !isClimbing && currentState == PlayerState.DRUID)
        {
            ChangeAnimation(jumpHash);
        }
    }
    private void CheckMovement()
    {
        moveDirection = move.ReadValue<Vector2>();
        if (moveDirection.x > 0)
        {
            flipX = false;
        }
        else if(moveDirection.x < 0)
        {
            flipX = true;
        }
        UpdateAnimation();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Vine") && currentState == PlayerState.DRUID)
        {
            isClimbing = true;
            canJump = false;
            canHide = true;
            ChangeAnimation(climbHash);
        }
        if(collision.CompareTag("Tree") && currentState == PlayerState.DRUID)
        {
            canHide = true;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Vine") && currentState == PlayerState.DRUID)
        {
            isClimbing = true;
            canJump = false;
            canHide = true;
            ChangeAnimation(climbHash);
        }
        if (collision.CompareTag("Tree") && currentState == PlayerState.DRUID)
        {
            canHide = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Vine") && currentState == PlayerState.DRUID)
        {
            isClimbing = false;
            canHide = false;
        }
        if (collision.CompareTag("Tree") && currentState == PlayerState.DRUID)
        {
            canHide = false;
        }
        if(isGrounded && currentState == PlayerState.DRUID)
        {
            ChangeAnimation(idleHash);
        }
        else if(!isGrounded && currentState == PlayerState.DRUID)
        {
            ChangeAnimation(jumpHash);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground") || currentState == PlayerState.DRUID)        
            return;
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
        yield return new WaitForSeconds(animTime);
        ChangeState(PlayerState.DRUID);
    }
    private IEnumerator Hide()
    {
        if (!canHide)
        {
            isHiding = false;
            yield break;
        }
        isHiding = !isHiding;
        canMove = false;
        canJump = false;
        canSummon = false;
        ChangeAnimation(hideHash);
        while(isHiding && canHide)
        {
            yield return null;
        }
        canMove = true;
        canJump = true;
        canSummon = true;
        isHiding = false;
        ChangeAnimation(idleHash);
    }
    private IEnumerator SummonTimer()
    {
        if(currentState == PlayerState.DRUID)
            yield break;
        yield return null;
    }
}
