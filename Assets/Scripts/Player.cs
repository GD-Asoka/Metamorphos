using System;
using System.Collections;
using System.Collections.Generic;
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

    public float moveSpeed = 5f, jumpForce = 100f;
    private float gravity;
    private Vector2 moveDirection;
    public LayerMask groundMask, ceilingMask, platformMask;

    public Sprite druid, animal;
    public GameObject tree, vine;

    private bool canJump = true, climbing = false;
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
        if(Physics2D.Raycast(clickPos, Vector3.forward, 100f, groundMask))
        {
            Instantiate(tree, clickPos, Quaternion.identity);
        }
        else if(Physics2D.Raycast(clickPos, Vector3.forward, 100f, ceilingMask))
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
        input.Enable();
    }
    private void OnDisable()
    {
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
    }
    private void Jump()
    {
        if (canJump)
        {
            rb.AddForce(Vector2.up * jumpForce * jumpVal, ForceMode2D.Impulse);
        }
    }

    private void CheckJump()
    {
        jumpVal = jump.ReadValue<float>();
        if (Physics2D.Raycast(transform.position, Vector2.down, 1f, groundMask) || Physics2D.Raycast(transform.position, Vector2.down, 1f, ceilingMask) || Physics2D.Raycast(transform.position, Vector2.down, 1f, platformMask))
        {
            if (MathF.Abs(rb.velocity.y) < 0.1f)
            {
                canJump = true;
            }
        }        
        else
        {
            canJump = false;
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
            rb.gravityScale = 0;
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
            rb.gravityScale = gravity;
        }
    }
}
