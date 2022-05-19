using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomObjectGenerator : MonoBehaviour
{
	public PFinfo[] animalPrefabs;
	public PFinfo[] plantPrefabs;
	private BoxCollider area;

	private List<GameObject> animalGameObjects = new List<GameObject>();
	private List<GameObject> plantGameObjects = new List<GameObject>();

	private LearningEnvController learningEnvController;

	private float mapWidth;//����(x��) �� ũ��, 0.95��ŭ �ٿ� ������ �ϵ��ڵ�.
	private float mapLength; //����(z��) �� ũ��, 0.95��ŭ �ٿ� ������ �ϵ��ڵ�.
	private float mapMaxHeight; //�ִ� ����(y��) �� ����

	public bool enableStatusBar = false;

    public void Start()
	{
		learningEnvController = GetComponent<LearningEnvController>();
		
		mapWidth = learningEnvController.mapWidth*0.95f;
		mapLength = learningEnvController.mapLength*0.95f;
		mapMaxHeight = learningEnvController.mapMaxHeight;

		StartCoroutine(GenerateObject());
	}

	IEnumerator GenerateObject()
    {
		yield return new WaitForSeconds(1.0f);

		//���� ����
		foreach (var animal in animalPrefabs) SpawnAnimal(animal);

		//�Ĺ� ����
		foreach (var plant in plantPrefabs) SpawnPlant(plant);
	}

	private Vector3 GetRandomPosition()
	{ 
		Vector3 spawnPos = new Vector3(Random.Range(-mapWidth / 2f, mapWidth / 2f), mapMaxHeight, Random.Range(-mapLength / 2f, mapLength / 2f));
		Ray ray = new Ray(spawnPos, Vector3.down); //���� ��ǥ�� �����ؼ� ����.
		RaycastHit hitData;
		Physics.Raycast(ray, out hitData); //���� �������� ���� ��ġ(Y���� maxHeight)���� ������ ���� ���.
		spawnPos.y -= hitData.distance; //���� ���� �Ÿ���ŭ y���� ����. ������ ���� �ٴڿ� �� �°� �����ǰԲ�.

		return spawnPos;
	}

	private void SpawnAnimal(PFinfo animal)
	{

		int cnt = animal.count;
		GameObject animalPrefab = animal.prefab;

		if (enableStatusBar == true) //GetComponentInChildren<>() �� cost �� ����. �������� �������� if ������ ����ȭ. default �� false.
		{
			while (cnt-- > 0)
			{
				GameObject instance = Instantiate(animalPrefab, GetRandomPosition(), Quaternion.identity, transform);
				instance.GetComponentInChildren<StatBarController>().gameObject.SetActive(true);
				animalGameObjects.Add(instance);
			}
		}
		else
        {
			while (cnt-- > 0)
			{
				GameObject instance = Instantiate(animalPrefab, GetRandomPosition(), Quaternion.identity, transform);
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

	[System.Serializable]
	public struct PFinfo
	{
		//���� ������Ʈ
		public GameObject prefab;
		//���� ������Ʈ ����
		public int count;
	}
}
