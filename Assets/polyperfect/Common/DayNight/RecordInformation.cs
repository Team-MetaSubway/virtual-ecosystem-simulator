using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RecordInformation : MonoBehaviour
{
	RandomObjectGenerator RandomObjectGenerator;
	List<int[]> DayAnimalCount = new List<int[]>();
	public static RecordInformation instance = null;

	private void Awake()
	{
		RandomObjectGenerator = GameObject.Find("ObjectGenerator").GetComponent<RandomObjectGenerator>();
		instance = this;
	}

	public void SaveAnimalCount()
	{
		DayAnimalCount.Add(RandomObjectGenerator.SaveAnimalCount());
	}

	public double CalDeviation()
	{
		int[] sum = Enumerable.Repeat<int>(0, (int)AnimalList.Animal.NumOfAnimals - 1).ToArray<int>();
		int cnt = 0;
		foreach(var curlist in DayAnimalCount)
		{
			cnt++;
			for (int i = 0; i < (int)AnimalList.Animal.NumOfAnimals - 1; i++) 
			{
				sum[i] += curlist[i];
			}
		}

		double[] average = Enumerable.Repeat<double>(0, (int)AnimalList.Animal.NumOfAnimals - 1).ToArray<double>();
		for (int i = 0; i < (int)AnimalList.Animal.NumOfAnimals - 1; i++)
		{
			average[i] = (double)sum[i] / cnt;
		}

		double[] deviation = Enumerable.Repeat<double>(0, (int)AnimalList.Animal.NumOfAnimals - 1).ToArray<double>();
		foreach (var curlist in DayAnimalCount)
		{
			for (int i = 0; i < (int)AnimalList.Animal.NumOfAnimals - 1; i++)
			{
				deviation[i] += (curlist[i] - average[i]) * (curlist[i] - average[i]);
			}
		}

		double deviationAver = 0;
		for (int i = 0; i < (int)AnimalList.Animal.NumOfAnimals - 1; i++)
		{
			deviation[i] = Math.Sqrt(deviation[i] / cnt);
			deviationAver += deviation[i];
		}
		deviationAver /= (int)AnimalList.Animal.NumOfAnimals - 1;

		return deviationAver;
	}
}
