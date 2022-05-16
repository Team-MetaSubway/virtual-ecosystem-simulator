using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomObjectGenerator : MonoBehaviour
{
	public PFinfo[] prefabs;
	private BoxCollider area;

	private List<GameObject> gameObject = new List<GameObject>();

	float maxHeightOfMap;

    private void Awake()
    {
		maxHeightOfMap = FindObjectOfType<LearningEnvController>().mapMaxHeight;
    }

    public void Start()
	{
		StartCoroutine(GenerateObject());
	}

	IEnumerator GenerateObject()
    {
		yield return new WaitForSeconds(1.0f);
		area = GetComponent<BoxCollider>();

		//prefab선택
		for (int i = 0; i < prefabs.Length; ++i)
		{
			//count개 생성
			int count = prefabs[i].count;
			for (int j = 0; j < count; j++)
				Spawn(i);
		}
		area.enabled = false;
	}

	private Vector3 GetRandomPosition()
	{
		Vector3 basePosition = transform.position;
		Vector3 size = area.size;

		float posX = basePosition.x + Random.Range(-size.x / 2f, size.x / 2f);
		//float posY = basePosition.y + Random.Range(-size.y / 2f, size.y / 2f);
		float posZ = basePosition.z + Random.Range(-size.z / 2f, size.z / 2f);

		Vector3 spawnPos = new Vector3(posX, maxHeightOfMap, posZ);
		Ray ray = new Ray(spawnPos, Vector3.down); //월드 좌표로 변경해서 삽입.
		RaycastHit hitData;
		Physics.Raycast(ray, out hitData); //현재 랜덤으로 정한 위치(Y축은 maxHeight)에서 땅으로 빛을 쏜다.
		spawnPos.y -= hitData.distance; //땅에 맞은 거리만큼 y에서 뺀다. 동물이 지형 바닥에 딱 맞게 스폰되게끔.


		Debug.Log("generator position is:"+ spawnPos);
		return spawnPos;
	}

	private void Spawn(int idx)
	{
		GameObject selectedPrefab = prefabs[idx].prefab;

		Vector3 spawnPos = GetRandomPosition();//랜덤위치함수

		//prefab, position, rotation
		//GameObject instance = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
		//instance.transform.parent = transform;
		GameObject instance = Instantiate(selectedPrefab, spawnPos, Quaternion.identity, transform);
		gameObject.Add(instance);
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
