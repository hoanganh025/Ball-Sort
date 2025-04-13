using System.Collections;
using System.Collections.Generic;
using Core.Definition;
using Core.Manager;
using DefaultNamespace;
using PrimeTween;
using Unity.Mathematics;
using UnityEngine;

public static class BallMovement
{
    public static void BallMoveUp(Bottle bottle)
    {
        Ball b = bottle.Peek();
        Debug.Log("Ball up: " + b.ballType);
        if (b == null)
        {
            Debug.Log("not have bottle");
        }
        Tween.Position(b.transform, bottle.GetUpPosPosition(), 0.2f, Ease.InCubic);
    }
    
    public static void BallMoveDown(Bottle bottle)
    {
        int firstIndex = bottle.balls.Count - 1;
        
        Ball b = bottle.Peek();
        if (b == null)
        {
            Debug.Log("not have bottle");
        }
        Tween.Position(b.transform, bottle.GetSlotWorldPosition(firstIndex), 0.2f, Ease.InCubic);
    }
    
    public static void BallMoving(Bottle bottle1, Bottle bottle2)
    {
        Debug.Log("Bottle 1: " + bottle1.bottleID + ", Bottle 2: " + bottle2.bottleID);
        
        Ball b = bottle1.Peek();
        if (b == null)
        {
            Debug.Log("not have bottle");
        }

        Sequence.Create()
            .Chain(Tween.Position(b.transform, bottle2.GetUpPosPosition(), 0.2f, Ease.InCubic))
            .Chain(Tween.Position(b.transform, bottle2.NextTargetPosition(), 0.2f, Ease.InCubic));
    }
     
}
