using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
using Polyperfect.Common;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Polyperfect.Animals
{
    public class WanderScript_Harvibore : Common_WanderScript
    {
        LearningEnvController learningEnv;
        float mapWidth;
        float mapLength;
        float mapMaxHeight;
        Transform transformOfParent;
        public override void Awake()
        {
            base.Awake();
            learningEnv = transform.parent.GetComponent<LearningEnvController>();
            mapWidth = learningEnv.mapWidth * 0.95f; //맵의 최대 가로 길이(x축으로), 너무 구석에 스폰되는 것을 방지하기 위해 0.8 곱함.
            mapLength = learningEnv.mapLength * 0.95f; //맵의 최대 세로 길이(z축으로), 너무 구석에 스폰되는 것을 방지하기 위해 0.8 곱함.
            mapMaxHeight = learningEnv.mapMaxHeight; //맵의 최대 높이(y축으로)
            transformOfParent = transform.parent.transform;
            animalType = AnimalType.Herbivore;
        }

        public override void OnEnable()
        {
            SetState(WanderState.Walking);
            Vector3 pos = new Vector3(Random.value * mapWidth - mapWidth / 2, mapMaxHeight, Random.value * mapLength - mapLength / 2); //로컬 좌표 랜덤하게 생성.
            Ray ray = new Ray(transformOfParent.TransformPoint(pos), Vector3.down); //월드 좌표로 변경해서 삽입.
            RaycastHit hitData;
            Physics.Raycast(ray, out hitData); //현재 랜덤으로 정한 위치(Y축은 maxHeight)에서 땅으로 빛을 쏜다.
            pos.y -= hitData.distance; //땅에 맞은 거리만큼 y에서 뺀다. 동물이 지형 바닥에 딱 맞게 스폰되게끔.
            transform.localPosition = pos;
            transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 359f), 0);
            SetStart();
            base.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            enabled = true; //다시 스폰되게.
        }
        // Update is called once per frame
        void Update() //base class의 UpdateAnimalState 함수를 여기에 구현. 적절히 수정.
        {
            
        }

        public override void OnTriggerEnter(Collider other)
        {
            //초식동물은 collider의 크기가 크다. 크기는 곧 감지 범위이다. ray sensor 처럼.

            //////////////////////////////////////////////////////////
            //
            //1. 동물을 encounter 한 경우
            //
            //1-1. 현재 Wander 상태인 경우, 마주친 동물이 - 
            //1-1-1. 동물이 초식 동물이면 do nothing
            //1-1-2. 동물이 육식 동물이면 현재 무엇을 하고 있든 회피상태로 전환한다. 
            //     회피는 Running상태. 육식동물의 반대편으로 계속 이동한다.
            ///
            //1-2. 현재 Running상태(회피상태)인 경우
            //1-2-1. 동물이 초식 동물이면 do nothing.
            //1-2-2. 동물이 육식 동물이면 do nothing. 회피 중 회피는 복잡도 때문에 일단 구현 X.
            //
            //1-3. 현재 Walking(먹이를 먹으러 가는)상태인 경우
            //1-3-1. 동물이 초식 동물이면 do nothing
            //1-3-2. 동물이 육식 동물이면 1-2 처럼 회피 상태로 전환한다.
            //
            ///////////////////////////////////////////////////////////
            //
            //2. 먹이를 encounter 한 경우
            //
            //2-1. 현재 Wander 상태인 경우 Walking 상태로 전환. 방향을 먹이방향으로 고정.
            //2-2. 현재 Running 상태(회피상태)인 경우 무시.
            //2-3. 현재 Walking 상태(먹이상태)인 경우 무시.
            //
            ///////////////////////////////////////////////////////////
            //
            //3. 벽(Env 태그)을 encounter 한 경우
            //
            //3-1. 현재 Wander 상태인 경우 벽 normal에 대한 반사벡터 구해서 방향 벡터 set.
            //3-2. 현재 Running 상태(회피상태)인 경우 무시.
            //3-3. 현재 Walking 상태(먹이상태)인 경우 무시.
            //
            //////////////////////////////////////////////////////////

            if (CurrentState == WanderState.Dead) return; //내가 현재 죽었다면 리턴. 오류 방지용 코드.

            if (other.gameObject.layer == LayerMask.NameToLayer("Animal")) //1. 동물은 공통으로 Animal 레이어.
            {
                //나중에, 죽은 동물이면 무시하는 코드 삽입해야할 듯.
                AnimalType targetType = other.GetComponent<Common_WanderScript>().animalType;
                switch(CurrentState)
                {
                    case WanderState.Wander: //1-1
                    {
                        if(targetType==AnimalType.Calnivore) // 1-1-2.
                        {
                            //타겟 설정하는 코드 삽입 필요.
                            SetState(WanderState.Running);
                            //타겟에서 벗어났는지 확인하는 코루틴 삽입 필요.
                        }
                        break;
                    }

                    case WanderState.Running: //1-2
                    {
                        //현재로선 회피 중 회피는 구현 X.
                        break;
                    }
                    case WanderState.Walking: //1-3.
                    {
                        if (targetType == AnimalType.Calnivore) // 1-3-2.
                        {
                            //타겟 설정하는 코드 삽입 필요.
                            SetState(WanderState.Running);
                            //타겟에서 벗어났는지 확인하는 코루틴 삽입 필요.
                        }
                        break;
                    }
                    default: break;
                }
            }
            else if(other.gameObject.layer == LayerMask.NameToLayer("Food")) //2. 먹이는 공통으로 Food 레이어.
            {
                switch (CurrentState)
                {
                    case WanderState.Wander: //2-1
                        {
                            //먹이 설정하는 벡터 설정
                            SetState(WanderState.Walking);
                            //먹이와의 거리 판단하는 코루틴 삽입
                            break;
                        }

                    case WanderState.Running: //2-2
                        {
                            break;
                        }
                    case WanderState.Walking: //2-3.
                        {
                            break;
                        }
                    default: break;
                }
            }
            else if(other.CompareTag("Env")) //3. 벽의 태그는 Env.
            {
                switch (CurrentState)
                {
                    case WanderState.Wander: //2-1
                        {
                            //반사 벡터 구해서 방향벡터를 set하는 코드 삽입.
                            SetState(WanderState.Walking);
                            break;
                        }

                    case WanderState.Running: //2-2
                        {
                            break;
                        }
                    case WanderState.Walking: //2-3.
                        {
                            break;
                        }
                    default: break;
                }
            }
        }

        void EatFood() //먹이를 먹고 허기를 채우는 함수.
        {

        }
    }
}
