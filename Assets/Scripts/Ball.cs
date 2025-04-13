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

        private void ChangeState(BallState newState)
        {
            currentState= newState;
        }

        private void HandleMovingCompleted(BallState targetState, Action action = null)
        {
            currentState= targetState;
            action?.Invoke();
            if(_actionQueue.TryDequeue(out var result))
                result?.Invoke();
        }

        private Sequence ExecuteMoveUp(Bottle bottle)
        {
            return Sequence.Create()
                .ChainCallback(() => currentState= BallState.Up)
                .Chain(Tween.PositionAtSpeed(transform, transform.position, bottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                .OnComplete(() => currentState= BallState.Holding);
            // .ChainCallback(() => HandleMovingCompleted(BallState.Holding));
        }
        
        public void MoveUp(Bottle bottle)
        {
            if (_movingSequencer.isAlive)
            {
                switch (currentState)
                {
                    case BallState.Idle:
                        if (preBottle != null)
                        {
                            _movingSequencer.Stop();
                            _movingSequencer = Sequence.Create()
                                .ChainCallback(() => currentState= BallState.Up)
                                .Chain(Tween.PositionAtSpeed(transform, transform.position, preBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                                .ChainCallback(() => currentState= BallState.Holding)
                                .ChainDelay(0.1f)
                                .ChainCallback(() => currentState= BallState.Moving)
                                .Chain(Tween.PositionAtSpeed(transform, preBottle.GetUpPosPosition(), bottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                                .ChainCallback(() => currentState= BallState.Holding)
                                .ChainCallback(() => preBottle = null);
                        }
                        else
                        {
                            _movingSequencer = Sequence.Create()
                                .ChainCallback(() => currentState= BallState.Up)
                                .Chain(Tween.PositionAtSpeed(transform, transform.position, bottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                                .ChainCallback(() => currentState= BallState.Holding);
                        }
                            
                        break;
                    
                    case BallState.Up:
                        if (preBottle != null)
                        {
                            _movingSequencer.Stop();
                            _movingSequencer = Sequence.Create()
                                .ChainCallback(() => currentState= BallState.Up)
                                .Chain(Tween.PositionAtSpeed(transform, transform.position, preBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                                .ChainCallback(() => currentState= BallState.Holding)
                                .ChainDelay(0.1f)
                                .ChainCallback(() => currentState= BallState.Moving)
                                .Chain(Tween.PositionAtSpeed(transform, preBottle.GetUpPosPosition(), bottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                                .ChainCallback(() => currentState= BallState.Holding)
                                .ChainCallback(() => preBottle = null);
                        }
                        else
                        {
                            _movingSequencer = Sequence.Create()
                                .ChainCallback(() => currentState= BallState.Up)
                                .Chain(Tween.PositionAtSpeed(transform, transform.position, preBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                                .ChainCallback(() => currentState= BallState.Holding)
                                .ChainDelay(0.1f)
                                .ChainCallback(() => currentState= BallState.Moving)
                                .Chain(Tween.PositionAtSpeed(transform, preBottle.GetUpPosPosition(), bottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                                .ChainCallback(() => currentState= BallState.Holding);
                        }
                        
                        break;
                    
                    case BallState.Holding:
                        break;
                    
                    case BallState.Moving:
                        _movingSequencer.Stop();
                        _movingSequencer = Sequence.Create()
                            .Chain(Tween.PositionAtSpeed(transform, transform.position, bottle.GetUpPosPosition(), GameManager.instance.ballSpeed));
                        break;
                    
                    case BallState.Down:
                        _movingSequencer.Stop();
                        _movingSequencer = ExecuteMoveUp(bottle);
                        break;
                }
            }
            else
            {
                Debug.LogError("Moveup when not alive sequence");
                currentState= BallState.Up;
                _movingSequencer = ExecuteMoveUp(bottle);
            }
        }
    
        public void MoveDown(Bottle bottle)
        {
            if (_movingSequencer.isAlive)
            {
                _movingSequencer.Stop();
                _actionQueue.Clear();

                _movingSequencer = Sequence.Create()
                    .ChainCallback(() => currentState= BallState.Down)
                    .Chain(Tween.PositionAtSpeed(transform, transform.position, bottle.NextTargetPosition(), GameManager.instance.ballSpeed))
                    .ChainCallback(() => currentState= BallState.Idle);
            }
            else
            {
                _movingSequencer = Sequence.Create()
                    .ChainCallback(() => currentState= BallState.Down)
                    .Chain(Tween.PositionAtSpeed(transform, transform.position, bottle.NextTargetPosition(), GameManager.instance.ballSpeed))
                    .ChainCallback(() => currentState= BallState.Idle);
            }
        }
        
        public void ChangeBottle(Bottle fromBottle, Bottle targetBottle, float delay, int index)
        {
            textIndex.text = index.ToString();
            preBottle = fromBottle;
            
            switch (currentState)
            {
                case BallState.Idle:
                    if (_movingSequencer.isAlive)
                    {
                        Debug.LogWarning("Up alive" + this.name);
                        _movingSequencer.Stop();
                        _actionQueue.Clear();
                        
                        _movingSequencer = Sequence.Create()
                            .ChainCallback(() => currentState= BallState.Up)
                            .Chain(Tween.PositionAtSpeed(transform,transform.position, fromBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Holding)
                            .ChainCallback(() => currentState= BallState.Moving)
                            .Chain(Tween.PositionAtSpeed(transform,fromBottle.GetUpPosPosition(), targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Down)
                            .Chain(Tween.PositionAtSpeed(transform,targetBottle.GetUpPosPosition(), targetBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed))
                            .ChainCallback(() => HandleMovingCompleted(BallState.Idle));
                    }
                    else
                    {
                        Debug.LogWarning("Up not alive"+this.name);
                        _movingSequencer = Sequence.Create()
                            .ChainDelay(delay)
                            .ChainCallback(() => currentState= BallState.Up)
                            .Chain(Tween.PositionAtSpeed(transform,transform.position, fromBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Holding)
                            .ChainCallback(() => currentState= BallState.Moving)
                            .Chain(Tween.PositionAtSpeed(transform,fromBottle.GetUpPosPosition(), targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Down)
                            .Chain(Tween.PositionAtSpeed(transform,targetBottle.GetUpPosPosition(), targetBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed))
                            .ChainCallback(() => HandleMovingCompleted(BallState.Idle));
                    }
                    
                    break;
                
                case BallState.Up:
                    // Debug.LogWarning("==============Up=============== " + this.name + " Move to "+ targetBottle + " in "+ index);
                    
                    if (_movingSequencer.isAlive)
                    {
                        Debug.LogWarning("Up alive " + this.name);
                        
                        _movingSequencer.Stop();
                        _actionQueue.Clear();
                        
                        _movingSequencer = Sequence.Create()
                            .Chain(Tween.PositionAtSpeed(transform,transform.position, fromBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Moving)
                            .Chain(Tween.PositionAtSpeed(transform,fromBottle.GetUpPosPosition(), targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Down)
                            .Chain(Tween.PositionAtSpeed(transform,targetBottle.GetUpPosPosition(), targetBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed))
                            .ChainCallback(() => HandleMovingCompleted(BallState.Idle));
                    }
                    else
                    {
                        Debug.LogWarning("Up not alive " + this.name);
                        
                        _movingSequencer = Sequence.Create()
                            .Chain(Tween.PositionAtSpeed(transform,transform.position, fromBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Moving)
                            .Chain(Tween.PositionAtSpeed(transform,fromBottle.GetUpPosPosition(), targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Down)
                            .Chain(Tween.PositionAtSpeed(transform,targetBottle.GetUpPosPosition(), targetBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed))
                            .ChainCallback(() => HandleMovingCompleted(BallState.Idle));
                    }
                    
                    break;
                
                case BallState.Holding:
                    // Debug.LogWarning("==============Holding=============== " + this.name + " Move to "+ targetBottle + " in "+ index);
        
                    if (_movingSequencer.isAlive)
                    {
                        Debug.LogWarning("Holding alive" + this.name);
                        _movingSequencer.Stop();
                        _actionQueue.Clear();
                        
                        _movingSequencer = Sequence.Create()
                            .ChainCallback(() => currentState= BallState.Moving)
                            .Chain(Tween.PositionAtSpeed(transform,transform.position, fromBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Holding)
                            .ChainDelay(0.1f)
                            .ChainCallback(() => currentState= BallState.Moving)
                            .Chain(Tween.PositionAtSpeed(transform,fromBottle.GetUpPosPosition(), targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Down)
                            .Chain(Tween.PositionAtSpeed(transform,targetBottle.GetUpPosPosition(), targetBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed))
                            .ChainCallback(() => HandleMovingCompleted(BallState.Idle));
                    }
                    else
                    {
                        Debug.LogWarning("Holding not alive" + this.name);
                        _movingSequencer = Sequence.Create()
                            .ChainCallback(() => currentState= BallState.Moving)
                            .Chain(Tween.PositionAtSpeed(transform,transform.position, fromBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Holding)
                            .ChainDelay(0.1f)
                            .ChainCallback(() => currentState= BallState.Moving)
                            .Chain(Tween.PositionAtSpeed(transform,fromBottle.GetUpPosPosition(), targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Down)
                            .Chain(Tween.PositionAtSpeed(transform,targetBottle.GetUpPosPosition(), targetBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed))
                            .ChainCallback(() => HandleMovingCompleted(BallState.Idle));
                    }
                   
                    break;
                
                case BallState.Down:
                    
                    if (_movingSequencer.isAlive)
                    {
                        _movingSequencer.Stop();
                        _actionQueue.Clear();
                        
                        Debug.LogWarning("Down alive" + this.name);
                        _movingSequencer = Sequence.Create()
                            .ChainCallback(() => currentState= BallState.Up)
                            .Chain(Tween.PositionAtSpeed(transform,transform.position, fromBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Holding)
                            .Chain(Tween.PositionAtSpeed(transform,targetBottle.GetUpPosPosition(), targetBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed))
                            .ChainCallback(() => HandleMovingCompleted(BallState.Idle));
                    }
                    else
                    {
                        Debug.LogWarning("Down not alive" + this.name);
                        _movingSequencer = Sequence.Create()
                            .ChainCallback(() => currentState= BallState.Up)
                            .Chain(Tween.PositionAtSpeed(transform,transform.position, fromBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Holding)
                            .Chain(Tween.PositionAtSpeed(transform,targetBottle.GetUpPosPosition(), targetBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed))
                            .ChainCallback(() => HandleMovingCompleted(BallState.Idle));
                    }
                    
                    break;
                
                case BallState.Moving:
                    // Debug.LogWarning("==============Moving=============== " + this.name + " Move to "+ targetBottle + " in "+ index);
                    
                    if (_movingSequencer.isAlive)
                    {
                        Debug.LogWarning("Moving alive" + this.name);
                        _movingSequencer.Stop();
                        _actionQueue.Clear();
                        
                        _movingSequencer = Sequence.Create()
                            // .Chain(Tween.PositionAtSpeed(transform,transform.position, fromBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            // .ChainCallback(() => currentState= BallState.Moving)
                            .Chain(Tween.PositionAtSpeed(transform,transform.position, targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Down)
                            .Chain(Tween.PositionAtSpeed(transform,targetBottle.GetUpPosPosition(), targetBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed))
                            .ChainCallback(() => HandleMovingCompleted(BallState.Idle));
                    }
                    else
                    {
                        Debug.LogWarning("Moving not alive"+this.name);
                        _movingSequencer = Sequence.Create()
                            .Chain(Tween.PositionAtSpeed(transform,transform.position, fromBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Moving)
                            .Chain(Tween.PositionAtSpeed(transform,fromBottle.GetUpPosPosition(), targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed))
                            .ChainCallback(() => currentState= BallState.Down)
                            .Chain(Tween.PositionAtSpeed(transform,targetBottle.GetUpPosPosition(), targetBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed))
                            .ChainCallback(() => HandleMovingCompleted(BallState.Idle));
                    }
                    
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}