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

	private float mapWidth;//가로(x축) 맵 크기, 0.95만큼 다운 사이즈 하드코딩.
	private float mapLength; //세로(z축) 맵 크기, 0.95만큼 다운 사이즈 하드코딩.
	private float mapMaxHeight; //최대 높이(y축) 맵 높이

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

		//동물 생성
		foreach (var animal in animalPrefabs) SpawnAnimal(animal);

		//식물 생성
		foreach (var plant in plantPrefabs) SpawnPlant(plant);
	}

	private Vector3 GetRandomPosition()
	{ 
		Vector3 spawnPos = new Vector3(Random.Range(-mapWidth / 2f, mapWidth / 2f), mapMaxHeight, Random.Range(-mapLength / 2f, mapLength / 2f));
		Ray ray = new Ray(spawnPos, Vector3.down); //월드 좌표로 변경해서 삽입.
		RaycastHit hitData;
		Physics.Raycast(ray, out hitData); //현재 랜덤으로 정한 위치(Y축은 maxHeight)에서 땅으로 빛을 쏜다.
		spawnPos.y -= hitData.distance; //땅에 맞은 거리만큼 y에서 뺀다. 동물이 지형 바닥에 딱 맞게 스폰되게끔.

		return spawnPos;
	}

	private void SpawnAnimal(PFinfo animal)
	{

		int cnt = animal.count;
		GameObject animalPrefab = animal.prefab;

		if (enableStatusBar == true) //GetComponentInChildren<>() 은 cost 가 높다. 가독성이 안좋더라도 if 문으로 최적화. default 는 false.
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
		//게임 오브젝트
		public GameObject prefab;
		//게임 오브젝트 갯수
		public int count;
	}
}
