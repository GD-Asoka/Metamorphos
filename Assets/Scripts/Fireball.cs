using System.Collections;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public Rigidbody2D rb;
    private Player player;
    private Vector2 dir;
    public float speed = 10f, scaleRate = 0.1f, lifespan = 5f;
    private EnemyAI parent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<Player>();
        parent = FindObjectOfType<EnemyAI>();
    }

    private void Start()
    {
        if (!player)
            return;
        dir = (player.transform.position - transform.position).normalized;
        parent.canAttack = false;
        StartCoroutine(DeathTimer());
    }

    private IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(lifespan);
        parent.canAttack = true;
        gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (!player)
            return;
        rb.velocity = dir * speed;
        transform.localScale += new Vector3(scaleRate, scaleRate, 0);
        transform.localScale = new Vector3(Mathf.Clamp(transform.localScale.x, 0, 1), Mathf.Clamp(transform.localScale.y, 0, 1), 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            parent.canAttack = true;
            gameObject.SetActive(false);
        }
    }
}
