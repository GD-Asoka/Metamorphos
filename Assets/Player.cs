using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;

    private PlayerControls input;
    private InputAction move, jump, interact, fire;

    public float moveSpeed = 5f;
    public float jumpForce = 100f;
    public Sprite druid, animal;
    private Vector2 moveDirection;
    public LayerMask groundMask;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();        
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        input = new PlayerControls();
        move = input.Player.Move;
        jump = input.Player.Jump;
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    void Update()
    {       
        CheckMovement();
        CheckJump();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
    }

    private void CheckMovement()
    {
        moveDirection = move.ReadValue<Vector2>();
    }

    float jumpVal;
    private void CheckJump()
    {
        if (Physics2D.Raycast(transform.position, Vector2.down, 1f, groundMask))
        {
            jumpVal = jump.ReadValue<float>();
            rb.AddForce(Vector2.up * jumpForce * jumpVal, ForceMode2D.Impulse);
        }
    }
}
