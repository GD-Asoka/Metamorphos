using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public Sprite[] vineSprites, treeSprites;
    public float maxScale, growthRate = 0.1f, animTime = 0.1f, animRate = 0.1f, lifespan = 5f, minScale = 0.1f;
    private Vector3 actualSize;
    private SpriteRenderer sr;
    private Animator anim;
    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        //anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        actualSize = transform.localScale;
        maxScale = actualSize.y;
        transform.localScale = new Vector3(0.1f, 0.1f, actualSize.z);
        StartCoroutine(Grow());
    }

    private IEnumerator Grow()
    {
        while(transform.localScale.y < maxScale)
        {
            transform.localScale += new Vector3((actualSize.x/actualSize.y) * growthRate, growthRate,0);
            yield return new WaitForSeconds(animRate);
        }
        transform.localScale = new Vector3(actualSize.x, actualSize.y, actualSize.z);
        yield return new WaitForSeconds(lifespan);
        while (transform.localScale.y > minScale)
        {
            transform.localScale -= new Vector3((actualSize.x / actualSize.y) * growthRate, growthRate, 0);
            yield return new WaitForSeconds(animRate);
        }
        gameObject.SetActive(false);
    }
}
