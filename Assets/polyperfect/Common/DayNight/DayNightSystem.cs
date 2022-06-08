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

        //starDome �ʱⰪ
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
        //��Ÿ���� õõ�� ȸ���ϰ� 
        StarDome.transform.Rotate(new Vector3(0, 2f * Time.deltaTime, 0));

        //�ð� ����
        time += timeRate * Time.deltaTime;

        //��¥ ����,�ð� �ʱ�ȭ
        if (time >= 1.0f)
        {
			if(!SceneFlag)
				RecordInformation.SaveAnimalCount();
			Day++;
            time = 0.0f;
        }

		//���ѵ� ��¥�� �Ǹ�
		if (DayLimit != 0 && Day >= DayLimit)
		{
			RandomObjectGenerator.instance.DisableAll();
			SceneManager.LoadScene("ScoreBoard");
		}

        //�ؿ� �� �����̱�
        sun.transform.eulerAngles = (time - 0.25f) * noon * 4.0f;
        moon.transform.eulerAngles = (time - 0.75f) * noon * 4.0f;


        //�� �е� 
        sun.intensity = sunIntensity.Evaluate(time);
        moon.intensity = moonIntensity.Evaluate(time);

        //�ð��� ���� ���򺯰�
        sun.color = sunColor.Evaluate(time);
        moon.color = moonColor.Evaluate(time);

        //�ؿ� �� �������
        
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
        
        
  
        //���� �� ���� 
        if (time > 0.25f && time < 0.75f)
        {
            //�� (��Ÿ�� �Ⱥ��̰�)
            //sun.gameObject.SetActive(true);
            //moon.gameObject.SetActive(false);

            starMat.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, Time.deltaTime));
            AMPM = "Day";
       
        }
        else
        {
            //�� (��Ÿ�� ���̰�)
            starMat.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, Time.deltaTime));
            AMPM = "Night";

           // moon.gameObject.SetActive(true);
           // sun.gameObject.SetActive(false);
        }
    


        //�� �ݻ� �е� 
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
