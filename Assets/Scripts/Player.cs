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

    private PlayerControls input;
    private InputAction move, jump, interact, fire, altFire, mouse;

    public float moveSpeed = 5f, jumpForce = 100f, groundCheckDist = 2f;
    private float gravity;
    private Vector2 moveDirection;
    public LayerMask groundMask, ceilingMask, platformMask, combinedMask;

    public Sprite druid, animal;
    public GameObject tree, vine;

    private bool canJump = true, climbing = false, isJumping;
    private float jumpVal;

    public Sprite[] fireTransform, waterTransform, waterElemental, fireElemental;
    public float animTime = 1f, animSpeed = 0.1f;

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
        sr.sprite = druid;
        col = GetComponent<Collider2D>();
        input = new PlayerControls();
        gravity = rb.gravityScale;
    }

    private void Start()
    {
        fire.performed += Fire;
        altFire.performed += AltFire;
        interact.performed += Interact;
    }

    private void AltFire(CallbackContext ctx)
    {
        bool rand = Random.value > 0.5f ? true : false;
        if(rand)
            ChangeState(PlayerState.FIRE);
        else
            ChangeState(PlayerState.WATER);
    }
    private void Interact(CallbackContext ctx)
    {
        ChangeState(PlayerState.DRUID);
    }

    private void ChangeState(PlayerState newState)
    {
        currentState = newState;
        switch(currentState)
        {
            case PlayerState.DRUID:
                StartCoroutine(DruidTransform());
                break;
            case PlayerState.FIRE:
                StartCoroutine(FireTransform());
                break;
            case PlayerState.WATER:
                StartCoroutine(WaterTransform());
                break;
        }
    }

    private IEnumerator FireTransform()
    {
        float timeElapsed = 0;
        int index = 0;
        while(timeElapsed <= animTime)
        {
            sr.sprite = fireTransform[index];
            index++;
            if(index >= fireTransform.Length)
                index = 0;
            yield return new WaitForSeconds(animSpeed);
            timeElapsed += animSpeed;
        }
        index = 0;
        while(currentState == PlayerState.FIRE)
        {
            sr.sprite = fireElemental[index];
            index++;
            if(index >= fireElemental.Length)
                index = 0;
            yield return new WaitForSeconds(animSpeed);
        }

    }
    private IEnumerator WaterTransform()
    {
        float timeElapsed = 0;
            int index = 0;
        while (timeElapsed <= animTime)
        {
            sr.sprite = waterTransform[index];
            index++;
            if (index >= waterTransform.Length)
                index = 0;
            yield return new WaitForSeconds(animSpeed);
            timeElapsed += animSpeed;
        }
             index = 0;
        while (currentState == PlayerState.WATER)
        {
            sr.sprite = waterElemental[index];
            index++;
            if (index >= waterElemental.Length)
                index = 0;
            yield return new WaitForSeconds(animSpeed);
        }
    }
    private IEnumerator DruidTransform()
    {
        sr.sprite = druid;
        yield return null;
    }

    private void Fire(CallbackContext ctx)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(mousePos);
        clickPos.z = 0;
        //if(Physics2D.Raycast(clickPos, Vector3.forward, 100f, groundMask))
        //{
        //    Instantiate(tree, clickPos, Quaternion.identity);
        //}
        //else if(Physics2D.Raycast(clickPos, Vector3.forward, 100f, ceilingMask))
        //{
        //    Instantiate(vine, clickPos, Quaternion.identity);
        //}
        if(Physics2D.OverlapPoint(clickPos, groundMask))
        {
            Instantiate(tree, clickPos, Quaternion.identity);
        }
        else if(Physics2D.OverlapPoint(clickPos, ceilingMask))
        {
            Instantiate(vine, clickPos, Quaternion.identity);
        }
    }

    private void OnEnable()
    {
        move = input.Player.Move;
        jump = input.Player.Jump;
        fire = input.Player.Fire;
        altFire = input.Player.AltFire;
        interact = input.Player.Interact;
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
        CheckClicks();
    }
    private void FixedUpdate()
    {
        Move();
        Jump();
    }

    private void Move()
    {
        if(climbing)
        {
            rb.velocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
        }
        else
        {
            rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
        }
        if (moveDirection.x > 0)
        {
            sr.flipX = false;
        }
        else if (moveDirection.x < 0)
        {
            sr.flipX = true;
        }
    }
    private void Jump()
    {
        if (canJump && isJumping)
        {
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
        print($"CanJump: {canJump}");
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
    private void CheckClicks()
    {
          
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Vine"))
        {
            climbing = true;
            canJump = false;
            //rb.gravityScale = 0;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Vine"))
        {
            climbing = true;
            canJump = false;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Vine"))
        {
            climbing = false;
            //rb.gravityScale = gravity;
        }
    }
}
