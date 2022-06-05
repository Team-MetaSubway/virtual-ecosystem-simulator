using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RecordInformation : MonoBehaviour
{
	RandomObjectGenerator RandomObjectGenerator;
	List<int[]> DayAnimalCount = new List<int[]>();
	bool flag = true;

	private void Awake()
	{
		RandomObjectGenerator = GameObject.Find("ObjectGenerator").GetComponent<RandomObjectGenerator>();
	}

	private void Update()
	{
	
		string Scenename = SceneManager.GetActiveScene().name;
		if (Scenename == "ScoreBoard" && flag)
		{
			flag = false;
			PrintCount();
		}
	}

	public void SaveAnimalCount()
	{
		DayAnimalCount.Add(RandomObjectGenerator.SaveAnimalCount());
	}

	public void PrintCount()
	{
		foreach(var curlist in DayAnimalCount)
		{
			for (int i = 0; i < (int)AnimalList.Animal.NumOfAnimals - 1; i++) 
			{
				Debug.Log(RandomObjectGenerator.animalLists[i].name + " " + curlist[i]);
			}
		}
	}
}
