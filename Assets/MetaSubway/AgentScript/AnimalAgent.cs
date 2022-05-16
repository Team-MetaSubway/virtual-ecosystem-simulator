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
    BehaviorParameters behaviorParameters;
    LearningEnvController learningEnv;
    float existential;
    float mapWidth;
    float mapLength;
    float mapMaxHeight;
    Transform transformOfParent;
    float staminaThreshold;

    int killCnt = 0;
    bool canRunning;

    public override void Initialize()
    {
        animalState = GetComponent<Polyperfect.Common.Common_WanderScript>();
        behaviorParameters = GetComponent<BehaviorParameters>();
        learningEnv = transform.parent.GetComponent<LearningEnvController>();
        transformOfParent = transform.parent.transform;
        

        existential = 0.5f / MaxStep;
        mapWidth = learningEnv.mapWidth*0.8f; //���� �ִ� ���� ����(x������), �ʹ� ������ �����Ǵ� ���� �����ϱ� ���� 0.8 ����.
        mapLength = learningEnv.mapLength*0.8f; //���� �ִ� ���� ����(z������), �ʹ� ������ �����Ǵ� ���� �����ϱ� ���� 0.8 ����.
        mapMaxHeight = learningEnv.mapMaxHeight; //���� �ִ� ����(y������)
        staminaThreshold = animalState.StaminaThreshold;
    }
    public override void OnEpisodeBegin()
    {
        animalState.enabled = true;
        animalState.SetState(Polyperfect.Common.Common_WanderScript.WanderState.Walking);

        Vector3 pos = new Vector3(Random.value * mapWidth - mapWidth / 2, mapMaxHeight, Random.value * mapLength - mapLength / 2); //���� ��ǥ �����ϰ� ����.
        Ray ray= new Ray(transformOfParent.TransformPoint(pos), Vector3.down); //���� ��ǥ�� �����ؼ� ����.
        RaycastHit hitData;
        Physics.Raycast(ray, out hitData); //���� �������� ���� ��ġ(Y���� maxHeight)���� ������ ���� ���.
        pos.y -= hitData.distance; //���� ���� �Ÿ���ŭ y���� ����. ������ ���� �ٴڿ� �� �°� �����ǰԲ�.
        Debug.Log("Episode Begin position is: " + pos);
        animalState.transform.localPosition = pos;
        animalState.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 359f), 0);
        canRunning = true;
        animalState.SetStart();
        killCnt = 0;
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
        animalState.UpdateAnimalState( 
            dirToGo, actions.DiscreteActions[2]); //z
        Evaluate();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(animalState.Stamina);
    }
    
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
       
        if(canRunning==true&&animalState.Stamina<=0.0f)
        {
            canRunning = false;
            actionMask.SetActionEnabled(2, 1, false);
        }
        else if(canRunning==false&&animalState.Stamina>staminaThreshold)
        {
            canRunning = true;
            actionMask.SetActionEnabled(2, 1, true);
        }
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
