using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using System.Linq;

public enum Animal
{
    EmptyAnimal = 0,
    Bear,
    Beaver,
    NumOfAnimals
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
    BufferSensorComponent bufferSensor;
    static LayerMask animalLayerMask;
    int idx;
    float detectionRangeNormalizer;

    Dictionary<string, float> animalTagSet = new Dictionary<string, float>();

    public override void Initialize()
    {
        animalState = GetComponent<Polyperfect.Common.Common_WanderScript>();
        bufferSensor = GetComponent<BufferSensorComponent>();
        animalType = (Animal)GetComponent<BehaviorParameters>().TeamId;

        animalLayerMask = LayerMask.GetMask("Animal");
        staminaThreshold = animalState.StaminaThreshold;

        transformOfParent = transform.parent.transform;

        maxStamina = 1f/animalState.MaxStamina;
        maxHunger = 1f/animalState.MaxHunger;
        maxToughness = 1f/animalState.MaxToughness;

        float value = 0;
        foreach (string animal in System.Enum.GetNames(typeof(Animal)))
        {
            animalTagSet.Add(animal, value++/(float)Animal.NumOfAnimals);
        }
    }
    public override void OnEpisodeBegin()
    {
        animalState.enabled = true;

        animalState.characterController.enabled = false;
        transform.localPosition = RandomObjectGenerator.instance.GetRandomPosition();
        transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 359f), 0);
        animalState.characterController.enabled = true;

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

        //buffer process
        Vector3 nowPosition = transform.position;
        var listOfAnimals = Physics.OverlapSphere(nowPosition, animalState.DetectionRange, animalLayerMask);
        var closestAnimals = listOfAnimals.OrderBy(c => (c.transform.position - nowPosition).sqrMagnitude).ToArray();
        //Debug.Log("현재 접촉 동물 수:" + closestAnimals.Length);
        int len = Mathf.Min(bufferSensor.MaxNumObservables + 1, closestAnimals.Length);

        for(idx=1; idx<len; ++idx)
        {
            Vector3 targetPosition = closestAnimals[idx].transform.position;
            float[] animalObservation = new float[] {
                                                        (targetPosition-nowPosition).magnitude/animalState.DetectionRange,
                                                        Mathf.Atan2(targetPosition.z-nowPosition.z,targetPosition.x-nowPosition.x)/Mathf.PI,
                                                        animalTagSet[closestAnimals[idx].tag]
                                                    };
            //Debug.Log(animalObservation);
            bufferSensor.AppendObservation(animalObservation);
        }
    
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
#if ENABLE_RESPAWN
            EndEpisode();
#else
            enabled = false;
#endif
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
#if ENABLE_RESPAWN
        if (killCnt >= 3) EndEpisode();
#endif
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
