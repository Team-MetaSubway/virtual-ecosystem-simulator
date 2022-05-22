using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public enum Animal
{
    Bear = 0,
    Beaver,
    NumOfAnimal
}


public class AnimalAgent : Agent
{
    Polyperfect.Common.Common_WanderScript animalState;
    float staminaThreshold;
    Animal animalType;
    public static Transform transformOfParent;
    float maxStamina;
    float maxToughness;
    float maxHunger;
    int killCnt = 0;
    bool canRunning;
    float wallCollideFactor;
    float previousReward;
    float previousHunger;
    float previousToughness;

    public override void Initialize()
    {
        animalState = GetComponent<Polyperfect.Common.Common_WanderScript>();

        animalType = (Animal)GetComponent<BehaviorParameters>().TeamId;
        staminaThreshold = animalState.StaminaThreshold;

        transformOfParent = transform.parent.transform;

        maxStamina = 1f/animalState.MaxStamina;
        maxHunger = 1f/animalState.MaxHunger;
        maxToughness = 1f/animalState.MaxToughness;
    }
    public override void OnEpisodeBegin()
    {
        animalState.enabled = true;

        Vector3 pos = new Vector3(Random.value * LearningEnvController.instance.mapWidth - LearningEnvController.instance.mapWidth / 2,
                                      LearningEnvController.instance.mapMaxHeight,
                                      Random.value * LearningEnvController.instance.mapLength - LearningEnvController.instance.mapLength / 2); //로컬 좌표 랜덤하게 생성.

        Ray ray = new Ray(transformOfParent.TransformPoint(pos), Vector3.down); //월드 좌표로 변경해서 삽입.
        RaycastHit hitData;
        Physics.Raycast(ray, out hitData); //현재 랜덤으로 정한 위치(Y축은 maxHeight)에서 땅으로 빛을 쏜다.
        pos.y -= hitData.distance; //땅에 맞은 거리만큼 y에서 뺀다. 동물이 지형 바닥에 딱 맞게 스폰되게끔.
        animalState.characterController.enabled = false;
        transform.localPosition = pos;
        transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 359f), 0);
        animalState.characterController.enabled = true;

        animalState.SetStart();
        killCnt = 0;
        wallCollideFactor = -0.001f;
        previousReward = 0f;
        previousToughness = animalState.Toughness;
        previousHunger = animalState.Hunger;
        canRunning = true;
    }
    public override void OnActionReceived(ActionBuffers actions)
    {

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var forwardAxis = actions.DiscreteActions[0];
        var rotateAxis = actions.DiscreteActions[1];

        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward; //뒤 무빙
                break;
        }
        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;//위에서 봤을때 시계 회전
                break;
            case 2:
                rotateDir = transform.up * 1f; //위에서 봤을때 반시계 회전
                break;
        }
        transform.Rotate(rotateDir, animalState.TurnSpeed*Time.deltaTime);
        animalState.UpdateAnimalState( 
            dirToGo, actions.DiscreteActions[2]); //z
        Evaluate();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(animalState.Toughness);
        sensor.AddObservation(animalState.Hunger);
        sensor.AddObservation(animalState.Stamina);
    }
    
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        actionMask.SetActionEnabled(2, 1, canRunning);
    }
    /*
    usage: 
    actionMask.SetActionEnabled(0, 1, false);
    actionMask.SetActionEnabled(0, 2, false);
    */

    private void Evaluate()
    {
        if(animalState.CurrentState==Polyperfect.Common.Common_WanderScript.WanderState.Dead)
        {
            SetReward(-1f);
            animalState.enabled = false;
            EndEpisode();
        }
        else if(animalState.HasKilled)
        {
            ++killCnt;
            animalState.HasKilled = false;
        }
        SetReward((animalState.Toughness-previousToughness)*maxToughness+(animalState.Hunger-previousHunger)*maxHunger);
        previousToughness = animalState.Toughness;
        previousHunger = animalState.Hunger;
        if (animalState.IsCollidedWithWall)
        {
            AddReward(wallCollideFactor);
            animalState.IsCollidedWithWall = false;
        }

        if (canRunning == true && animalState.Stamina <= 0.0f)
        {
            canRunning = false;
        }
        else if (canRunning == false && animalState.Stamina > staminaThreshold)
        {
            canRunning = true;
        }

        if (killCnt >= 3) EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1; //+z, transform.forward
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[1] = 2; //clockwise, -y
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[1] = 1; //ccw, +y
        }
        if (Input.GetKey(KeyCode.LeftShift)) discreteActionsOut[2] = (int)Polyperfect.Common.Common_WanderScript.WanderState.Running;
        else discreteActionsOut[2] = (int)Polyperfect.Common.Common_WanderScript.WanderState.Walking;
    }
}
