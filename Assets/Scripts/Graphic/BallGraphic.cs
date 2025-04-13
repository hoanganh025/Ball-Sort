using System;
using System.Collections;
using System.Collections.Generic;
using Core.Definition;
using DefaultNamespace;
using UnityEngine;

public class BallGraphic : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Ball ball;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ball = GetComponent<Ball>();
    }

    public void SetColor(ConstBall.BallType type)
    {
        ball.ballType = type;
        if (ConstColor.BallColors.TryGetValue(type, out var color))
        {
            spriteRenderer.color = color;
        }
    }
}
