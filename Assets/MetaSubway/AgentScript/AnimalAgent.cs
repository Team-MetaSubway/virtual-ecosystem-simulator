using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;


public enum Animal
{
    Bear = 0,
    Beaver,
    NumOfAnimal
}


public class AnimalAgent : Agent
{
    Polyperfect.Common.Common_WanderScript animalState;
    BehaviorParameters behaviorParameters;
    LearningEnvController learningEnv;
    int killCnt = 0;
    float existential;
    float mapWidth;
    float mapLength;
    float mapMaxHeight;

    public override void Initialize()
    {
        animalState = GetComponent<Polyperfect.Common.Common_WanderScript>();
        behaviorParameters = GetComponent<BehaviorParameters>();
        learningEnv = GetComponentInParent<LearningEnvController>();

        existential = 0.5f / MaxStep;
        mapWidth = learningEnv.mapWidth*0.8f; //맵의 최대 가로 길이(x축으로), 너무 구석에 스폰되는 것을 방지하기 위해 0.8 곱함.
        mapLength = learningEnv.mapLength*0.8f; //맵의 최대 세로 길이(z축으로), 너무 구석에 스폰되는 것을 방지하기 위해 0.8 곱함.
        mapMaxHeight = learningEnv.mapMaxHeight; //맵의 최대 높이(y축으로)
    }
    public override void OnEpisodeBegin()
    {
        animalState.enabled = true;
        animalState.SetState(Polyperfect.Common.Common_WanderScript.WanderState.Idle);

        Vector3 pos = new Vector3(Random.value * mapWidth - mapWidth / 2, mapMaxHeight, Random.value * mapLength - mapLength / 2); //로컬 좌표 랜덤하게 생성.
        Ray ray= new Ray(transform.TransformPoint(pos), Vector3.down); //월드 좌표로 변경해서 삽입.
        RaycastHit hitData;
        Physics.Raycast(ray, out hitData); //현재 랜덤으로 정한 위치(Y축은 maxHeight)에서 땅으로 빛을 쏜다.
        pos.y -= hitData.distance; //땅에 맞은 거리만큼 y에서 뺀다. 동물이 지형 바닥에 딱 맞게 스폰되게끔.

        animalState.transform.localPosition = pos;
        animalState.realStart();
        killCnt = 0;
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        animalState.updateAnimalState( 
            new Vector3(
                actions.ContinuousActions[0], //x
                actions.ContinuousActions[1], //y
                actions.ContinuousActions[2])); //z
        evaluate();
    }

    private void evaluate()
    {
        //if (learningEnv.IsTimeout)
        {
            // 끝났을 때 추가작업
            //EpisodeInterrupted();
        }
        //else
        {
            if(animalState.CurrentState==Polyperfect.Common.Common_WanderScript.WanderState.Dead)
            {
                SetReward(0.0f);
                EndEpisode();
            }
            else if(animalState.HasKilled)
            {
                AddReward(1.0f/3);
                ++killCnt;
                animalState.HasKilled = false;
            }

            if((Animal)behaviorParameters.TeamId==Animal.Bear)
            {
                if (killCnt >= 3) EndEpisode();
                AddReward(-existential);
            }
            else if((Animal)behaviorParameters.TeamId == Animal.Beaver)
            {
                AddReward(existential);
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = 0.0f;
        continuousActionsOut[1] = 0.0f;
        continuousActionsOut[2] = 0.0f;

        if (Input.GetKey(KeyCode.W))
        {
            continuousActionsOut[2] = 1.0f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            continuousActionsOut[2] = -1.0f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            continuousActionsOut[0] = -1.0f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            continuousActionsOut[0] = 1.0f;
        }
    }
}
