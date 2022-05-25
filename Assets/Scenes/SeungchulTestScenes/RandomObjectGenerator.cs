using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjectGenerator : MonoBehaviour
{
	public PFinfo[] animalPrefabs;
	public PFinfo[] plantPrefabs;

	[Tooltip("size x of the map, x value")]
	public float mapWidth=180f;
	[Tooltip("size z of the map, z value")]
	public float mapLength = 180f;
	[Tooltip("maximum height of the map, y value")]
	public float mapMaxHeight = 100f;

	[Tooltip("toggle status bar")]
	public bool enableStatusBar = false;

	private List<Polyperfect.Common.Common_WanderScript> animalGameObjects = new List<Polyperfect.Common.Common_WanderScript>();
	private List<GameObject> plantGameObjects = new List<GameObject>();

	private int terrainLayer;

	public static RandomObjectGenerator instance = null;

	private void Awake()
	{
		mapWidth *= 0.95f;
		mapLength *= 0.95f;
		terrainLayer = LayerMask.GetMask("Terrain");
		instance = this;
	}

	public void Start()
	{
		StartCoroutine(GenerateObject());
#if ENABLE_RESPAWN
		StartCoroutine(RespawnAnimals());
		StartCoroutine(RespawnFood());
#endif
	}

	IEnumerator GenerateObject()
    {
		yield return new WaitForSeconds(1.0f);

		//�Ĺ� ����
		foreach (var plant in plantPrefabs) SpawnPlant(plant);
		
		//���� ����
		foreach (var animal in animalPrefabs) SpawnAnimal(animal);

	}

	IEnumerator RespawnAnimals()
    {
		while(true)
        {
			foreach(var animal in animalGameObjects)
            {
				if (animal.enabled == false) 
					animal.enabled = true;
			}
			yield return new WaitForSeconds(5.0f);
        }
    }
	IEnumerator RespawnFood()
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
				//GameObject instance = Instantiate(animalPrefab, GetRandomPosition(), Quaternion.identity, transform);
				GameObject instance = Instantiate(animalPrefab, transform);
				animalGameObjects.Add(instance.GetComponent<Polyperfect.Common.Common_WanderScript>());
			}
		}
		else
        {
			while (cnt-- > 0)
			{
				//GameObject instance = Instantiate(animalPrefab, GetRandomPosition(), Quaternion.identity, transform);
				GameObject instance = Instantiate(animalPrefab, transform);
				instance.GetComponentInChildren<StatBarController>().gameObject.SetActive(false);
				animalGameObjects.Add(instance.GetComponent<Polyperfect.Common.Common_WanderScript>());
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

	[System.Serializable]
	public struct PFinfo
	{
		//���� ������Ʈ
		public GameObject prefab;
		//���� ������Ʈ ����
		public int count;
	}
}
