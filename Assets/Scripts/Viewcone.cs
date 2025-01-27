using UnityEngine;

public class Viewcone : MonoBehaviour
{
    private EnemyAI parent;
    public Vector2 lastPosition;

    private void Awake()
    {
        parent = GetComponentInParent<EnemyAI>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            parent.newState = EnemyAI.State.ATTACK;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            lastPosition = collision.transform.position;
            parent.newState = EnemyAI.State.CHASE;
        }
    }
}
