using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Core.Definition;
using DefaultNamespace;
using PrimeTween;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Core.Manager
{
    public struct SwitchBallCommand
    {
        public int fromBottleIndex;
        public int fromBallIndex;
        
        public int toBottleIndex;
        public int toBallIndex;
    }
    
    public class GameManager : MonoBehaviour
    {
        [Header("General")]
        public int selectedBottleIndex = -1;

        [Header("Bottle")]
        public Vector3 PosInitBottle = new Vector3(-3, -2, 0);
        public GameObject Prf_Bottle;
        public List<Bottle> bottles;
        public Bottle currentBottle;
        public Bottle targetBottle;
        
        [Header("Ball")]
        public GameObject Prf_Ball;
        public float ballSpeed = 10f;
        
        public TMP_Text  winText;
        
        public static GameManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            FitScreen();
            winText.enabled = false;
            InitBottle(5);
        }
        
        private void FitScreen()
        {
            Debug.Log("Fit screen");
            //Only fit when current aspect less than default aspect
            Camera cam = Camera.main!;
            const float defaultAspect = 9f / 16f;
            float currentAspect = cam.aspect;

            if (currentAspect > defaultAspect)
                return;
    
            cam.orthographicSize *= defaultAspect / currentAspect;
        }

        public List<SwitchBallCommand> CheckSwitchBall(int bottleIndex1, int bottleIndex2)
        {
            var commands = new List<SwitchBallCommand>();
            
            var bottle1 = bottles[bottleIndex1];
            var bottle2 = bottles[bottleIndex2];
            
            var bottle1Balls = bottle1.balls;
            var bottle2Balls = bottle2.balls;

            if (bottle1Balls.Count == 0 || bottle2Balls.Count == 4)
            {
                return commands;
            }

            var topBallIndexInBottle1 = bottle1Balls.Count - 1;
            var topBallIndexInBottle2 = bottle2Balls.Count - 1;
            var ballSample = bottle1Balls[topBallIndexInBottle1];

            //Handle ball type in different bottle
            if (bottle2Balls.Count > 0 && bottle2Balls[topBallIndexInBottle2].ballType != ballSample.ballType)
            {
                 return commands;
            }

            //Index of top ball +1 in bottle 2
            var targetIndex = bottle2Balls.Count;

            //Handle current ball in same bottle
            for (var i = topBallIndexInBottle1; i >= 0; i--)
            {
                //Get each ball in bottle 1
                var ball = bottle1Balls[i];
                
                if (ball.ballType == ballSample.ballType)
                {
                    commands.Add(new SwitchBallCommand
                    {
                        fromBottleIndex = bottleIndex1,
                        fromBallIndex = i,
                        toBottleIndex = bottleIndex2,
                        toBallIndex = targetIndex
                    });

                    targetIndex++;

                    if (targetIndex == 4)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            
            return commands;
        }

        private void InitBottle(int numBottle)
        {
            float yPos = -2f;
            float zPos = 0f;

            float padding = 0.5f;
            
            Vector3 leftEdge = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0.5f, Camera.main.nearClipPlane));
            Vector3 rightEdge = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0.5f, Camera.main.nearClipPlane));
            leftEdge.z = 0f;
            rightEdge.z = 0f;
            
            float totalWidth = rightEdge.x - leftEdge.x - 2 * padding;
            float step = totalWidth / (numBottle - 1);
            
            //Create all bottle
            for (var i = 0; i < numBottle; i++)
            {
                float xPos = leftEdge.x + padding + step * i;
                Vector3 spawnPos = new Vector3(xPos, yPos, zPos);
                
                var bottleGameObject = Instantiate(Prf_Bottle, spawnPos, quaternion.identity);
                var bottle = bottleGameObject.GetComponent<Bottle>();
                
                bottle.name = bottle.name+i.ToString();
                bottle.SetMaxBall(4);
                bottle.bottleID = i;
                
                bottles.Add(bottle);
            }
            
            var bottleHasBall = numBottle - 2;
            
            //Init and shuffle ball
            var balls = BallShuffle(InitBall(bottleHasBall * 4));
            // var balls = InitBall(bottleHasBall * 4);

            //Add ball to bottle
            var bottleIndex = 0;

            for (var i = 0; i < balls.Count; i++)
            {
                //If current bottle is full, switch to next bottle
                if (bottles[bottleIndex].balls.Count == bottles[bottleIndex].maxBall)
                {
                    bottleIndex++;
                }
                
                bottles[bottleIndex].AddBall(balls[i]);
                //Set ball info
                balls[i].currentBottle = bottles[bottleIndex];
                balls[i].index = bottles[bottleIndex].LastBallIndex();
                //Set position for ball in bottle
                balls[i].transform.position = bottles[bottleIndex].GetSlotWorldPosition(bottles[bottleIndex].LastBallIndex());
            }
        }

        private List<Ball> InitBall(int numBall)
        {
            var ballTypes = new List<ConstBall.BallType>
            {
                ConstBall.BallType.PurpleBall,
                ConstBall.BallType.BlueBall,
                ConstBall.BallType.GreenBall
            };

            var balls = new List<Ball>();

            foreach (var bT in ballTypes)
            {
                for (var i = 0; i < 4; i++)
                {
                    var ballInstantiate = Instantiate(Prf_Ball, this.transform.position, quaternion.identity);

                    var ball = ballInstantiate.GetComponent<Ball>();
                    ball.name = ball.name+bT.ToString()+i.ToString();
                    ball.textIndex.text = i.ToString();
                    ball.SetType(bT);
                    ball.SetColor(bT);
                    ball.currentState = BallState.Idle;
                    
                    balls.Add(ball);
                }
            }

            return balls;
        }

        private List<Ball> BallShuffle(List<Ball> listBalls)
        {
            var listCount = listBalls.Count;
            
            while (listCount > 1)
            {
                listCount--;
                var k = Random.Range(0, listCount + 1);
                (listBalls[k], listBalls[listCount]) = (listBalls[listCount], listBalls[k]);
            }

            return listBalls;
        }

        public void SwitchBall(Bottle bottle1, Bottle bottle2)
        {
            var bottle1Balls = bottle1.balls;
            var bottle2Balls = bottle2.balls;

            if (bottle1Balls.Count == 0 || bottle2Balls.Count == 4)
            {
                return;
            }

            var topBallIndexInBottle1 = bottle1Balls.Count - 1;
            var topBallIndexInBottle2 = bottle2Balls.Count - 1;
            var ballSample = bottle1Balls[topBallIndexInBottle1];

            //Handle ball type in different bottle
            if (bottle2Balls.Count > 0 && bottle2Balls[topBallIndexInBottle2].ballType != ballSample.ballType)
            {
                return;
            }

            //Handle current ball in same bottle
            for (var i = topBallIndexInBottle1; i >= 0; i--)
            {
                //Debug.Log("===================="+bottle1.name + bottle1Balls.Count);
                //Get each ball in bottle 1
                if (bottle1Balls[i].ballType != ballSample.ballType)
                    break;
                
                if (bottle2Balls.Count == 4)
                    break;
                
                Ball b = bottle1.PopBall();
                int index = bottle2.AddBallReturnPos(b);
                
                //b.ChangeBottle(bottle1, bottle2, 0.7f - (i * 0.2f), index);
                b.ChangeBottle(bottle1, bottle2, 0.4f - (i * 0.1f), index);
                
                if (GameManager.instance.CheckWinCondition())
                {
                    winText.enabled = true;
                }
            }
        }
        
        public void OnClickBottle(int bottleIndex)
        {
            bool bottleSameType = false;
            
            if (bottles[bottleIndex].balls.Count == 4)
            {
                var ballSample = bottles[bottleIndex].balls[0];
                if (ballSample is not null)
                {
                    bottleSameType = true;
                    for (int i = 1; i < bottles[bottleIndex].balls.Count ; i++)
                    {
                        if (bottles[bottleIndex].balls[i].ballType != ballSample.ballType)
                        {
                            bottleSameType = false;
                            break;
                        }
                    }
                    
                }
            }
            
            if (bottleSameType)
            {
                Debug.Log("Bottle full-------------------------");
                return;
            }
            
            //If choose first bottle
            if (selectedBottleIndex == -1)
            {
                if(bottles[bottleIndex].balls.Count == 0)
                {
                    return;
                }
                
                selectedBottleIndex = bottleIndex;
                currentBottle = bottles[selectedBottleIndex];
                bottles[bottleIndex].Peek()?.MarkHolding().MoveUp();
            }
            //Else choose second bottle
            else
            {
                //If second bottle == first bottle
                if (bottleIndex == selectedBottleIndex)
                {
                    bottles[bottleIndex].Peek()?.MoveDown();
                }
                //If second bottle != first bottle
                else
                {
                    if(bottles[bottleIndex].balls.Count == 0 || (bottles[bottleIndex].Peek()?.ballType == bottles[selectedBottleIndex].Peek()?.ballType))
                    {
                        //If same color but second bottle full
                        if (bottles[bottleIndex].balls.Count == 4)
                        {
                            bottles[selectedBottleIndex].Peek().MoveDown();
                        }
                        //If same color and second bottle not full
                        else
                        {
                            targetBottle = bottles[bottleIndex];
                            SwitchBall(bottles[selectedBottleIndex], bottles[bottleIndex]);
                        }
                        
                    }
                    else if ((bottles[selectedBottleIndex].Peek()?.ballType != bottles[bottleIndex].Peek()?.ballType))
                    {
                        bottles[selectedBottleIndex].Peek().MoveDown();
                    }
                }
                
                //Reset selected bottle index
                selectedBottleIndex = -1;
            }
        }
        
        public bool CheckWinCondition()
        {
            for (int i = 0; i < bottles.Count; i++)
            {
                if (bottles[i].balls.Count != 0 && bottles[i].balls.Count != 4)
                {
                    return false;
                }
                
                foreach (var ball in bottles[i].balls)
                {
                    if(ball.ballType != bottles[i].balls[0].ballType)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        public void ResetScene()
        {
            SceneManager.LoadScene(0);
        }
    }
}