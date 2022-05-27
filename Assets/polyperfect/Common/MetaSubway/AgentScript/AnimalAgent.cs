using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using System.Linq;

public class AnimalAgent : Agent
{
    Polyperfect.Common.Common_WanderScript animalState;
    
    float maxToughness;
    float maxHunger;
    float maxStamina;

    float toughnessThreshold;
    float hungerThreshold;
    float staminaThreshold;
    
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

        maxToughness = 1f/animalState.MaxToughness;
        maxHunger = 1f/animalState.MaxHunger;
        maxStamina = 1f/animalState.MaxStamina;

        toughnessThreshold = 0.68f * animalState.MaxToughness; //������ 0.95. ���� ������ ���ϴٰ� �Ǵ�, 0.68 ���� ������.
        hungerThreshold = 0.68f * animalState.MaxHunger; //������ 0.95. ���� ������ ���ϴٰ� �Ǵ�, 0.68 ���� ������.
        staminaThreshold = animalState.StaminaThreshold;

        maxBufferSize = bufferSensor.MaxNumObservables + 1;
        wallCollideFactor = -0.1f;
    }
    public override void OnEpisodeBegin()
    {
        animalState.enabled = true;

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
                dirToGo = transform.forward; //�� ����
                break;
        }
        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;//������ ������ �ð� ȸ��
                break;
            case 2:
                rotateDir = transform.up * 1f; //������ ������ �ݽð� ȸ��
                break;
        }
        transform.Rotate(rotateDir, animalState.TurnSpeed*Time.deltaTime);
        animalState.UpdateAnimalState(dirToGo, actions.DiscreteActions[2]);
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
        //Debug.Log("���� ���� ���� ��:" + closestAnimals.Length);
        for (int idx=1; idx<len; ++idx)
        {
            Vector3 targetPosition = closestAnimals[idx].transform.position;
            Vector3 localSpaceDirection = transform.InverseTransformDirection(targetPosition - nowPosition);
            float[] animalObservation = new float[] {
                                                        (targetPosition-nowPosition).magnitude/animalState.DetectionRange,
                                                        Mathf.Atan2(localSpaceDirection.x,localSpaceDirection.z)/Mathf.PI,
                                                        RandomObjectGenerator.instance.animalTagSet[closestAnimals[idx].tag]
                                                    };
            //Debug.Log("�Ÿ�" + animalObservation[0]*animalState.DetectionRange + "����" + animalObservation[1]*Mathf.PI + "�±�" + animalObservation[2]);
            bufferSensor.AppendObservation(animalObservation);
        }
        
    }
    
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        actionMask.SetActionEnabled(2, 1, canRunning);
    }

    private void Evaluate()
    {
        if(animalState.CurrentState==Polyperfect.Common.Common_WanderScript.WanderState.Dead)
        {
            AddReward(-1f);
            animalState.enabled = false;
#if ENABLE_RESPAWN
            EndEpisode();
#else
            enabled = false;
#endif
        }
        
        AddReward((animalState.Toughness-previousToughness)*maxToughness+(animalState.Hunger-previousHunger)*maxHunger);
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
        if (animalState.Toughness >= toughnessThreshold && animalState.Hunger >= hungerThreshold)
        {
            //������ ������ ���� �� ��ٷӰ�?
            toughnessThreshold = Mathf.Clamp(toughnessThreshold + 0.01f * animalState.MaxToughness, 0, 0.95f * animalState.MaxToughness); //1% ���� ��ư�. 95%���� ����.
            hungerThreshold = Mathf.Clamp(hungerThreshold + 0.01f * animalState.MaxHunger, 0, 0.95f * animalState.MaxHunger); //''
            Debug.Log("�Ϻ�.");
            animalState.enabled = false;
            EndEpisode();
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
}
