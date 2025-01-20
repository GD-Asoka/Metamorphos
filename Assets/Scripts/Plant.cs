using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public float maxScale, growthRate = 0.1f, animTime = 0.1f;
    private Vector3 actualSize;
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
    }
}
