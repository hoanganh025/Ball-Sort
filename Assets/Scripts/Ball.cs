using System;
using System.Collections;
using System.Collections.Generic;
using Core.Definition;
using Core.Manager;
using PrimeTween;
using TMPro;
using UnityEngine;
using Color = System.Drawing.Color;

namespace DefaultNamespace
{
    public enum BallState
    {
        Idle = 0,
        Up,
        Holding,
        Down,
        Moving
    }
    
    public class Ball : MonoBehaviour
    {
        public ConstBall.BallType ballType;
        public BallState currentState;
        public SpriteRenderer spriteRenderer;

        public Sequence _movingSequencer;
        
        private readonly Queue<Action> _actionQueue = new();

        public TMP_Text textIndex;
        public TMP_Text textState;

        private Bottle preBottle;

        private Bottle _targetBottle;
        
        public Bottle currentBottle;
        public Bottle targetBottle
        {
            get => _targetBottle;
            set => _targetBottle = value;
        }
        
        public int index;
        
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            textState.text = currentState.ToString();
        }

        public Ball SetType(ConstBall.BallType type)
        {
            ballType = type;
            return this;
        }
        
        public Ball SetColor(ConstBall.BallType type)
        {
            if (ConstColor.BallColors.TryGetValue(type, out var color))
            {
                spriteRenderer.color = color;
            }
            return this;
        }

        private Sequence ExecuteMoveUp(Bottle bottle)
        {
            return Sequence.Create()
                .ChainCallback(() => currentState= BallState.Up)
                .Chain(Tween.PositionAtSpeed(transform, transform.position, bottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                .OnComplete(() => currentState= BallState.Holding);
        }

        private void ResetBottle()
        {
            currentBottle = targetBottle;
            targetBottle = null;
        }

        public void HandleUpCompleted()
        {
            if (targetBottle != null)
            {
                currentState = BallState.Holding;
                Moving();
                return;
            }

            currentState = BallState.Holding;
        }

        public void HandleMovingCompleted()
        {
            // if (targetState == BallState.Holding)
            // {
            //     Tween.PositionAtSpeed(transform, transform.position, targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
            //         .OnComplete(this, ball => ball.currentState = BallState.Holding);
            //     return;
            // }
            
            if (targetBottle != null)
            {
                currentState = BallState.Holding;
                MoveDown();
                
                ResetBottle();
                
                return;
            }
                
        }
        
        public void HandleDownCompleted()
        {
            currentState = BallState.Idle;
        }

        private void MoveUpCaseUp()
        {
            currentState = BallState.Holding;
            Moving();
            currentState = BallState.Holding;
            ResetBottle();
        }

        public void MoveUp()
        {
            switch (currentState)
            {
                case BallState.Up:
                    if (targetBottle != null )
                    {
                        if (targetBottle == GameManager.instance.currentBottle)
                        {
                            Tween.StopAll(transform);
                            Tween.PositionAtSpeed(transform, transform.position, currentBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
                                .OnComplete(this, ball =>
                                {
                                    Tween.PositionAtSpeed(transform, transform.position, targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed);
                                    currentState = BallState.Holding;
                                    ResetBottle();
                                });
                            return;
                        }
                        Tween.StopAll(transform);
                        Tween.PositionAtSpeed(transform, transform.position, currentBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
                            .OnComplete(this, ball => ball.HandleUpCompleted());
                        return;
                    }
                    
                    return;
                
                case BallState.Moving:
                    Tween.StopAll(transform);
                    Tween.PositionAtSpeed(transform, transform.position, targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
                        .OnComplete(() =>
                        {
                            currentState = BallState.Holding;
                            ResetBottle();
                        });
                    return;
                
                case BallState.Holding:
                    return;
                
                case BallState.Idle:
                case BallState.Down:
                    if (targetBottle != null)
                    {
                        Tween.StopAll(transform);
                        currentState = BallState.Up;
                        Tween.PositionAtSpeed(transform, transform.position, currentBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
                            .OnComplete(this, ball => ball.HandleUpCompleted());
                        return;
                    }
                    
                    Tween.StopAll(transform);
                    currentState = BallState.Up;
                    Tween.PositionAtSpeed(transform, transform.position, currentBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
                        .OnComplete(this, ball => ball.HandleUpCompleted());
                    
                    return;
            }
        }
    
        public void MoveDown()
        {
            switch (currentState)
            {
                case BallState.Idle:
                    return;
                case BallState.Moving:
                    return;
                
                case BallState.Up:
                case BallState.Holding:
                    if (targetBottle != null)
                    {
                        Tween.StopAll(transform);
                        currentState = BallState.Down;
                        Tween.PositionAtSpeed(transform, transform.position, targetBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed)
                            .OnComplete(this, ball => ball.HandleDownCompleted());
                        return;
                    }
                    
                    Tween.StopAll(transform);
                    currentState = BallState.Down;
                    Tween.PositionAtSpeed(transform, transform.position, currentBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed)
                        .OnComplete(this, ball => ball.HandleDownCompleted());
                    return;
                
                case BallState.Down:
                    return;
            }
        }

        public void Moving()
        {
            switch (currentState)
            {
                case BallState.Idle:
                    return;
                
                case BallState.Up:
                    Debug.Log("Moving case up");
                    return;
                
                case BallState.Holding:
                case BallState.Moving:
                    Tween.StopAll(transform);
                    currentState = BallState.Moving;
                    Tween.PositionAtSpeed(transform, transform.position, targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
                        .OnComplete(this, ball => ball.HandleMovingCompleted());
                    return;
                
                case BallState.Down:
                    return;
            }
        }
        
        public void ChangeBottle(Bottle fromBottle, Bottle target, float delay, int index)
        {
            textIndex.text = index.ToString();
            
            this.currentBottle = fromBottle;
            this.targetBottle = target;
            this.index = index;
            
            switch (currentState)
            {
                case BallState.Idle:
                case BallState.Down:
                    MoveUp();
                    break;
                
                case BallState.Up:
                    break;
                
                case BallState.Holding:
                case BallState.Moving:
                    Moving();
                    break;
            }
        }
    }
}