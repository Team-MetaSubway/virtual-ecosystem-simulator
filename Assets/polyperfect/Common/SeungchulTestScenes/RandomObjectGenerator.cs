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
		StartCoroutine(RespawnAnimals()); //��ȭ�н��� ����. ���� �ڵ� ��Ȱ.
#endif

	}

	IEnumerator GenerateObject() //�ʱ� �Ĺ�, ���� ����.
    {
		yield return new WaitForSeconds(1.0f);

		//�Ĺ� ����
		foreach (var plant in plantPrefabs) SpawnPlant(plant);
		
		//���� ����
		foreach (var animal in animalPrefabs) SpawnAnimal(animal);

	}

	IEnumerator RespawnAnimals() //���� �ڵ� ��Ȱ.
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
	IEnumerator RespawnFood() // ���� �ڵ� ������. �׻� On.
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
		Ray ray = new Ray(spawnPos, Vector3.down); //���� ��ǥ�� �����ؼ� ����.
		RaycastHit hitData;
		Physics.Raycast(ray, out hitData, 2*mapMaxHeight, terrainLayer); //���� �������� ���� ��ġ(Y���� maxHeight)���� ������ ���� ���.
		spawnPos.y -= hitData.distance; //���� ���� �Ÿ���ŭ y���� ����. ������ ���� �ٴڿ� �� �°� �����ǰԲ�.
		return spawnPos;
	}

	private void SpawnAnimal(PFinfo animal)
	{

		int cnt = animal.count;
		GameObject animalPrefab = animal.prefab;

		if (enableStatusBar == true) //GetComponentInChildren<>() �� cost �� ����. �������� �������� if ������ ����ȭ. default �� true.
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
		//�θ��� �ݰ� 7 �ȿ� �����ǰ� �ϵ��ڵ�. ������ġ�� ���� ���� ���� �ִ�. ������ġ�� ���� �����϶����� ������ġ ã�� �ݺ�.
		var transformOfParent = parentAnimalInstance.transform.position;

		Vector3 spawnPos;
		while (true)
		{
			spawnPos = new Vector3(transformOfParent.x + Random.Range(-childSpawnRange, childSpawnRange),
								   mapMaxHeight,
								   transformOfParent.z + Random.Range(-childSpawnRange, childSpawnRange)); //���� ��ġ ���
			Ray ray = new Ray(spawnPos, Vector3.down); //�Ʒ� �������� �� ���.
			RaycastHit hitData;
			if (Physics.Raycast(ray, out hitData, 2 * mapMaxHeight, terrainLayer)) //�¾����� = ���� ���̸�
			{
				spawnPos.y -= hitData.distance; //���� ���� �Ÿ���ŭ y���� ����. ������ ���� �ٴڿ� �� �°� �����ǰԲ�.
				break;
			}
		}
		childAnimalInstance.GetComponent<CharacterController>().enabled = false;
		childAnimalInstance.transform.position = spawnPos; //�θ� ��ó, ���� �� ���� ��ǥ�� ������
		childAnimalInstance.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 359f), 0); //�ٶ󺸴� ���� �����ϰ�
		childAnimalInstance.GetComponent<CharacterController>().enabled = true;

		StartCoroutine(childAnimalInstance.GetComponent<Polyperfect.Common.Common_WanderScript>().ChildGrowthCoroutine(parentAnimalInstance));
		animalGameObjects.Add(childAnimalInstance);
	}

	[System.Serializable]
	public struct PFinfo
	{
		//���� ������Ʈ
		public GameObject prefab;
		//���� ������Ʈ ����
		public int count;
	}
}
