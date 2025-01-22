using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public Sprite[] vineSprites, treeSprites;
    public float maxScale, growthRate = 0.1f, animTime = 0.1f;
    private Vector3 actualSize;
    private SpriteRenderer sr;
    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
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
            yield return new WaitForSeconds(animTime);
        }
        transform.localScale = new Vector3(actualSize.x, actualSize.y, actualSize.z);
        int index = 0;
        while(gameObject.activeSelf)
        {
            if(gameObject.tag == "Vine")
            {
                sr.sprite = vineSprites[index];
                index++;
                if(index >= vineSprites.Length)
                    index = 0;
            }
            else
            {
                sr.sprite = treeSprites[index];
                index++;
                if (index >= treeSprites.Length)
                    index = 0;
            }
            yield return new WaitForSeconds(animTime);
        }
    }
}
