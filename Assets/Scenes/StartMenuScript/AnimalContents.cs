using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimalContents : MonoBehaviour
{
	public Dropdown animalDropDown;
	public InputField cntInputField;
	public GameObject animalInfo;
	int contentCnt;
	int contentY;

	private void Start()
	{
		contentCnt = 0;
		contentY = -120;
	}

	public void OnSelectClick()
	{
		if (cntInputField.textComponent.text.Length == 0)
			return;
		GameObject animalContent = Instantiate(animalInfo);
		animalContent.transform.position = new Vector3(920, contentY, 0);
		contentY += -200;
		animalContent.transform.parent = transform;
		animalContent.SetActive(true);
		Text name = animalContent.transform.Find("Name").gameObject.GetComponent<Text>();
		name.text = animalDropDown.captionText.text;
		Text count = animalContent.transform.Find("Count").gameObject.GetComponent<Text>();
		count.text = cntInputField.textComponent.text;
	}

}
