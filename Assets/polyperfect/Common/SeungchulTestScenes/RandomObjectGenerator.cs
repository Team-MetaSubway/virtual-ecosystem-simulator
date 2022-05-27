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
		Rabbit,
		Giraffe,
		Elephant,
		Boar,
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
	private float childSpawnRange;
	
	public Dictionary<string, float> animalTagSet = new Dictionary<string, float>();


	public static RandomObjectGenerator instance = null;

	private void Awake()
	{
		mapWidth *= 0.95f;
		mapLength *= 0.95f;
		terrainLayer = LayerMask.GetMask("Terrain");
		childSpawnRange = 7f;

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
		StartCoroutine(RespawnAnimals()); //강화학습용 세팅. 동물 자동 부활.
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
				GameObject animalInstance = Instantiate(animalPrefab, transform);
				animalGameObjects.Add(animalInstance);
			}
		}
		else
        {
			while (cnt-- > 0)
			{
				GameObject animalInstance = Instantiate(animalPrefab, transform);
				animalInstance.GetComponentInChildren<StatBarController>().gameObject.SetActive(false);
				animalGameObjects.Add(animalInstance);
			}
		}
	}

	private void SpawnPlant(PFinfo plant)
    {
		int cnt = plant.count;
		GameObject plantPrefab = plant.prefab;

		while(cnt-->0)
        {
			GameObject plantInstance = Instantiate(plantPrefab, GetRandomPosition(), Quaternion.identity, transform);
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

		StartCoroutine(childAnimalInstance.GetComponent<Polyperfect.Common.Common_WanderScript>().ChildGrowthCoroutine(parentAnimalInstance));
		animalGameObjects.Add(childAnimalInstance);
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
