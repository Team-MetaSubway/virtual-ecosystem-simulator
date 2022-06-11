using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
	public Slider progressbar;
	public Text loadtext;
	public static string loadScene;
	public static int loadType;
	private void Start()
	{
		//while (progressbar.value >= 1f)
		//{
		//	progressbar.value = Mathf.MoveTowards(progressbar.value, 1f, Time.deltaTime);
		//}
		//SceneManager.LoadScene("MainScene2");
		StartCoroutine(LoadScene());
	}
	public static void LoadSceneHandle(string _name)
	{
		loadScene = _name;
		SceneManager.LoadScene("LoadingScene");
	}
	IEnumerator LoadScene()
	{
		yield return null;
		AsyncOperation operation = SceneManager.LoadSceneAsync(loadScene);
		operation.allowSceneActivation = false;
		while(!operation.isDone)
		{
			yield return null;

			if (progressbar.value < 0.9f)
			{
				progressbar.value = Mathf.MoveTowards(progressbar.value, 0.9f, Time.deltaTime);
			}
			else if(operation.progress >= 0.9f)
			{
				progressbar.value = Mathf.MoveTowards(progressbar.value, 1f, Time.deltaTime);
			}

			if (progressbar.value >= 1f)
			{
				loadtext.text = "Press SpaceBar";
			}
			if (Input.GetKeyDown(KeyCode.Space) && progressbar.value >= 1f && operation.progress >= 0.9f)
			{
				operation.allowSceneActivation = true;
			}
		}
	}
}
