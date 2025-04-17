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
    [Flags]
    public enum BallState : byte
    {
        None = 0,
        Idle = 1,
        Up = 1 << 1,
        Holding = 1 << 2,
        Down = 1 << 3,
        Moving = 1 << 4,
        /// <summary>Force cache this value</summary>
        MarkHolding = 1 << 5
    }
    
    public class Ball : MonoBehaviour
    {
        public ConstBall.BallType ballType;
        public BallState currentState;
        public SpriteRenderer spriteRenderer;

        public TMP_Text textIndex;
        public TMP_Text textState;

        private Bottle preBottle;

        [SerializeField]
        private Bottle _targetBottle;
        
        public Bottle currentBottle;
        public Bottle targetBottle
        {
            get => _targetBottle;
            set => _targetBottle = value;
        }
        
        public int index;

        private Tween _movementTween;

        #region ---Internal State Management---
        
        private void SetState(BallState state)
        {
            var isMarkedHolding = HasState(BallState.MarkHolding);
            currentState = state;
            if (isMarkedHolding)
                AddState(BallState.MarkHolding);
        }

        private void AddState(BallState state)
        {
            if(HasState(state)) return;
            currentState |= state;
        }

        private void RemoveState(BallState state) => currentState &= ~state;

        private bool HasState(BallState state) => currentState.HasFlag(state);

        #endregion
        
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

        private void ResetBottle()
        {
            currentBottle = targetBottle;
            targetBottle = null;
        }

        #region ---Movement Complete Listener---
        
        private void HandleUpCompleted()
        {
            if (targetBottle != null)
            {
                SetState(BallState.Holding);
                Moving();
                return;
            }

            SetState(BallState.Holding);
        }

        private void HandleMovingCompleted()
        {
            // if (targetState == BallState.Holding)
            // {
            //     Tween.PositionAtSpeed(transform, transform.position, targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
            //         .OnComplete(this, ball => ball.currentState = BallState.Holding);
            //     return;
            // }

            if (targetBottle == null)
            {
                Debug.Log("No target bottle");
                return;
            }

            if (HasState(BallState.MarkHolding))
            {
                Debug.Log("Target bottle is has holding");
                SetState(BallState.Holding);
                ResetBottle();
                return;
            }
            
            // Debug.Log("Target bottle is has holding");
            SetState(BallState.Holding);
            MoveDown();
            ResetBottle();
        }

        private void HandleDownCompleted() => SetState(BallState.Idle);

        #endregion
        
        #region ---Public API Handle Movement---
        
        public Ball MarkHolding()
        {
            if (targetBottle == null)
                return this;
            AddState(BallState.MarkHolding);
            return this;
        }

        private void PositionBySpeed<T>(Transform target, Vector3 startValue, Vector3 endValue, T listener, Action<T> onCompleted, Ease ease = Ease.Default) where T : class
        {
            // var prevProgress = _movementTween.isAlive ? _movementTween.progress : 0;
            // var speed = prevProgress > 0 ? GameManager.instance.ballSpeed * prevProgress : GameManager.instance.ballSpeed; 
            var speed = GameManager.instance.ballSpeed; 
            _movementTween.Stop();
            _movementTween = Tween.PositionAtSpeed(target, startValue, endValue, speed, ease)
                .OnComplete(listener, onCompleted);
        }
        
        public void MoveUp()
        {
            if (HasState(BallState.Idle) || HasState(BallState.Down))
            {
                Debug.Log(this.name + "in exist state idle or down");
                SetState(BallState.Up);
                PositionBySpeed(transform, transform.position, currentBottle.GetUpPosPosition(), this, ball => ball.HandleUpCompleted());
                
                // if (targetBottle != null)
                // {
                //     SetState(BallState.Up);
                //     
                //     Tween.StopAll(transform);
                //     Tween.PositionAtSpeed(transform, transform.position, currentBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
                //         .OnComplete(this, ball => ball.HandleUpCompleted());
                //     return;
                // }
                //     
                // SetState(BallState.Up);
                // // PositionBySpeed(transform, transform.position, currentBottle.GetUpPosPosition(), this, ball => ball.HandleUpCompleted());
                //
                // Tween.StopAll(transform);
                // Tween.PositionAtSpeed(transform, transform.position, currentBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
                //     .OnComplete(this, ball => ball.HandleUpCompleted());
                return;
            }

            if (HasState(BallState.Up))
            {
                return;
            }
            
            if (HasState(BallState.Moving))
            {
                Debug.Log(this.name + "in exist state moving");
                ResetBottle();
                
                PositionBySpeed(transform, transform.position, currentBottle.GetUpPosPosition(), this, ball =>
                {
                    ball.SetState(BallState.Holding);
                    
                });
                
                // Tween.StopAll(transform);
                // Tween.PositionAtSpeed(transform, transform.position, targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
                //     .OnComplete(() =>
                //     {
                //         SetState(BallState.Holding);
                //         ResetBottle();
                //     });
            }
            
            // switch (currentState)
            // {
            //     case BallState.Up:
            //         if (targetBottle != null )
            //         {
            //             Tween.StopAll(transform);
            //             Tween.PositionAtSpeed(transform, transform.position, currentBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
            //                 .OnComplete(this, ball => ball.HandleUpCompleted());
            //             return;
            //         }
            //         
            //         return;
            //     
            //     case BallState.Moving:
            //         Tween.StopAll(transform);
            //         Tween.PositionAtSpeed(transform, transform.position, targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
            //             .OnComplete(() =>
            //             {
            //                 AddState(BallState.Holding);
            //                 ResetBottle();
            //             });
            //         return;
            //     
            //     case BallState.Holding:
            //         return;
            //     
            //     case BallState.Idle:
            //     case BallState.Down:
            //         if (targetBottle != null)
            //         {
            //             Tween.StopAll(transform);
            //             currentState = BallState.Up;
            //             Tween.PositionAtSpeed(transform, transform.position, currentBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
            //                 .OnComplete(this, ball => ball.HandleUpCompleted());
            //             return;
            //         }
            //         
            //         Tween.StopAll(transform);
            //         currentState = BallState.Up;
            //         Tween.PositionAtSpeed(transform, transform.position, currentBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
            //             .OnComplete(this, ball => ball.HandleUpCompleted());
            //         
            //         return;
            // }
        }
    
        public void MoveDown()
        {
            if (HasState(BallState.Up) || HasState(BallState.Holding))
            {
                SetState(BallState.Down);
                
                
                if (targetBottle != null)
                {
                    SetState(BallState.Down);
                    PositionBySpeed(transform, transform.position, targetBottle.GetSlotWorldPosition(index), this, ball => ball.HandleDownCompleted());
                    
                    // Tween.StopAll(transform);
                    // Tween.PositionAtSpeed(transform, transform.position, targetBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed)
                    //     .OnComplete(this, ball => ball.HandleDownCompleted());
                    return;
                }
                    
                SetState(BallState.Down);
                PositionBySpeed(transform, transform.position, currentBottle.GetSlotWorldPosition(index), this, ball => ball.HandleDownCompleted());
                // Tween.StopAll(transform);
                // Tween.PositionAtSpeed(transform, transform.position, currentBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed)
                //     .OnComplete(this, ball => ball.HandleDownCompleted());
            }
            // switch (currentState)
            // {
            //     case BallState.Idle:
            //         return;
            //     case BallState.Moving:
            //         return;
            //     
            //     case BallState.Up:
            //     case BallState.Holding:
            //         if (targetBottle != null)
            //         {
            //             SetState(BallState.Down);
            //             Tween.StopAll(transform);
            //             Tween.PositionAtSpeed(transform, transform.position, targetBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed)
            //                 .OnComplete(this, ball => ball.HandleDownCompleted());
            //             return;
            //         }
            //         
            //         SetState(BallState.Down);
            //         Tween.StopAll(transform);
            //         Tween.PositionAtSpeed(transform, transform.position, currentBottle.GetSlotWorldPosition(index), GameManager.instance.ballSpeed)
            //             .OnComplete(this, ball => ball.HandleDownCompleted());
            //         return;
            //     
            //     case BallState.Down:
            //         return;
            // }
        }

        public void Moving()
        {
            if (HasState(BallState.Holding) || HasState(BallState.Moving))
            {
                // SetState(BallState.Moving);
                // PositionBySpeed(transform, transform.position, targetBottle.GetUpPosPosition(), this, ball => ball.HandleMovingCompleted());
                
                if (!ApproximatePoint(transform.position, targetBottle.GetUpPosPosition()))
                {
                    SetState(BallState.Moving);
                    PositionBySpeed(transform, transform.position, targetBottle.GetUpPosPosition(), this, ball => ball.HandleMovingCompleted());
                }
                else
                {
                    HandleMovingCompleted();
                }
                
                // Tween.StopAll(transform);
                // Tween.PositionAtSpeed(transform, transform.position, targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
                //     .OnComplete(this, ball => ball.HandleMovingCompleted());
            }
            
            // switch (currentState)
            // {
            //     case BallState.Idle:
            //         return;
            //     
            //     case BallState.Up:
            //         Debug.Log("Moving case up");
            //         return;
            //     
            //     case BallState.Holding:
            //     case BallState.Moving:
            //         Tween.StopAll(transform);
            //         AddState(BallState.Moving);
            //         Tween.PositionAtSpeed(transform, transform.position, targetBottle.GetUpPosPosition(), GameManager.instance.ballSpeed)
            //             .OnComplete(this, ball => ball.HandleMovingCompleted());
            //         return;
            //     
            //     case BallState.Down:
            //         return;
            // }
        }
        
        public void ChangeBottle(Bottle fromBottle, Bottle target, float delay, int index)
        {
            textIndex.text = index.ToString();
            
            this.currentBottle = fromBottle;
            this.targetBottle = target;
            this.index = index;
            
            if(HasState(BallState.MarkHolding))
                RemoveState(BallState.MarkHolding);
            
            if (HasState(BallState.Down) || HasState(BallState.Idle))
            {
                Tween.Delay(delay).OnComplete(this, ball => MoveUp());
                // MoveUp();
                return;
            }
            
            if (HasState(BallState.Holding) || HasState(BallState.Moving))
            {
                Moving();
                return;
            }
            
            // if (HasState(BallState.Up))
            // {
            //     MoveUp();
            //     return;
            // }
            
            // switch (currentState)
            // {
            //     case BallState.Idle:
            //     case BallState.Down:
            //         MoveUp();
            //         break;
            //     
            //     case BallState.Up:
            //         break;
            //     
            //     case BallState.Holding:
            //     case BallState.Moving:
            //         Moving();
            //         break;
            // }
        }
        
        #endregion
        
        #region ---Helper Methods---

        private bool ApproximatePoint(Vector3 a, Vector3 b)
        {
            return ApproximateFloat(a.x, b.x) && ApproximateFloat(a.y, b.y) && ApproximateFloat(a.z, b.z);
        }
        
        private bool ApproximateFloat(float a, float b, float tolerance = 1E-06f) => Mathf.Abs(a - b) <= tolerance;
        
        #endregion

    }
}