using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DayNightSystem : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float time;
    public float fullDayLength;
    public float startTime = 0.4f;
    private float timeRate;
    public Vector3 noon;
    
    public GameObject StarDome;
    Material starMat;
    
    [Header("Sun")]
    public Light sun;
    public Gradient sunColor;
    public AnimationCurve sunIntensity;

    [Header("Moon")]
    public Light moon;
    public Gradient moonColor;
    public AnimationCurve moonIntensity;
    
    [Header("Other setting")]
    public AnimationCurve lightingIntensityMultiplier;
    public AnimationCurve reflectionsIntensityMultiplier;
    
    
    public bool showUI;
    public int Day = 0;
	int DayLimit = 0;
    private string AMPM;

    public static DayNightSystem instance = null;
	RecordInformation RecordInformation;

	bool SceneFlag = true;

	private void Awake()
    {
		instance = this;
		DownloadDay();
	}

    // Start is called before the first frame update
    void Start()
    {
        timeRate = 1.0f / fullDayLength;
        time = startTime;

        //starDome 초기값
        starMat = StarDome.GetComponentInChildren<MeshRenderer>().material;
        starMat.color = new Color(1f, 1f, 1f, 0f);
		if (SceneManager.GetActiveScene().name == "MainScene")
		{
			RecordInformation = GameObject.Find("RecObject").GetComponent<RecordInformation>();
			SceneFlag = false;
		}
	}

	// Update is called once per frame
	void Update()
    {
        //스타돔이 천천히 회전하게 
        StarDome.transform.Rotate(new Vector3(0, 2f * Time.deltaTime, 0));

        //시간 증가
        time += timeRate * Time.deltaTime;

        //날짜 증가,시간 초기화
        if (time >= 1.0f)
        {
			if(!SceneFlag)
				RecordInformation.SaveAnimalCount();
			Day++;
            time = 0.0f;
        }

		//제한된 날짜가 되면
		if (DayLimit != 0 && Day >= DayLimit)
		{
			RandomObjectGenerator.instance.DisableAll();
			SceneManager.LoadScene("ScoreBoard");
		}

        //해와 달 움직이기
        sun.transform.eulerAngles = (time - 0.25f) * noon * 4.0f;
        moon.transform.eulerAngles = (time - 0.75f) * noon * 4.0f;


        //빛 밀도 
        sun.intensity = sunIntensity.Evaluate(time);
        moon.intensity = moonIntensity.Evaluate(time);

        //시간에 따른 색깔변경
        sun.color = sunColor.Evaluate(time);
        moon.color = moonColor.Evaluate(time);

        //해와 달 사라지게
        
        if (sun.intensity == 0 && sun.gameObject.activeInHierarchy)
        {
            sun.gameObject.SetActive(false);
        }
        else if (sun.intensity > 0 && !sun.gameObject.activeInHierarchy)
        {

            sun.gameObject.SetActive(true);
        }
        
        if (moon.intensity == 0 && moon.gameObject.activeInHierarchy)
        {   
            moon.gameObject.SetActive(false);
        }
        else if (moon.intensity > 0 && !moon.gameObject.activeInHierarchy)
        {           
            moon.gameObject.SetActive(true);
        }
        
        
  
        //낮과 밤 구분 
        if (time > 0.25f && time < 0.75f)
        {
            //낮 (스타돔 안보이게)
            //sun.gameObject.SetActive(true);
            //moon.gameObject.SetActive(false);

            starMat.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, Time.deltaTime));
            AMPM = "Day";
       
        }
        else
        {
            //밤 (스타돔 보이게)
            starMat.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, Time.deltaTime));
            AMPM = "Night";

           // moon.gameObject.SetActive(true);
           // sun.gameObject.SetActive(false);
        }
    


        //빛 반사 밀도 
        RenderSettings.ambientIntensity = lightingIntensityMultiplier.Evaluate(time);
        RenderSettings.reflectionIntensity = reflectionsIntensityMultiplier.Evaluate(time);
        
    }
    
    //UI 
    void OnGUI()
    {
        if (showUI)
        {
            GUILayout.Box("Today: " + Day );
            GUILayout.Box(AMPM);
        }
    }

	void DownloadDay()
	{
		DayLimit = PlayerPrefs.GetInt("DayLimit");
		PlayerPrefs.DeleteKey("DayLimit");
	}
}
