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
    float maxStamina;
    float maxToughness;
    float maxHunger;
    float toughnessThreshold;
    float hungerThreshold;
    int killCnt;
    bool canRunning;
    float wallCollideFactor;
    float previousReward;
    float previousHunger;
    float previousToughness;
    BufferSensorComponent bufferSensor;
    static LayerMask animalLayerMask;
    int idx;
    int maxBufferSize;

    Dictionary<string, float> animalTagSet = new Dictionary<string, float>();

    public override void Initialize()
    {
        animalState = GetComponent<Polyperfect.Common.Common_WanderScript>();
        bufferSensor = GetComponent<BufferSensorComponent>();
        animalType = (Animal)GetComponent<BehaviorParameters>().TeamId;

        animalLayerMask = LayerMask.GetMask("Animal");
        staminaThreshold = animalState.StaminaThreshold;


        maxStamina = 1f/animalState.MaxStamina;
        maxHunger = 1f/animalState.MaxHunger;
        maxToughness = 1f/animalState.MaxToughness;

        hungerThreshold = 0.95f * animalState.MaxHunger;
        toughnessThreshold = 0.95f * animalState.MaxToughness;

        maxBufferSize = bufferSensor.MaxNumObservables + 1;

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
        wallCollideFactor = -0.1f;
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
        sensor.AddObservation(animalState.Toughness*maxToughness);
        sensor.AddObservation(animalState.Hunger*maxHunger);
        sensor.AddObservation(animalState.Stamina*maxStamina);

        //buffer process
        Vector3 nowPosition = transform.position;
        var listOfAnimals = Physics.OverlapSphere(nowPosition, animalState.DetectionRange, animalLayerMask);
        var closestAnimals = listOfAnimals.OrderBy(c => (c.transform.position - nowPosition).sqrMagnitude).ToArray();
        int len = Mathf.Min(maxBufferSize, closestAnimals.Length);
        //Debug.Log("현재 접촉 동물 수:" + closestAnimals.Length);
        for (idx=1; idx<len; ++idx)
        {
            Vector3 targetPosition = closestAnimals[idx].transform.position;
            Vector3 localSpaceDirection = transform.InverseTransformDirection(targetPosition - nowPosition);
            float[] animalObservation = new float[] {
                                                        (targetPosition-nowPosition).magnitude/animalState.DetectionRange,
                                                        Mathf.Atan2(localSpaceDirection.x,localSpaceDirection.z)/Mathf.PI,
                                                        animalTagSet[closestAnimals[idx].tag]
                                                    };
            //Debug.Log("거리" + animalObservation[0]*animalState.DetectionRange + "각도" + animalObservation[1]*Mathf.PI + "태그" + animalObservation[2]);
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
        if (animalState.Toughness >= toughnessThreshold && animalState.Hunger >= hungerThreshold) EndEpisode();
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
