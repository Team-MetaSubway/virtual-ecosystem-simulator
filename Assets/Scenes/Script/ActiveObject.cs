using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ActiveObject : MonoBehaviour
{
	bool flag = true;

	void Update()
    {
		GameObject MapGenerator = gameObject.transform.Find("MapGenerator").gameObject;
		string Scenename = SceneManager.GetActiveScene().name;
		if (flag && Scenename != "LoadingScene")
		{
			MapGenerator.SetActive(true);
			flag = false;
		}
		if (!flag && Scenename == "LoadingScene")
		{
			MapGenerator.SetActive(false);
			flag = true;
		}
	}
}
