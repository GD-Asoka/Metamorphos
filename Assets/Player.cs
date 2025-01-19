using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;

    private PlayerControls input;
    private InputAction move, jump, interact, fire, altFire, mouse;

    public float moveSpeed = 5f;
    public float jumpForce = 100f;
    private Vector2 moveDirection;
    public LayerMask groundMask;

    public Sprite druid, animal;
    public GameObject tree, vine;

    private bool canJump = true;
    private float jumpVal;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();        
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        input = new PlayerControls();
    }

    private void Start()
    {
        fire.performed += Fire;
        altFire.performed += AltFire;
    }

    private void AltFire(CallbackContext ctx)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(mousePos);
        clickPos.z = 0;
        Instantiate(vine, clickPos, Quaternion.identity);
    }

    private void Fire(CallbackContext ctx)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(mousePos);
        clickPos.z = 0;
        Instantiate(tree, clickPos, Quaternion.identity);
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
        rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
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
        if (Physics2D.Raycast(transform.position, Vector2.down, 1f, groundMask))
        {
            canJump = true;
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
}
