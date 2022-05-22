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
    public class WanderScriptHerbivore : Common_WanderScript
    {
        Vector3 directionToGo;

        public override void Awake()
        {
            base.Awake();
            animalType = AnimalType.Herbivore;
        }
        bool started = false;
        public override void OnEnable()
        {
            base.SetStart();
            characterController.enabled = false;
            Vector3 pos = new Vector3(Random.value * LearningEnvController.instance.mapWidth - LearningEnvController.instance.mapWidth / 2f,
                                      LearningEnvController.instance.mapMaxHeight,
                                      Random.value * LearningEnvController.instance.mapLength - LearningEnvController.instance.mapLength / 2f); //로컬 좌표 랜덤하게 생성.

            Ray ray = new Ray(pos, Vector3.down); //월드 좌표로 변경해서 삽입.
            RaycastHit hitData;
            Physics.Raycast(ray, out hitData); //현재 랜덤으로 정한 위치(Y축은 maxHeight)에서 땅으로 빛을 쏜다.
            pos.y -= hitData.distance; //땅에 맞은 거리만큼 y에서 뺀다. 동물이 지형 바닥에 딱 맞게 스폰되게끔.

            transform.position = pos;
            transform.rotation = Quaternion.Euler(0, Random.Range(0f, 359f), 0);
            characterController.enabled = true;

            base.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            started = false;
        }
        // Update is called once per frame
        void Update() //base class의 UpdateAnimalState 함수를 여기에 구현. 적절히 수정.
        {
            if (CurrentState == WanderState.Dead) return;
            if (Toughness <= 0)
            {
                Die();
                return;
            }
            switch(CurrentState)
            {
                case WanderState.Walking:
                    stamina = Mathf.MoveTowards(stamina, MaxStamina, Time.deltaTime);
                    break;
                case WanderState.Running:
                    stamina -= Time.deltaTime;
                    if (stamina <= 0) SetMoveSlow();
                    break;
                case WanderState.FoundFood:
                    break;

                default: break;
            }
            FaceDirection(directionToGo);
            characterController.SimpleMove(moveSpeed * directionToGo);
        }

        public override void OnTriggerEnter(Collider other)
        {
            //초식동물은 collider의 크기가 크다. 크기는 곧 감지 범위이다. ray sensor 처럼.

            //////////////////////////////////////////////////////////
            //
            //1. 동물을 encounter 한 경우
            //
            //1-1. 현재 Walking 상태인 경우, 마주친 동물이 - 
            //1-1-1. 동물이 초식 동물이면 do nothing
            //1-1-2. 동물이 육식 동물이면 현재 무엇을 하고 있든 회피상태로 전환한다. 
            //     회피는 Running상태. 육식동물의 반대편으로 계속 이동한다.
            ///
            //1-2. 현재 Running상태(회피상태)인 경우
            //1-2-1. 동물이 초식 동물이면 do nothing.
            //1-2-2. 동물이 육식 동물이면 do nothing. 회피 중 회피는 복잡도 때문에 일단 구현 X.
            //
            //1-3. 현재 FoundFood(먹이를 먹으러 가는)상태인 경우
            //1-3-1. 동물이 초식 동물이면 do nothing
            //1-3-2. 동물이 육식 동물이면 1-2 처럼 회피 상태로 전환한다.
            //
            ///////////////////////////////////////////////////////////
            //
            //2. 먹이를 encounter 한 경우
            //
            //2-1. 현재 Walking 상태인 경우 FountFood 상태로 전환. 방향을 먹이방향으로 고정.
            //2-2. 현재 Running 상태(회피상태)인 경우 무시.
            //2-3. 현재 FoundFood 상태(먹이상태)인 경우 무시.
            //
            ///////////////////////////////////////////////////////////
            //
            //3. 벽(Env 태그)을 encounter 한 경우
            //
            //3-1. 현재 Walking 상태인 경우 벽 normal에 대한 반사벡터 구해서 방향 벡터 set.
            //3-2. 현재 Running 상태(회피상태)인 경우 무시.
            //3-3. 현재 FoundFood 상태(먹이상태)인 경우 무시.
            //
            //////////////////////////////////////////////////////////

            if (CurrentState == WanderState.Dead) return; //내가 현재 죽었다면 리턴. 오류 방지용 코드.

            if (other.gameObject.layer == LayerMask.NameToLayer("Animal")) //1. 동물은 공통으로 Animal 레이어.
            {
                //나중에, 죽은 동물이면 무시하는 코드 삽입해야할 듯.
                AnimalType targetType = other.GetComponent<Common_WanderScript>().animalType;
                switch(CurrentState)
                {
                    case WanderState.Walking: //1-1
                    {
                        if(targetType==AnimalType.Calnivore) // 1-1-2.
                        {
                            targetChaser = other.gameObject; //타겟 설정하는 코드 삽입 필요.
                            SetState(WanderState.Running);
                            StartCoroutine(CheckChaserCoroutine());//타겟에서 벗어났는지 확인하는 코루틴 삽입 필요.
                            }
                        break;
                    }

                    case WanderState.Running: //1-2
                    {
                        //현재로선 회피 중 회피는 구현 X.
                        break;
                    }
                    case WanderState.FoundFood: //1-3.
                    {
                        if (targetType == AnimalType.Calnivore) // 1-3-2.
                        {
                            targetChaser = other.gameObject; //타겟 설정하는 코드 삽입 필요.
                            SetState(WanderState.Running);
                            StartCoroutine(CheckChaserCoroutine());//타겟에서 벗어났는지 확인하는 코루틴 삽입 필요.
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
                    case WanderState.Walking: //2-1
                        {
                            targetFood = other.gameObject;//먹이 설정
                            directionToGo = Vector3.ProjectOnPlane((targetFood.transform.position - transform.position),Vector3.up).normalized; //초식동물이 먹이를 바라보는 방향.
                            SetState(WanderState.FoundFood);
                            StartCoroutine(EatFoodCoroutine());//먹이와의 거리 판단하는 코루틴 삽입
                            break;
                        }

                    case WanderState.Running: //2-2
                        {
                            break;
                        }
                    case WanderState.FoundFood: //2-3.
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
                    case WanderState.Walking: //2-1
                        {
                            Ray ray = new Ray(transform.position, directionToGo);
                            RaycastHit hit;
                            Physics.Raycast(ray, out hit);
                            directionToGo = Vector3.Reflect(directionToGo, hit.normal); //반사 벡터 구해서 방향벡터를 set하는 코드 삽입.
                            break;
                        }

                    case WanderState.Running: //2-2
                        {
                            break;
                        }
                    case WanderState.FoundFood: //2-3.
                        {
                            break;
                        }
                    default: break;
                }
            }
        }

        void EatFood() //먹이를 먹고 허기를 채우는 함수.
        {
            Destroy(targetFood);
            hunger = Mathf.Clamp(hunger + hungerFactor * 2, 0, maxHunger);
        }
        IEnumerator EatFoodCoroutine() //2초 마다 먹이를 먹어야하는지 판단하는 코루틴.
        {
            while(true)
            {
                if (CurrentState != WanderState.FoundFood) break;
                if (targetFood == null)
                {
                    SetState(WanderState.Walking);
                    break;
                }
                if((targetFood.transform.position-transform.position).sqrMagnitude < 16.0f)
                {
                    EatFood();
                    SetState(WanderState.Walking);
                    break;
                }
                yield return new WaitForSeconds(2.0f);
            }
        }
        IEnumerator CheckChaserCoroutine() //포식자에게서 벗어났는지 확인하는 코루틴.
        {
            while(true)
            {
                if (CurrentState != WanderState.Running) break;
                if(targetChaser==null)
                {
                    SetState(WanderState.Walking);
                    break;
                }
                Vector3 runAwayDirection = transform.position - targetChaser.transform.position; //포식자에서 초식동물을 바라보는 방향.
                if (runAwayDirection.sqrMagnitude>200f)//attackRangeSquare*4)//attackRangeSquare는 감지 범위의 제곱. 대충 곱하기 2.
                {
                    SetState(WanderState.Walking);
                    break;
                }
                directionToGo = Vector3.ProjectOnPlane(runAwayDirection,Vector3.up).normalized; //방향 assign.
                yield return new WaitForSeconds(2.0f);
            }
        }
        IEnumerator WanderCoroutine() // 랜덤 시간마다 방향을 바꾸는 코루틴.
        {
            while(true)
            {
                if (CurrentState != WanderState.Walking) break;
                directionToGo = Quaternion.Euler(0, Random.Range(0, 359f), 0) * Vector3.forward; //길이 1, 방향 0~359도로 랜덤한 벡터.

                yield return new WaitForSeconds(Random.Range(50f, 100f));
            }
        }
        public override void HandleBeginWalking()
        {
            base.HandleBeginWalking();
            StartCoroutine(WanderCoroutine());
        }
    }
}
