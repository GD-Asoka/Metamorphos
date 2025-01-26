using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGhost : MonoBehaviour
{
    private Animator anim;
    private readonly int summonHash = Animator.StringToHash("summoning");

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        anim.speed = 0.5f;
        anim.SetBool(summonHash, true);
    }
}
