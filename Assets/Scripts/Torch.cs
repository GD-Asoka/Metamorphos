using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : MonoBehaviour, ITriggerable
{
    private readonly int fireHash = Animator.StringToHash("FireTorch");
    private readonly int waterHash = Animator.StringToHash("WaterTorch");
    public enum TorchType { Fire, Water }
    public TorchType torchType;
    public TorchType currentType;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        currentType = torchType;
    }

    private void Start()
    {
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
                anim.SetBool(fireHash, true);
                anim.SetBool(waterHash, false);
                GameManager.instance.blueFlames--;
                GameManager.instance.redFlames++;
                break;
            case TorchType.Water:
                anim.SetBool(waterHash, true);
                anim.SetBool(fireHash, false);
                GameManager.instance.blueFlames++;
                GameManager.instance.redFlames--;
                break;
        }
    }

    public void Trigger()
    {
        print(Player.instance.currentState);
        if(Player.instance.currentState == Player.PlayerState.WATER && currentType == TorchType.Fire)
            ChangeAnimation(true);
        else if(Player.instance.currentState == Player.PlayerState.FIRE && currentType == TorchType.Water)
            ChangeAnimation(true);
    }
}
