using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    public float speed = 5f;
    private bool transformed = false;
    public Sprite druid, animal;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();        
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.D))
        {
            rb.AddForce(Vector2.right * speed * Time.deltaTime);
        }  
        if(Input.GetKey(KeyCode.A))
        {
            rb.AddForce(Vector2.left * speed * Time.deltaTime);
        }   
        if(Input.GetKeyDown(KeyCode.Space))
        {
            transformed = !transformed;
            if(transformed)
            {
                sr.sprite = animal;
                rb.gravityScale = 0;
            }
            else
            {
                sr.sprite = druid;
                rb.gravityScale = 1;
            }
        }
        if (transformed)
        {
            if (Input.GetKey(KeyCode.W))
            {
                rb.AddForce(Vector2.up * speed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.S))
            {
                rb.AddForce(Vector2.down * speed * Time.deltaTime);
            }
        }
    }
}
