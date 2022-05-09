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

    public override void Initialize()
    {
        animalState = GetComponent<Polyperfect.Common.Common_WanderScript>();
        behaviorParameters = GetComponent<BehaviorParameters>();
        learningEnv = GetComponentInParent<LearningEnvController>();
        //MaxStep = 5000; //sungwon: 제거 필요. 500 step 이후 강제로 Episode end.
        existential = 0.5f / MaxStep;
    }
    public override void OnEpisodeBegin()
    {
        animalState.enabled = true;
        animalState.SetState(Polyperfect.Common.Common_WanderScript.WanderState.Idle);
        animalState.transform.localPosition = new Vector3(Random.value * 20f - 10f, 4.0f, Random.value * 20f - 10f);
        animalState.realStart();
        //learningEnv.resetTimer();
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
