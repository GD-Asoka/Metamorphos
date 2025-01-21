using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSource : MonoBehaviour
{
    public Vector3 position;
    public float loudness;

    public SoundSource(Vector3 position, float loudness)
    {
        this.position = position;
        this.loudness = loudness;
    }
}

