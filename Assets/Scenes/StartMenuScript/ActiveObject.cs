using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ActiveObject : MonoBehaviour
{
	bool flag = true;

	private void Start()
	{
		
	}

	void Update()
    {
		if (SceneManager.GetActiveScene().name == "MainScene2" && flag)
		{
			GameObject MapGenerator = gameObject.transform.Find("MapGenerator").gameObject;
			MapGenerator.SetActive(true);
			SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
			MapGenerator.transform.parent = null;
			Destroy(gameObject);
			flag = false;
		}
    }
}
