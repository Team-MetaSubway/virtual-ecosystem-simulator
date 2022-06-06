using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class RandomObjectGenerator : MonoBehaviour
{
	public ObjectInfo[] animalLists; 
	public ObjectInfo[] plantLists;

	[Tooltip("size x of the map, x value")]
	public float mapWidth=180f;
	[Tooltip("size z of the map, z value")]
	public float mapLength = 180f;
	[Tooltip("maximum height of the map, y value")]
	public float mapMaxHeight = 100f;

	[Tooltip("toggle status bar")]
	public bool enableStatusBar = false;

	private List<GameObject> animalGameObjects = new List<GameObject>();
	private List<GameObject> plantGameObjects = new List<GameObject>();

	private List<WolfGroup> wolfGroups = new List<WolfGroup>();

	private int terrainLayer;
	private float childSpawnRange;
	
	public Dictionary<string, float> animalTagSet = new Dictionary<string, float>();


	public static RandomObjectGenerator instance = null;

	private int wolfTeamId = 10;

	private void Awake()
	{
		mapWidth *= 0.95f;
		mapLength *= 0.95f;
		terrainLayer = LayerMask.GetMask("Terrain");
		childSpawnRange = 7f;

		float value = 0;
		foreach (string animal in System.Enum.GetNames(typeof(AnimalList.Animal)))
		{
			animalTagSet.Add(animal, value++ / (float)AnimalList.Animal.NumOfAnimals);
		}
		DownloadAnimalData();
		instance = this;
	}

	public void Start()
	{
		StartCoroutine(GenerateObject());
		StartCoroutine(RespawnFood());
#if ENABLE_RESPAWN
		StartCoroutine(RespawnAnimals()); //강화학습용 세팅. 동물 자동 부활.
#endif

	}

	IEnumerator GenerateObject() //초기 식물, 동물 스폰.
    {
		yield return new WaitForSeconds(1.0f);

		//식물 생성
		foreach (var plant in plantLists) SpawnPlant(plant);

		//동물 생성
		foreach (var animal in animalLists) SpawnAnimal(animal);
	}

	IEnumerator RespawnAnimals() //동물 자동 부활.
    {
		while(true)
        {
			foreach(var animal in animalGameObjects)
            {
				var nowAnimalScript = animal.GetComponent<Polyperfect.Common.Common_WanderScript>();
				if (nowAnimalScript.enabled == false&&nowAnimalScript.animalType == Polyperfect.Common.Common_WanderScript.AnimalType.Herbivore) 
					nowAnimalScript.enabled = true;
			}
			yield return new WaitForSeconds(5.0f);
        }
    }
	IEnumerator RespawnFood() // 먹이 자동 리스폰. 항상 On.
    {
		yield return new WaitForSeconds(1.0f);
		GameObject plantParent = GameObject.Find(plantLists[0].name);
		
		while (true)
        {
			Instantiate(plantLists[0].prefab, GetRandomPosition(), Quaternion.identity, transform).transform.parent = plantParent.transform;
			yield return new WaitForSeconds(1.0f);
		}
    }

	public Vector3 GetRandomPosition()
	{
		Vector3 spawnPos = new Vector3(Random.Range(-mapWidth*0.5f, mapWidth*0.5f),
									   mapMaxHeight, 
									   Random.Range(-mapLength*0.5f, mapLength*0.5f));
		Ray ray = new Ray(spawnPos, Vector3.down); //월드 좌표로 변경해서 삽입.
		RaycastHit hitData;
		Physics.Raycast(ray, out hitData, 2*mapMaxHeight, terrainLayer); //현재 랜덤으로 정한 위치(Y축은 maxHeight)에서 땅으로 빛을 쏜다.
		spawnPos.y -= hitData.distance; //땅에 맞은 거리만큼 y에서 뺀다. 동물이 지형 바닥에 딱 맞게 스폰되게끔.
		return spawnPos;
	}

	private void SpawnAnimal(ObjectInfo animal)
	{

		int cnt = animal.count;
		GameObject animalPrefab = animal.prefab;

		GameObject animalParent = new GameObject(animalPrefab.name);
		animalParent.transform.parent = transform;
		animalParent.tag = "Animal";

		if (enableStatusBar == true) //GetComponentInChildren<>() 은 cost 가 높다. 가독성이 안좋더라도 if 문으로 최적화. default 는 true.
		{
			if (animal.prefab.CompareTag("Wolf"))
			{
				if (cnt % 3 != 0) Debug.LogError("number of wolf must be a multiple of 3.");
				else for (int i = 0; i < cnt / 3; ++i) CreateWolfGroup(animalPrefab);
			}
			else
			{
				while (cnt-- > 0)
				{
					GameObject animalInstance = Instantiate(animalPrefab, transform);
					animalGameObjects.Add(animalInstance);
					animalInstance.transform.parent = animalParent.transform;
				}
			}
		}
		else
        {
			if (animal.prefab.CompareTag("Wolf"))
			{
				if (cnt % 3 != 0) Debug.LogError("number of wolf must be a multiple of 3.");
				else for (int i = 0; i < cnt / 3; ++i) CreateWolfGroup(animalPrefab);
			}
			else
			{
				while (cnt-- > 0)
				{
					GameObject animalInstance = Instantiate(animalPrefab, transform);
					animalInstance.GetComponentInChildren<StatBarController>().gameObject.SetActive(false);
					animalGameObjects.Add(animalInstance);
					animalInstance.transform.parent = animalParent.transform;
				}
			}
		}
	}

	private void SpawnPlant(ObjectInfo plant)
    {
		int cnt = plant.count;
		GameObject plantPrefab = plant.prefab;


		GameObject plantParent = new GameObject(plant.name);
		plantParent.tag = "Plant";
		plantParent.transform.parent = transform;


		while (cnt-->0)
        {
			GameObject plantInstance = Instantiate(plantPrefab, GetRandomPosition(), Quaternion.identity, transform);
			plantInstance.transform.parent = plantParent.transform;
			plantGameObjects.Add(plantInstance);
		}
    }

	public void ReproduceAnimal(GameObject parentAnimalInstance)
    {
		GameObject childAnimalInstance = Instantiate(parentAnimalInstance, transform);
		//부모의 반경 7 안에 스폰되게 하드코딩. 랜덤위치가 지형 밖일 수도 있다. 랜덤위치가 지형 안쪽일때까지 랜덤위치 찾기 반복.
		var transformOfParent = parentAnimalInstance.transform.position;

		Vector3 spawnPos;
		while (true)
		{
			spawnPos = new Vector3(transformOfParent.x + Random.Range(-childSpawnRange, childSpawnRange),
								   mapMaxHeight,
								   transformOfParent.z + Random.Range(-childSpawnRange, childSpawnRange)); //랜덤 위치 잡고
			Ray ray = new Ray(spawnPos, Vector3.down); //아래 방향으로 빛 쏜다.
			RaycastHit hitData;
			if (Physics.Raycast(ray, out hitData, 2 * mapMaxHeight, terrainLayer)) //맞았으면 = 지형 안이면
			{
				spawnPos.y -= hitData.distance; //땅에 맞은 거리만큼 y에서 뺀다. 동물이 지형 바닥에 딱 맞게 스폰되게끔.
				break;
			}
		}
		childAnimalInstance.GetComponent<CharacterController>().enabled = false;
		childAnimalInstance.transform.position = spawnPos; //부모 근처, 지형 안 랜덤 좌표로 재지정
		childAnimalInstance.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 359f), 0); //바라보는 방향 랜덤하게
		childAnimalInstance.GetComponent<CharacterController>().enabled = true;

		GameObject parentObject = GameObject.Find(parentAnimalInstance.tag); //부모 오브젝트 찾고 집어넣음(개체수 세는 빈 오브젝트)
		childAnimalInstance.transform.parent = parentObject.transform;

		StartCoroutine(childAnimalInstance.GetComponent<Polyperfect.Common.Common_WanderScript>().ChildGrowthCoroutine(parentAnimalInstance));
		animalGameObjects.Add(childAnimalInstance);
	}

	[System.Serializable]
	public struct ObjectInfo
	{
		//게임 오브젝트 이름
		public string name;

		//게임 오브젝트 프리팹
		public GameObject prefab;

		//게임 오브젝트 갯수
		public int count;
	}

	public struct WolfGroup
    {
		public SimpleMultiAgentGroup wolfGroup;
		public Vector3 groupLocation;
		public int teamId;
		public List<WolfAgent> agents; 
		public Vector3 groupRandomPosition()
        {
			Vector3 spawnPos;
			while (true)
			{
				spawnPos = new Vector3(groupLocation.x + Random.Range(-5f, 5f),
									   instance.mapMaxHeight,
									   groupLocation.z + Random.Range(-5f, 5f)); //랜덤 위치 잡고
				Ray ray = new Ray(spawnPos, Vector3.down); //아래 방향으로 빛 쏜다.
				RaycastHit hitData;
				if (Physics.Raycast(ray, out hitData, 2 * instance.mapMaxHeight, instance.terrainLayer)) //맞았으면 = 지형 안이면
				{
					spawnPos.y -= hitData.distance; //땅에 맞은 거리만큼 y에서 뺀다. 동물이 지형 바닥에 딱 맞게 스폰되게끔.
					break;
				}
			}
			return spawnPos;
		}
		public void EndGroup()
        {
			agents[0].animalState.enabled = false;
			agents[1].animalState.enabled = false;
			agents[2].animalState.enabled = false;

			groupLocation = instance.GetRandomPosition();

			agents[0].myGroup.groupLocation = groupLocation;
			agents[1].myGroup.groupLocation = groupLocation;
			agents[2].myGroup.groupLocation = groupLocation;

			wolfGroup.EndGroupEpisode();
        }
    }


	void DownloadAnimalData()
	{
		for(int i=0; i<animalLists.Length; ++i)
        {
			var animal = animalLists[i];
			animal.count = PlayerPrefs.GetInt(animal.name, animal.count); //이름에 해당하는 업로드가 있었으면 받아오고, 업로드가 없었으면 그대로 현재 count 사용.
			PlayerPrefs.DeleteKey(animal.name);
			animalLists[i] = animal;
        }
	}

	public int[] SaveAnimalCount()
	{
		int[] count = new int[(int)AnimalList.Animal.NumOfAnimals - 1];
		int idx = 0;
		foreach(Transform child in transform)
		{
			if (child.tag == "Plant")
				continue;
			count[idx++] = child.childCount;
		}
		return count;
	}
	
	public void RainEffect()
    {
		foreach (var animal in animalGameObjects)
		{
			
			animal.GetComponent<Polyperfect.Common.Common_WanderScript>().RainImpact();
			
		}
	
		

		//Debug.Log("연결 완료");
    }

	public void RainEffect2()
	{
		foreach (var animal in animalGameObjects)
		{

			animal.GetComponent<Polyperfect.Common.Common_WanderScript>().RainUnimpact();

		}
	}

	private void CreateWolfGroup(GameObject wolfPrefab)
    {

		GameObject wolfParent = GameObject.Find("Wolf");

		WolfGroup nowWolfGroup = new WolfGroup();
		nowWolfGroup.groupLocation = GetRandomPosition();
		nowWolfGroup.teamId = wolfTeamId++;

		GameObject wolfInstance1 = Instantiate(wolfPrefab, transform);
		GameObject wolfInstance2 = Instantiate(wolfPrefab, transform);
		GameObject wolfInstance3 = Instantiate(wolfPrefab, transform);

		if (enableStatusBar == false)
		{
			wolfInstance1.GetComponentInChildren<StatBarController>().gameObject.SetActive(false);
			wolfInstance2.GetComponentInChildren<StatBarController>().gameObject.SetActive(false);
			wolfInstance3.GetComponentInChildren<StatBarController>().gameObject.SetActive(false);
		}
		nowWolfGroup.agents = new List<WolfAgent>();

		nowWolfGroup.agents.Add(wolfInstance1.GetComponent<WolfAgent>());
		nowWolfGroup.agents.Add(wolfInstance2.GetComponent<WolfAgent>());
		nowWolfGroup.agents.Add(wolfInstance3.GetComponent<WolfAgent>());

		nowWolfGroup.wolfGroup = new SimpleMultiAgentGroup();
		nowWolfGroup.wolfGroup.RegisterAgent(nowWolfGroup.agents[0]);
		nowWolfGroup.wolfGroup.RegisterAgent(nowWolfGroup.agents[1]);
		nowWolfGroup.wolfGroup.RegisterAgent(nowWolfGroup.agents[2]);


		animalGameObjects.Add(wolfInstance1);
		animalGameObjects.Add(wolfInstance2);
		animalGameObjects.Add(wolfInstance3);

		wolfInstance1.transform.parent = wolfParent.transform;
		wolfInstance2.transform.parent = wolfParent.transform;
		wolfInstance3.transform.parent = wolfParent.transform;

		nowWolfGroup.agents[0].SetGroup(nowWolfGroup, 0);
		nowWolfGroup.agents[1].SetGroup(nowWolfGroup, 1);
		nowWolfGroup.agents[2].SetGroup(nowWolfGroup, 2);

		StartCoroutine(WolfGroupRewardCoroutine(nowWolfGroup));
		wolfGroups.Add(nowWolfGroup);
	}

	IEnumerator WolfGroupRewardCoroutine(WolfGroup group)
    {
		float negativeReward = 0.03f;
		float positiveReward = -0.03f;
		while (true)
		{
			yield return new WaitForSeconds(1.0f);
			Vector3 pos1 = group.agents[0].transform.position;
			Vector3 pos2 = group.agents[1].transform.position;
			Vector3 pos3 = group.agents[2].transform.position;

			float distance = (pos1 - pos2).sqrMagnitude + (pos2 - pos3).sqrMagnitude + (pos3 - pos1).sqrMagnitude;
			if (distance < 2000f)
			{
				group.wolfGroup.AddGroupReward(positiveReward);
			}
			else
            {
				group.wolfGroup.AddGroupReward(negativeReward);
            }
		}
    }
}
