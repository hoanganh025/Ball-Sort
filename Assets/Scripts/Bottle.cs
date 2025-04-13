using System;
using System.Collections.Generic;
using Core.Definition;
using Core.Manager;
using UnityEngine;

namespace DefaultNamespace
{
    public class Bottle : MonoBehaviour
    {
        private readonly Vector3 offsetPostion = new (0, 0.5f, 0);
        private readonly Vector3 unitPostion = new (0, 1f, 0);
        
        public int bottleID;
        public List<Ball> balls;
        public int maxBall { get; private set; }
        
        private void OnMouseDown()
        {
            GameManager.instance.OnClickBottle(bottleID);
        }

        public void SetScaleBottle(int index)
        {
            Vector3 bottleScale = Vector3.zero;
            int bottleHeight = index * (int) unitPostion.y;
            bottleScale.y = bottleHeight;
            this.transform.localScale = bottleScale;
        }

        public Vector3 GetSlotWorldPosition(int index) => transform.position + index * unitPostion + offsetPostion;

        public Vector3 NextTargetPosition()
        {
            return GetSlotWorldPosition(balls.Count - 1);
        } 

        public Vector3 GetUpPosPosition() => GetSlotWorldPosition(maxBall) + 1.5f * offsetPostion;

        public Vector3 GetDownPosition() => GetSlotWorldPosition(balls.Count - 1);
 
        public Bottle SetMaxBall(int value)
        {
            maxBall = value;
            return this;
        }
        
        public Bottle AddBall(Ball ball)
        {
            if (balls.Count < maxBall)
            {
                // Debug.Log("Ball in bottle before add" + balls.Count);
                balls.Add(ball);
                //Debug.Log("Ball in bottle after add" + balls.Count);
                return this;
            }
            
            Debug.Log(this.bottleID + " is full");
            return this;
        }
        
        public int AddBallReturnPos(Ball ball)
        {
            if (balls.Count < maxBall)
            {
                balls.Add(ball);
                return balls.Count - 1;
            }
            
            Debug.Log(this.bottleID + " is full");
            this.Peek().MoveDown(this);
            return balls.Count - 1;
        }

        public Ball Peek() => balls.Count > 0 ? balls[^1] : null;

        public Ball PopBall()
        {
            if(balls.Count == 0)
                return null;
            
            var result = balls[^1];
            balls.RemoveAt(balls.Count - 1);
            return result;
        }

        public int LastBallIndex() => balls.Count - 1;
    }
}