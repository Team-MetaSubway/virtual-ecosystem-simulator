using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnimalCountDiff : MonoBehaviour
{
	public GameObject DiffInfo;
	// Start is called before the first frame update
	void Start()
	{
		int[] startcount = RecordInformation.instance.DayAnimalCount[0];
		int[] endcount = RecordInformation.instance.DayAnimalCount[RecordInformation.instance.DayAnimalCount.Count - 1];

		for (int i = 0; i < (int)AnimalList.Animal.NumOfAnimals - 1; i++)
		{
			if (startcount[i] == 0)
				continue;

			GameObject animalContent = Instantiate(DiffInfo);
			animalContent.transform.SetParent(transform);

			animalContent.transform.Find("Name").gameObject.GetComponent<TextMeshProUGUI>().text = RecordInformation.instance.AnimalNames[i];
			animalContent.transform.Find("StartCount").gameObject.GetComponent<TextMeshProUGUI>().text = startcount[i].ToString();
			animalContent.transform.Find("EndCount").gameObject.GetComponent<TextMeshProUGUI>().text = endcount[i].ToString();

			animalContent.SetActive(true);
		}
	}
}
