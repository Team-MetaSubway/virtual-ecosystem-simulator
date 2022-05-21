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
            mapWidth = learningEnv.mapWidth * 0.95f; //���� �ִ� ���� ����(x������), �ʹ� ������ �����Ǵ� ���� �����ϱ� ���� 0.8 ����.
            mapLength = learningEnv.mapLength * 0.95f; //���� �ִ� ���� ����(z������), �ʹ� ������ �����Ǵ� ���� �����ϱ� ���� 0.8 ����.
            mapMaxHeight = learningEnv.mapMaxHeight; //���� �ִ� ����(y������)
            transformOfParent = transform.parent.transform;
            animalType = AnimalType.Herbivore;
        }

        public override void OnEnable()
        {
            SetState(WanderState.Walking);
            Vector3 pos = new Vector3(Random.value * mapWidth - mapWidth / 2, mapMaxHeight, Random.value * mapLength - mapLength / 2); //���� ��ǥ �����ϰ� ����.
            Ray ray = new Ray(transformOfParent.TransformPoint(pos), Vector3.down); //���� ��ǥ�� �����ؼ� ����.
            RaycastHit hitData;
            Physics.Raycast(ray, out hitData); //���� �������� ���� ��ġ(Y���� maxHeight)���� ������ ���� ���.
            pos.y -= hitData.distance; //���� ���� �Ÿ���ŭ y���� ����. ������ ���� �ٴڿ� �� �°� �����ǰԲ�.
            transform.localPosition = pos;
            transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 359f), 0);
            SetStart();
            base.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            enabled = true; //�ٽ� �����ǰ�.
        }
        // Update is called once per frame
        void Update() //base class�� UpdateAnimalState �Լ��� ���⿡ ����. ������ ����.
        {
            
        }

        public override void OnTriggerEnter(Collider other)
        {
            //�ʽĵ����� collider�� ũ�Ⱑ ũ��. ũ��� �� ���� �����̴�. ray sensor ó��.

            //////////////////////////////////////////////////////////
            //
            //1. ������ encounter �� ���
            //
            //1-1. ���� Wander ������ ���, ����ģ ������ - 
            //1-1-1. ������ �ʽ� �����̸� do nothing
            //1-1-2. ������ ���� �����̸� ���� ������ �ϰ� �ֵ� ȸ�ǻ��·� ��ȯ�Ѵ�. 
            //     ȸ�Ǵ� Running����. ���ĵ����� �ݴ������� ��� �̵��Ѵ�.
            ///
            //1-2. ���� Running����(ȸ�ǻ���)�� ���
            //1-2-1. ������ �ʽ� �����̸� do nothing.
            //1-2-2. ������ ���� �����̸� do nothing. ȸ�� �� ȸ�Ǵ� ���⵵ ������ �ϴ� ���� X.
            //
            //1-3. ���� Walking(���̸� ������ ����)������ ���
            //1-3-1. ������ �ʽ� �����̸� do nothing
            //1-3-2. ������ ���� �����̸� 1-2 ó�� ȸ�� ���·� ��ȯ�Ѵ�.
            //
            ///////////////////////////////////////////////////////////
            //
            //2. ���̸� encounter �� ���
            //
            //2-1. ���� Wander ������ ��� Walking ���·� ��ȯ. ������ ���̹������� ����.
            //2-2. ���� Running ����(ȸ�ǻ���)�� ��� ����.
            //2-3. ���� Walking ����(���̻���)�� ��� ����.
            //
            ///////////////////////////////////////////////////////////
            //
            //3. ��(Env �±�)�� encounter �� ���
            //
            //3-1. ���� Wander ������ ��� �� normal�� ���� �ݻ纤�� ���ؼ� ���� ���� set.
            //3-2. ���� Running ����(ȸ�ǻ���)�� ��� ����.
            //3-3. ���� Walking ����(���̻���)�� ��� ����.
            //
            //////////////////////////////////////////////////////////

            if (CurrentState == WanderState.Dead) return; //���� ���� �׾��ٸ� ����. ���� ������ �ڵ�.

            if (other.gameObject.layer == LayerMask.NameToLayer("Animal")) //1. ������ �������� Animal ���̾�.
            {
                //���߿�, ���� �����̸� �����ϴ� �ڵ� �����ؾ��� ��.
                AnimalType targetType = other.GetComponent<Common_WanderScript>().animalType;
                switch(CurrentState)
                {
                    case WanderState.Wander: //1-1
                    {
                        if(targetType==AnimalType.Calnivore) // 1-1-2.
                        {
                            //Ÿ�� �����ϴ� �ڵ� ���� �ʿ�.
                            SetState(WanderState.Running);
                            //Ÿ�ٿ��� ������� Ȯ���ϴ� �ڷ�ƾ ���� �ʿ�.
                        }
                        break;
                    }

                    case WanderState.Running: //1-2
                    {
                        //����μ� ȸ�� �� ȸ�Ǵ� ���� X.
                        break;
                    }
                    case WanderState.Walking: //1-3.
                    {
                        if (targetType == AnimalType.Calnivore) // 1-3-2.
                        {
                            //Ÿ�� �����ϴ� �ڵ� ���� �ʿ�.
                            SetState(WanderState.Running);
                            //Ÿ�ٿ��� ������� Ȯ���ϴ� �ڷ�ƾ ���� �ʿ�.
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
                    case WanderState.Wander: //2-1
                        {
                            //���� �����ϴ� ���� ����
                            SetState(WanderState.Walking);
                            //���̿��� �Ÿ� �Ǵ��ϴ� �ڷ�ƾ ����
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
            else if(other.CompareTag("Env")) //3. ���� �±״� Env.
            {
                switch (CurrentState)
                {
                    case WanderState.Wander: //2-1
                        {
                            //�ݻ� ���� ���ؼ� ���⺤�͸� set�ϴ� �ڵ� ����.
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

        void EatFood() //���̸� �԰� ��⸦ ä��� �Լ�.
        {

        }
    }
}
