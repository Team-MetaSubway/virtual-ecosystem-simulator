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
                                      Random.value * LearningEnvController.instance.mapLength - LearningEnvController.instance.mapLength / 2f); //���� ��ǥ �����ϰ� ����.

            Ray ray = new Ray(pos, Vector3.down); //���� ��ǥ�� �����ؼ� ����.
            RaycastHit hitData;
            Physics.Raycast(ray, out hitData); //���� �������� ���� ��ġ(Y���� maxHeight)���� ������ ���� ���.
            pos.y -= hitData.distance; //���� ���� �Ÿ���ŭ y���� ����. ������ ���� �ٴڿ� �� �°� �����ǰԲ�.

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
        void Update() //base class�� UpdateAnimalState �Լ��� ���⿡ ����. ������ ����.
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
            //�ʽĵ����� collider�� ũ�Ⱑ ũ��. ũ��� �� ���� �����̴�. ray sensor ó��.

            //////////////////////////////////////////////////////////
            //
            //1. ������ encounter �� ���
            //
            //1-1. ���� Walking ������ ���, ����ģ ������ - 
            //1-1-1. ������ �ʽ� �����̸� do nothing
            //1-1-2. ������ ���� �����̸� ���� ������ �ϰ� �ֵ� ȸ�ǻ��·� ��ȯ�Ѵ�. 
            //     ȸ�Ǵ� Running����. ���ĵ����� �ݴ������� ��� �̵��Ѵ�.
            ///
            //1-2. ���� Running����(ȸ�ǻ���)�� ���
            //1-2-1. ������ �ʽ� �����̸� do nothing.
            //1-2-2. ������ ���� �����̸� do nothing. ȸ�� �� ȸ�Ǵ� ���⵵ ������ �ϴ� ���� X.
            //
            //1-3. ���� FoundFood(���̸� ������ ����)������ ���
            //1-3-1. ������ �ʽ� �����̸� do nothing
            //1-3-2. ������ ���� �����̸� 1-2 ó�� ȸ�� ���·� ��ȯ�Ѵ�.
            //
            ///////////////////////////////////////////////////////////
            //
            //2. ���̸� encounter �� ���
            //
            //2-1. ���� Walking ������ ��� FountFood ���·� ��ȯ. ������ ���̹������� ����.
            //2-2. ���� Running ����(ȸ�ǻ���)�� ��� ����.
            //2-3. ���� FoundFood ����(���̻���)�� ��� ����.
            //
            ///////////////////////////////////////////////////////////
            //
            //3. ��(Env �±�)�� encounter �� ���
            //
            //3-1. ���� Walking ������ ��� �� normal�� ���� �ݻ纤�� ���ؼ� ���� ���� set.
            //3-2. ���� Running ����(ȸ�ǻ���)�� ��� ����.
            //3-3. ���� FoundFood ����(���̻���)�� ��� ����.
            //
            //////////////////////////////////////////////////////////

            if (CurrentState == WanderState.Dead) return; //���� ���� �׾��ٸ� ����. ���� ������ �ڵ�.

            if (other.gameObject.layer == LayerMask.NameToLayer("Animal")) //1. ������ �������� Animal ���̾�.
            {
                //���߿�, ���� �����̸� �����ϴ� �ڵ� �����ؾ��� ��.
                AnimalType targetType = other.GetComponent<Common_WanderScript>().animalType;
                switch(CurrentState)
                {
                    case WanderState.Walking: //1-1
                    {
                        if(targetType==AnimalType.Calnivore) // 1-1-2.
                        {
                            targetChaser = other.gameObject; //Ÿ�� �����ϴ� �ڵ� ���� �ʿ�.
                            SetState(WanderState.Running);
                            StartCoroutine(CheckChaserCoroutine());//Ÿ�ٿ��� ������� Ȯ���ϴ� �ڷ�ƾ ���� �ʿ�.
                            }
                        break;
                    }

                    case WanderState.Running: //1-2
                    {
                        //����μ� ȸ�� �� ȸ�Ǵ� ���� X.
                        break;
                    }
                    case WanderState.FoundFood: //1-3.
                    {
                        if (targetType == AnimalType.Calnivore) // 1-3-2.
                        {
                            targetChaser = other.gameObject; //Ÿ�� �����ϴ� �ڵ� ���� �ʿ�.
                            SetState(WanderState.Running);
                            StartCoroutine(CheckChaserCoroutine());//Ÿ�ٿ��� ������� Ȯ���ϴ� �ڷ�ƾ ���� �ʿ�.
                        }
                        break;
                    }
                    default: break;
                }
            }
            else if(other.gameObject.layer == LayerMask.NameToLayer("Food")) //2. ���̴� �������� Food ���̾�.
            {
                switch (CurrentState)
                {
                    case WanderState.Walking: //2-1
                        {
                            targetFood = other.gameObject;//���� ����
                            directionToGo = Vector3.ProjectOnPlane((targetFood.transform.position - transform.position),Vector3.up).normalized; //�ʽĵ����� ���̸� �ٶ󺸴� ����.
                            SetState(WanderState.FoundFood);
                            StartCoroutine(EatFoodCoroutine());//���̿��� �Ÿ� �Ǵ��ϴ� �ڷ�ƾ ����
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
            else if(other.CompareTag("Env")) //3. ���� �±״� Env.
            {
                switch (CurrentState)
                {
                    case WanderState.Walking: //2-1
                        {
                            Ray ray = new Ray(transform.position, directionToGo);
                            RaycastHit hit;
                            Physics.Raycast(ray, out hit);
                            directionToGo = Vector3.Reflect(directionToGo, hit.normal); //�ݻ� ���� ���ؼ� ���⺤�͸� set�ϴ� �ڵ� ����.
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

        void EatFood() //���̸� �԰� ��⸦ ä��� �Լ�.
        {
            Destroy(targetFood);
            hunger = Mathf.Clamp(hunger + hungerFactor * 2, 0, maxHunger);
        }
        IEnumerator EatFoodCoroutine() //2�� ���� ���̸� �Ծ���ϴ��� �Ǵ��ϴ� �ڷ�ƾ.
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
        IEnumerator CheckChaserCoroutine() //�����ڿ��Լ� ������� Ȯ���ϴ� �ڷ�ƾ.
        {
            while(true)
            {
                if (CurrentState != WanderState.Running) break;
                if(targetChaser==null)
                {
                    SetState(WanderState.Walking);
                    break;
                }
                Vector3 runAwayDirection = transform.position - targetChaser.transform.position; //�����ڿ��� �ʽĵ����� �ٶ󺸴� ����.
                if (runAwayDirection.sqrMagnitude>200f)//attackRangeSquare*4)//attackRangeSquare�� ���� ������ ����. ���� ���ϱ� 2.
                {
                    SetState(WanderState.Walking);
                    break;
                }
                directionToGo = Vector3.ProjectOnPlane(runAwayDirection,Vector3.up).normalized; //���� assign.
                yield return new WaitForSeconds(2.0f);
            }
        }
        IEnumerator WanderCoroutine() // ���� �ð����� ������ �ٲٴ� �ڷ�ƾ.
        {
            while(true)
            {
                if (CurrentState != WanderState.Walking) break;
                directionToGo = Quaternion.Euler(0, Random.Range(0, 359f), 0) * Vector3.forward; //���� 1, ���� 0~359���� ������ ����.

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
