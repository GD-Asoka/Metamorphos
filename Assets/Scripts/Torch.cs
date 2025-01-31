using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : MonoBehaviour, ITriggerable
{
    private readonly int fireHash = Animator.StringToHash("FireTorch");
    private readonly int waterHash = Animator.StringToHash("WaterTorch");
    public enum TorchType { Fire, Water }
    public TorchType torchType;
    private TorchType currentType;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        currentType = torchType;
        ChangeAnimation();
    }

    private void ChangeAnimation(bool toSwitch = false)
    {
        if (toSwitch)
        {
            switch (currentType)
            {
                case TorchType.Fire:
                    currentType = TorchType.Water;
                    break;
                case TorchType.Water:
                    currentType = TorchType.Fire;
                    break;
            }
        }
        switch (currentType)
        {
            case TorchType.Fire:
                anim.SetTrigger(fireHash);
                break;
            case TorchType.Water:
                anim.SetTrigger(waterHash);
                break;
        }
    }

    public void Trigger()
    {
        ChangeAnimation(true);
    }
}
