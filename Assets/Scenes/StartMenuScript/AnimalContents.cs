using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AnimalContents : MonoBehaviour
{
	public Dropdown AnimalDropDown;
	public InputField CntInputField;
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
		if (CntInputField.textComponent.text.Length == 0
			|| (CntInputField.textComponent.text.Length == 1 && CntInputField.textComponent.text[0] == '0'))
			return;
		GameObject animalContent = Instantiate(animalInfo);
		animalContent.transform.position = new Vector3(920, contentY, 0);
		contentY += -200;
		animalContent.transform.SetParent(transform);
		animalContent.SetActive(true);
		Text name = animalContent.transform.Find("Name").gameObject.GetComponent<Text>();
		name.text = AnimalDropDown.captionText.text;
		Text count = animalContent.transform.Find("Count").gameObject.GetComponent<Text>();
		count.text = int.Parse(CntInputField.textComponent.text).ToString();
		Button remove =	animalContent.transform.transform.Find("Remove").GetComponent<Button>();
		remove.onClick.AddListener(OnRemoveClick);
	}

	public void OnRemoveClick()
	{
		Destroy(EventSystem.current.currentSelectedGameObject.transform.parent.gameObject);
	}
}
