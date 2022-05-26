using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MySceneManager : MonoBehaviour
{
	int idx;

	void Awake()
	{
		idx = 1;
	}

	public void movetoscene()
	{
		SceneManager.LoadScene(idx);
	}

	public void selectscene()
	{
		GameObject clickObject = EventSystem.current.currentSelectedGameObject;
		if (clickObject.name == "Button1")
			idx = 1;
		else if (clickObject.name == "Button2")
			idx = 2;
		else if (clickObject.name == "Button3")
			idx = 3;
	}
}
