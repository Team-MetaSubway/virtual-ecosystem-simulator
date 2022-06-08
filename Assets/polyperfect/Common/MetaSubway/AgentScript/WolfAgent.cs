using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using System.Linq;

public class WolfAgent : Agent
{
    [HideInInspector]
    public Polyperfect.Common.Common_WanderScript animalState;
    [HideInInspector]
    public RandomObjectGenerator.WolfGroup myGroup;
    private bool hasGroup = false;
    WolfAgent friendWolf1;
    WolfAgent friendWolf2;


    float maxToughness;
    float maxHunger;
    float maxStamina;

    float toughnessThreshold;
    float hungerThreshold;
    float staminaThreshold;

    float toughnessFactor;
    float hungerFactor;

    float previousHunger;
    float previousToughness;
    float wallCollideFactor;

    bool canRunning;

    BufferSensorComponent bufferSensor;
    static LayerMask animalLayerMask;

    int maxBufferSize;

    public override void Initialize()
    {
        animalState = GetComponent<Polyperfect.Common.Common_WanderScript>();
        bufferSensor = GetComponent<BufferSensorComponent>();

        animalLayerMask = LayerMask.GetMask("Animal");

        maxToughness = 1f / animalState.MaxToughness;
        maxHunger = 1f / animalState.MaxHunger;
        maxStamina = 1f / animalState.MaxStamina;

        toughnessThreshold = 0.68f * animalState.MaxToughness; //원래는 0.95. 성공 조건이 과하다고 판단, 0.68 으로 재조정.
        hungerThreshold = 0.5f * animalState.MaxHunger; //원래는 0.95. 성공 조건이 과하다고 판단, 0.5 으로 재조정.
        staminaThreshold = animalState.StaminaThreshold;

        maxBufferSize = bufferSensor.MaxNumObservables + 1;
        wallCollideFactor = -0.1f;

        toughnessFactor = 1.5f;
        hungerFactor = 0.5f;

        hasGroup = false;

        //Debug.Log("탐지 범위:" + animalState.DetectionRange);
    }
    public override void OnEpisodeBegin()
    {
        animalState.enabled = true;

        if (hasGroup == true)
        {
            animalState.characterController.enabled = false;
            transform.position = myGroup.groupRandomPosition();
            animalState.characterController.enabled = true;
        }
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
                //animalState.Direction = dirToGo;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;//위에서 봤을때 시계 회전
                //dirToGo = Quaternion.Euler(0, -animalState.TurnSpeed * Time.deltaTime, 0) * dirToGo;
                transform.Rotate(rotateDir, animalState.TurnSpeed * Time.deltaTime);
                break;
            case 2:
                rotateDir = transform.up * 1f; //위에서 봤을때 반시계 회전
                //dirToGo = Quaternion.Euler(0, animalState.TurnSpeed * Time.deltaTime, 0) * dirToGo;
                transform.Rotate(rotateDir, animalState.TurnSpeed * Time.deltaTime);
                break;
        }

        animalState.Direction = dirToGo;
        animalState.UpdateAnimalState(actions.DiscreteActions[2]);
        Evaluate();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(animalState.Toughness * maxToughness);
        sensor.AddObservation(animalState.Hunger * maxHunger);
        sensor.AddObservation(animalState.Stamina * maxStamina);

        //buffer process
        Vector3 nowPosition = transform.position;
        var listOfAnimals = Physics.OverlapSphere(nowPosition, animalState.DetectionRange, animalLayerMask);
        var closestAnimals = listOfAnimals.OrderBy(c => (c.transform.position - nowPosition).sqrMagnitude).ToArray();
        int len = Mathf.Min(maxBufferSize, closestAnimals.Length);
        //Debug.Log("현재 접촉 동물 수:" + closestAnimals.Length);
        for (int idx = 1; idx < len; ++idx)
        {
            Vector3 targetPosition = closestAnimals[idx].transform.position;
            Vector3 localSpaceDirection = transform.InverseTransformDirection(targetPosition - nowPosition);
            float[] animalObservation = new float[] {
                                                        (targetPosition-nowPosition).magnitude/animalState.DetectionRange,
                                                        Mathf.Atan2(localSpaceDirection.x,localSpaceDirection.z)/Mathf.PI,
                                                        RandomObjectGenerator.instance.animalTagSet[closestAnimals[idx].tag]
                                                    };
            //Debug.Log("거리" + animalObservation[0]*animalState.DetectionRange + "각도" + animalObservation[1]*Mathf.PI + "태그" + closestAnimals[idx].tag + "태그 숫자" + animalObservation[2]);
            bufferSensor.AppendObservation(animalObservation);
        }

    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        actionMask.SetActionEnabled(2, 1, canRunning);
    }

    private void Evaluate()
    {
        float reward = (animalState.Toughness - previousToughness) * maxToughness * toughnessFactor + (animalState.Hunger - previousHunger) * maxHunger * hungerFactor;
        AddReward(reward);
        //myGroup.wolfGroup.AddGroupReward(reward);
        previousToughness = animalState.Toughness;
        previousHunger = animalState.Hunger;
        if (animalState.IsCollidedWithWall)
        {
            AddReward(wallCollideFactor);
            animalState.IsCollidedWithWall = false;
        }

        if (animalState.CurrentState == Polyperfect.Common.Common_WanderScript.WanderState.Dead)
        {
#if ENABLE_RESPAWN
            AddReward(-1f);
            myGroup.wolfGroup.AddGroupReward(-1f);
            myGroup.EndGroup();
            //EndEpisode();
#else
            animalState.enabled = false;
            enabled = false;
#endif
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
        if (animalState.Toughness >= toughnessThreshold && animalState.Hunger >= hungerThreshold)
        {
            //성공시 조건을 조금 더 까다롭게?
            toughnessThreshold = Mathf.Clamp(toughnessThreshold + 0.01f * animalState.MaxToughness, 0, 0.90f * animalState.MaxToughness); //1% 조건 어렵게. 95%까지 증가.
            hungerThreshold = Mathf.Clamp(hungerThreshold + 0.01f * animalState.MaxHunger, 0, 0.85f * animalState.MaxHunger); //1% 조건 어렵게. 85%까지 증가.
            Debug.Log("통과. 나는 " + gameObject.tag + " 체력은 " + toughnessThreshold + " 배고픔은 " + hungerThreshold);
            myGroup.wolfGroup.AddGroupReward(0.1f);
            myGroup.EndGroup();
        }
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

    public void SetGroup(RandomObjectGenerator.WolfGroup group, int idx)
    {
        myGroup = group;
        hasGroup = true;

        transform.GetComponent<BehaviorParameters>().TeamId = group.teamId;

        transform.position = group.groupRandomPosition();
        if(idx==0)
        {
            friendWolf1 = group.agents[1];
            friendWolf2 = group.agents[2];
        }
        else if(idx==1)
        {
            friendWolf1 = group.agents[0];
            friendWolf2 = group.agents[2];
        }
        else if(idx==2)
        {
            friendWolf1 = group.agents[0];
            friendWolf2 = group.agents[1];
        }
    }

    public void EatTogether()
    {
        myGroup.wolfGroup.AddGroupReward(0.05f);
        //if ((friendWolf1.transform.position - transform.position).sqrMagnitude < 1000f)
        {
            myGroup.wolfGroup.AddGroupReward(0.05f);
            friendWolf1.animalState.Hunger += 0.12f / maxHunger;
        }
        //if ((friendWolf2.transform.position - transform.position).sqrMagnitude < 1000f)
        {
            myGroup.wolfGroup.AddGroupReward(0.05f);
            friendWolf2.animalState.Hunger += 0.12f / maxHunger;
        }
    }
}
