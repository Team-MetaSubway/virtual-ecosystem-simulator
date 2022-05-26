using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjectGenerator : MonoBehaviour
{
	public PFinfo[] animalPrefabs;
	public PFinfo[] plantPrefabs;

	public enum Animal
	{
		EmptyAnimal = 0,
		Bear,
		Beaver,
		NumOfAnimals
	}


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

	private int terrainLayer;
	public Dictionary<string, float> animalTagSet = new Dictionary<string, float>();


	public static RandomObjectGenerator instance = null;

	private void Awake()
	{
		mapWidth *= 0.95f;
		mapLength *= 0.95f;
		terrainLayer = LayerMask.GetMask("Terrain");

		float value = 0;
		foreach (string animal in System.Enum.GetNames(typeof(Animal)))
		{
			animalTagSet.Add(animal, value++ / (float)Animal.NumOfAnimals);
		}
		instance = this;
	}

	public void Start()
	{
		StartCoroutine(GenerateObject());
	    StartCoroutine(RespawnFood());

#if ENABLE_RESPAWN
		StartCoroutine(RespawnAnimals()); //강화학습용 세팅. 동물 무한 부활.
#endif

	}

	IEnumerator GenerateObject() //초기 식물, 동물 스폰.
    {
		yield return new WaitForSeconds(1.0f);

		//식물 생성
		foreach (var plant in plantPrefabs) SpawnPlant(plant);
		
		//동물 생성
		foreach (var animal in animalPrefabs) SpawnAnimal(animal);

	}

	IEnumerator RespawnAnimals() //동물 자동 부활.
    {
		while(true)
        {
			foreach(var animal in animalGameObjects)
            {
				var nowAnimalScript = animal.GetComponent<Polyperfect.Common.Common_WanderScript>();
				if (nowAnimalScript.enabled == false) 
					nowAnimalScript.enabled = true;
			}
			yield return new WaitForSeconds(5.0f);
        }
    }
	IEnumerator RespawnFood() // 먹이 자동 리스폰. 항상 On.
    {
		while(true)
        {
			Instantiate(plantPrefabs[0].prefab, GetRandomPosition(), Quaternion.identity, transform);
			yield return new WaitForSeconds(5.0f);
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

	private void SpawnAnimal(PFinfo animal)
	{

		int cnt = animal.count;
		GameObject animalPrefab = animal.prefab;

		if (enableStatusBar == true) //GetComponentInChildren<>() 은 cost 가 높다. 가독성이 안좋더라도 if 문으로 최적화. default 는 true.
		{
			while (cnt-- > 0)
			{
				GameObject instance = Instantiate(animalPrefab, transform);
				animalGameObjects.Add(instance);
			}
		}
		else
        {
			while (cnt-- > 0)
			{
				GameObject instance = Instantiate(animalPrefab, transform);
				instance.GetComponentInChildren<StatBarController>().gameObject.SetActive(false);
				animalGameObjects.Add(instance);
			}
		}
	}

	private void SpawnPlant(PFinfo plant)
    {
		int cnt = plant.count;
		GameObject plantPrefab = plant.prefab;

		while(cnt-->0)
        {
			GameObject instance = Instantiate(plantPrefab, GetRandomPosition(), Quaternion.identity, transform);
			plantGameObjects.Add(instance);
		}
    }

	public void ReproduceAnimal(GameObject animalObject)
    {
		GameObject instance = Instantiate(animalObject, transform);
		StartCoroutine(instance.GetComponent<Polyperfect.Common.Common_WanderScript>().ChildGrowthCoroutine(gameObject));
		animalGameObjects.Add(instance);
	}

	[System.Serializable]
	public struct PFinfo
	{
		//게임 오브젝트
		public GameObject prefab;
		//게임 오브젝트 갯수
		public int count;
	}
}
