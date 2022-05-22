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

	public bool enableStatusBar = false;
	public bool enableRespawnHerbivore = false;
    public void Start()
	{
		StartCoroutine(GenerateObject());

		if(enableRespawnHerbivore)
			StartCoroutine(RespawnObject());
	}

	IEnumerator GenerateObject()
    {
		yield return new WaitForSeconds(1.0f);

		//�Ĺ� ����
		foreach (var plant in plantPrefabs) SpawnPlant(plant);
		
		//���� ����
		foreach (var animal in animalPrefabs) SpawnAnimal(animal);

	}

	IEnumerator RespawnObject()
    {
		while(true)
        {
			foreach(var animal in animalGameObjects)
            {
				if (animal.GetComponent<Polyperfect.Common.Common_WanderScript>().enabled == false) 
					animal.GetComponent<Polyperfect.Common.Common_WanderScript>().enabled = true;
			}
			yield return new WaitForSeconds(5.0f);
        }
    }

	private Vector3 GetRandomPosition()
	{ 
		Vector3 spawnPos = new Vector3(Random.Range(-LearningEnvController.instance.mapWidth / 2f, LearningEnvController.instance.mapWidth / 2f),
									   LearningEnvController.instance.mapMaxHeight, 
									   Random.Range(-LearningEnvController.instance.mapLength / 2f, LearningEnvController.instance.mapLength / 2f));
		Ray ray = new Ray(spawnPos, Vector3.down); //���� ��ǥ�� �����ؼ� ����.
		RaycastHit hitData;
		Physics.Raycast(ray, out hitData); //���� �������� ���� ��ġ(Y���� maxHeight)���� ������ ���� ���.
		spawnPos.y -= hitData.distance; //���� ���� �Ÿ���ŭ y���� ����. ������ ���� �ٴڿ� �� �°� �����ǰԲ�.
		Debug.Log("generator. ���� ����Ʈ��" + spawnPos + "�Ÿ���" + hitData.distance);
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
				animalGameObjects.Add(instance);
			}
		}
		else
        {
			while (cnt-- > 0)
			{
				//GameObject instance = Instantiate(animalPrefab, GetRandomPosition(), Quaternion.identity, transform);
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

	[System.Serializable]
	public struct PFinfo
	{
		//���� ������Ʈ
		public GameObject prefab;
		//���� ������Ʈ ����
		public int count;
	}
}
