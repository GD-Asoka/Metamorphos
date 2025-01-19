using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;

    private PlayerControls input;
    private InputAction move, jump, interact, fire, altFire;

    public float moveSpeed = 5f;
    public float jumpForce = 100f;
    private bool transformed = false;
    public Sprite druid, animal;
    private Vector2 moveDirection;
    public LayerMask groundMask;

    private bool canJump = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();        
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        input = new PlayerControls();
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        moveDirection = move.ReadValue<Vector2>();        
        CheckJump();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
    }

    float jumpVal;
    private void CheckJump()
    {
        jumpVal = jump.ReadValue<float>();
        if (Physics2D.Raycast(transform.position, Vector2.down, 1f, groundMask))
        {
            rb.AddForce(Vector2.up * jumpForce * jumpVal, ForceMode2D.Impulse);
        }
    }


}
