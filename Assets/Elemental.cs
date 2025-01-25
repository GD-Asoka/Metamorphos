using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elemental : MonoBehaviour, IExplodable
{
    protected Player parent;
    public float animTime = 1f, animSpeed = 0.1f;
    public Sprite[] fireTransform, fireElemental;
    protected SpriteRenderer sr;
    public GameObject explosion;

    private void OnEnable()
    {
        Player.Interacted += Explode;
    }
    private void OnDisable()
    {
        Player.Interacted -= Explode;
    }
    protected virtual void Awake()
    {
        parent = FindObjectOfType<Player>();
        sr = GetComponent<SpriteRenderer>();
    }
    protected virtual void Start()
    {
        StartCoroutine(Transform());
    }
    protected virtual IEnumerator Transform()
    {
        float timeElapsed = 0;
        int index = 0;
        while (timeElapsed <= animTime)
        {
            sr.sprite = fireTransform[index];
            index++;
            if (index >= fireTransform.Length)
                index = 0;
            yield return new WaitForSeconds(animSpeed);
            timeElapsed += animSpeed;
        }
        index = 0;
        while (gameObject.activeSelf)
        {
            sr.sprite = fireElemental[index];
            index++;
            if (index >= fireElemental.Length)
                index = 0;
            yield return new WaitForSeconds(animSpeed);
        }
    }    
    public void Explode()
    {
        gameObject.SetActive(false);
    }
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        ITriggerable x;
        if (collision.gameObject.TryGetComponent(out x))
        {
            x.Trigger();
            Explode();
        }
        else
        {
            Explode();
        }
    }
}