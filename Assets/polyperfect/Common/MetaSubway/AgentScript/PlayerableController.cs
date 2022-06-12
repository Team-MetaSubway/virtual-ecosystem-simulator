using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using DigitalRuby.RainMaker;

public class PlayerableController : MonoBehaviour
{
    // Start is called before the first frame update
    [HideInInspector]
    public bool isPlaying = false;
    public static PlayerableController instance;


    GameObject freeCamera;
    CameraSetting freeCameraScript;
    GameObject cameraParent;
    GameObject previousAnimal;
    ChangeCamera cameraSetting;

    private void Awake()
    {
        isPlaying = false;

        freeCamera = GameObject.Find("Free Camera");
        freeCameraScript = freeCamera.GetComponent<CameraSetting>();
        cameraParent = GameObject.Find("CameraControllerRainFinalFinal");
        cameraSetting = cameraParent.GetComponent<ChangeCamera>();
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            UpdateManually();
    }
    public void UpdateManually()
    {
        if (cameraSetting.isPerson == false)
        {
            if (isPlaying == true) //현재 플레이 중일 때
            {
                freeCamera.transform.parent = cameraParent.transform; //위치 옮기고
                previousAnimal.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.InferenceOnly;  // 동물 inference 로 변경.
                // 프리즈 제거
                isPlaying = false; //플레이 종료.
            }
            else if (isPlaying == false) //현재 플레이 중이 아닐 때
            {
                previousAnimal = freeCameraScript.GetClosestCarnivore();
                if (previousAnimal == null) return;
                freeCamera.transform.parent = previousAnimal.transform; //위치 옮기고
                previousAnimal.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.HeuristicOnly;// 동물 heuristic으로 변경.
                    
                freeCamera.transform.forward = previousAnimal.transform.forward; //바라보는 방향 맞추고
                freeCamera.transform.localPosition = -Vector3.forward*7f + Vector3.up * 7f; //카메라가 약간 뒤에 있게. 배틀그라운드처럼.
                freeCamera.transform.Rotate(30f, 0, 0);// 카메라가 약간 밑을 보게. 배틀그라운드처럼.
                isPlaying = true; //플레이 중으로 변경.
            }
        }
    }
}
