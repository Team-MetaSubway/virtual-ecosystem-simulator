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
        mapWidth = learningEnv.mapWidth*0.8f; //���� �ִ� ���� ����(x������), �ʹ� ������ �����Ǵ� ���� �����ϱ� ���� 0.8 ����.
        mapLength = learningEnv.mapLength*0.8f; //���� �ִ� ���� ����(z������), �ʹ� ������ �����Ǵ� ���� �����ϱ� ���� 0.8 ����.
        mapMaxHeight = learningEnv.mapMaxHeight; //���� �ִ� ����(y������)
    }
    public override void OnEpisodeBegin()
    {
        animalState.enabled = true;
        animalState.SetState(Polyperfect.Common.Common_WanderScript.WanderState.Idle);

        Vector3 pos = new Vector3(Random.value * mapWidth - mapWidth / 2, mapMaxHeight, Random.value * mapLength - mapLength / 2); //���� ��ǥ �����ϰ� ����.
        Ray ray= new Ray(transform.TransformPoint(pos), Vector3.down); //���� ��ǥ�� �����ؼ� ����.
        RaycastHit hitData;
        Physics.Raycast(ray, out hitData); //���� �������� ���� ��ġ(Y���� maxHeight)���� ������ ���� ���.
        pos.y -= hitData.distance; //���� ���� �Ÿ���ŭ y���� ����. ������ ���� �ٴڿ� �� �°� �����ǰԲ�.

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
            // ������ �� �߰��۾�
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
