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
        GameObject targetFood;
        Common_WanderScript targetChaser;
        static LayerMask foodLayer;
        private float detectionRangeSquare;
        bool isStaminaRemain = false;

        public override void Awake()
        {
            base.Awake();
            weatherFactor = 0f;//강화학습용 임시
            attackRange = characterController.radius;
            attackRangeSquare = attackRange * attackRange;
            animalType = AnimalType.Herbivore;
            foodLayer = LayerMask.NameToLayer("Food");
            var detectionRange = GetComponentInChildren<CapsuleCollider>().radius;
            detectionRangeSquare = detectionRange * detectionRange;
        }

        public override void OnEnable()
        {
            weatherFactor = Mathf.Clamp(weatherFactor + 0.01f, 0, 1.0f); //강화학습용 임시
            base.OnEnable();
        }

        public override void FixedUpdate()
        {
            if (CurrentState == WanderState.Dead) return;
            if (Toughness <= 0)
            {
                Die();
                return;
            }

            switch (CurrentState)
            {
                case WanderState.Walking:
                    stamina = Mathf.MoveTowards(stamina, MaxStamina, Time.deltaTime);
                    break;
                case WanderState.Running:
                    if (isStaminaRemain)
                    {
                        stamina -= Time.deltaTime;
                        if (stamina <= 0)
                        {
                            SetMoveSlow();
                            isStaminaRemain = false;
                        }
                    }
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
            //collider 는 detector이고 현재 food, Terrain, animal 레이어 감지.

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

            if (other.gameObject.layer == animalLayer) //1. 동물은 공통으로 Animal 레이어.
            {
                Common_WanderScript target = other.GetComponent<Common_WanderScript>();
                switch (CurrentState)
                {
                    case WanderState.Walking: //1-1
                    {
                        if(target.animalType==AnimalType.Calnivore&&target.Dominance>dominance) // 1-1-2.
                        {
                            StopCoroutine(WanderCoroutine());
                            targetChaser = target; //타겟 설정하는 코드 삽입 필요.
                            SetState(WanderState.Running);
                            isStaminaRemain = true;
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
                        if (target.animalType == AnimalType.Calnivore&& target.Dominance > dominance) // 1-3-2.
                        {
                            targetChaser = target; //타겟 설정하는 코드 삽입 필요.
                            SetState(WanderState.Running);
                            StartCoroutine(CheckChaserCoroutine());//타겟에서 벗어났는지 확인하는 코루틴 삽입 필요.
                        }
                        break;
                    }
                    default: break;
                }
            }
            else if(other.gameObject.layer == foodLayer) //2. 먹이는 공통으로 Food 레이어.
            {
                switch (CurrentState)
                {
                    case WanderState.Walking: //2-1
                        {
                            StopCoroutine(WanderCoroutine());
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
            hunger = Mathf.Clamp(hunger + hungerFactor * 2, 0, maxHunger); //현재 약 60.
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
                if (runAwayDirection.sqrMagnitude>detectionRangeSquare*2f)//detectionRangeSquare는 감지 범위의 제곱. 대충 곱하기 2.
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
