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
    public static Player instance;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private Animator anim;
    #region animation hashes
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
    #endregion
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

    private bool canJump = true, isClimbing, isJumping, canMove = true, canHide, isGrounded, isBusy, isDead, hasWon;
    public bool isHiding;
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
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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
    float idleTime = 0, idleLimit = 5f;
    void Update()
    {
        if(isDead)
        {
            return;
        }
        CheckJump();
        CheckMovement();
        if (currentState != PlayerState.DRUID)
            anim.SetBool(idleHash, false);
        idleTime += Time.deltaTime;
        if(idleTime >= idleLimit)
        {
            GameManager.instance.PlayPlayerVFX(GameManager.Player_VFX.Bored);
            anim.SetBool(idleHash, false);
            ChangeAnimation(Random.value < 0.5f ? summonHash : jumpHash);
            idleTime = 0;
        }
        if(Input.anyKey)
            idleTime = 0;
    }
    private void FixedUpdate()
    {
        if (isDead || hasWon)
        {
            return;
        }
        if (canMove && !isBusy)
        {
            Move();
            Jump();
        }
    }
    private void Fire(CallbackContext ctx)
    {
        if(currentState != PlayerState.DRUID || isBusy)
            return;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(mousePos);
        clickPos.z = 0;
        if (Physics2D.OverlapPoint(clickPos, groundMask))
        {
            GameManager.instance.PlayPlayerVFX(GameManager.Player_VFX.Tree);
            Instantiate(tree, clickPos, Quaternion.identity);
            StartCoroutine(QueueAnimation(summonHash, idleHash));
        }
        else if (Physics2D.OverlapPoint(clickPos, ceilingMask))
        {
            GameManager.instance.PlayPlayerVFX(GameManager.Player_VFX.Vine);
            StartCoroutine(QueueAnimation(summonHash, idleHash));
            Instantiate(vine, clickPos, Quaternion.identity);
        }
        GameManager.instance.druidPowers++;
    }
    private void AltFire(CallbackContext ctx)
    {
        FlipTime();
        GameManager.instance.druidPowers++;
    }
    private void FishTransform(CallbackContext ctx)
    {
        GameManager.instance.PlayPlayerVFX(GameManager.Player_VFX.Fish);
        if (currentState == PlayerState.DRUID)
            ChangeState(PlayerState.WATER);
        else if (currentState == PlayerState.WATER)
            ChangeState(PlayerState.DRUID);
    }
    private void BirdTransform(CallbackContext ctx)
    {
        GameManager.instance.PlayPlayerVFX(GameManager.Player_VFX.Bird);
        if (currentState == PlayerState.DRUID)
            ChangeState(PlayerState.FIRE);
        else if (currentState == PlayerState.FIRE)
            ChangeState(PlayerState.DRUID);
    }
    private void Interact(CallbackContext ctx)
    {
        StartCoroutine(UseAbility());
    }
    private IEnumerator UseAbility()
    {
        if(currentState == PlayerState.FIRE)
        {
            rb.AddForce(flipX ? Vector2.left * attackForce : Vector2.right * attackForce, ForceMode2D.Impulse);
            ChangeAnimation(attackHash);
            yield return new WaitForSeconds(animTime);
            StartCoroutine(Explode());
        }
        else if(currentState == PlayerState.WATER)
        {
            if (!waterPower.isPlaying)
            {
                rb.AddForce(flipX ? new Vector2(-1, 1) * splashForce : Vector2.one * splashForce, ForceMode2D.Impulse);
                waterPower.Play();
            }
            yield return new WaitForSeconds(animTime);
            StartCoroutine(Explode());
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
        prevState = currentState;
        currentState = newState;
        SummonGhost();
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
        StartCoroutine(SummonTimer());
        GameManager.instance.druidPowers++;
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
        StartCoroutine(QueueAnimation(fireHash, birdHash));
        rb.gravityScale = birdG;
    }
    private void WaterTransform()
    {
        StartCoroutine(QueueAnimation(waterHash, fishHash));
        rb.gravityScale = fishG;
    }
    private void DruidTransform()
    {
        if(prevState == PlayerState.FIRE)
        {
            StartCoroutine(QueueAnimation(fireHash, idleHash));
        }
        else if(prevState == PlayerState.WATER)
        {
            StartCoroutine(QueueAnimation(waterHash, idleHash));
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
        DisableAnimations();
        anim.SetBool(animToTrigger, true);               
    }
    private IEnumerator QueueAnimation(int initialAnimHash, int finalAnimHash)
    {
        isBusy = true;
        DisableAnimations();
        anim.SetBool(initialAnimHash, true);
        currentHash = initialAnimHash;
        yield return new WaitForSeconds(animTime);
        anim.SetBool(initialAnimHash, false);
        anim.SetBool(finalAnimHash, true);
        currentHash = finalAnimHash;
        isBusy = false;
    }
    private void DisableAnimations()
    {
        anim.SetBool(idleHash, false);
        anim.SetBool(attackHash, false);
        anim.SetBool(walkHash, false);
        anim.SetBool(jumpHash, false);
        anim.SetBool(climbHash, false);
        anim.SetBool(summonHash, false);
        anim.SetBool(fireHash, false);
        anim.SetBool(waterHash, false);
        anim.SetBool(birdHash, false);
        anim.SetBool(fishHash, false);
        anim.SetBool(hideHash, false);
        isHiding = false;
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
            }            
        }
        else if (currentState == PlayerState.FIRE)
        {
            rb.velocity = new Vector2(rb.velocity.x + moveDirection.x * moveSpeed * Time.fixedDeltaTime, rb.velocity.y);
        }
        else if (currentState == PlayerState.WATER)
        {
            rb.velocity = new Vector2(rb.velocity.x + moveDirection.x * moveSpeed * Time.fixedDeltaTime, rb.velocity.y);
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
            rb.AddForce(Vector2.up * jumpForce * 0.5f, ForceMode2D.Impulse);
            isJumping = false;
        }
        else if (canJump && isJumping && currentState == PlayerState.WATER)
        {
            rb.AddForce(Vector2.up * jumpForce * 1.3f, ForceMode2D.Impulse);
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
        if(collision.CompareTag("Fireball"))
        {
            if (currentState == PlayerState.DRUID)
            {
                GameManager.instance.PlayPlayerVFX(GameManager.Player_VFX.Hurt);
                StopAllCoroutines();                
                StartCoroutine(Defeat());
            }
            else
            {
                StartCoroutine(Explode());
            }
        }
        if(collision.CompareTag("Door"))
        {
            if(GameManager.instance.CheckWin())
            {
                GameManager.instance.PlayPlayerVFX(GameManager.Player_VFX.Sing);
                StartCoroutine(Win());
            }
        }        
        ITriggerable hit;
        if (collision.TryGetComponent(out hit) && currentState != PlayerState.DRUID)
        {
            hit.Trigger();
            StartCoroutine(Explode());
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
        ITriggerable hit;
        if (collision.gameObject.TryGetComponent(out hit) && currentState != PlayerState.DRUID)
        {
            hit.Trigger();
            StartCoroutine(Explode());
        }
        if (collision.gameObject.CompareTag("Door"))
        {
            if (GameManager.instance.CheckWin())
            {
                StartCoroutine(Win());
            }
        }
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
        ChangeAnimation(hideHash);
        while(isHiding && canHide)
        {
            yield return null;
        }
        canMove = true;
        canJump = true;
        isHiding = false;
        //ChangeAnimation(idleHash);
    }
    private IEnumerator Defeat()
    {
        isBusy = true;
        anim.SetTrigger("defeat");
        DisableAnimations();
        isDead = true;
        yield return new WaitForSeconds(1f);
        GameManager.instance.Restart();
    }
    private IEnumerator Win()
    {        
        isBusy = true;
        anim.SetTrigger("victory");
        DisableAnimations();
        hasWon = true;
        yield return new WaitForSeconds(5f);
        GameManager.instance.LoadNextLevel();        
    }
    private IEnumerator SummonTimer()
    {
        float time = 0;
        while(currentState != PlayerState.DRUID && time < summonTime)
        {
            time += Time.deltaTime;  
            yield return null;
        }
        if (currentState == PlayerState.DRUID)
            yield break;
        StartCoroutine(Explode());
    }
}
