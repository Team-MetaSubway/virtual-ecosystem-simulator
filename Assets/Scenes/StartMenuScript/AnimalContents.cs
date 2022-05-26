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

	public void OnSelectClick()
	{
		if (CntInputField.textComponent.text.Length == 0 || int.Parse(CntInputField.textComponent.text) == 0)
			return;
		GameObject animalContent = Instantiate(animalInfo);
		animalContent.transform.position = new Vector3(920, -120, 0);
		animalContent.transform.SetParent(transform);

		Text name = animalContent.transform.Find("Name").gameObject.GetComponent<Text>();
		name.text = AnimalDropDown.captionText.text;

		Text count = animalContent.transform.Find("Count").gameObject.GetComponent<Text>();
		count.text = int.Parse(CntInputField.textComponent.text).ToString();

		Button remove =	animalContent.transform.transform.Find("Remove").GetComponent<Button>();
		remove.onClick.AddListener(OnRemoveClick);
		animalContent.SetActive(true);
	}

	public void OnRemoveClick()
	{
		Destroy(EventSystem.current.currentSelectedGameObject.transform.parent.gameObject);
	}
}
