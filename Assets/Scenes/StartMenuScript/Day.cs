using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Day : MonoBehaviour
{
	public void UploadDay()
	{
		int day;
		if (gameObject.GetComponent<Text>().text.Length == 0)
			day = 0;
		else day = int.Parse(gameObject.GetComponent<Text>().text);
		PlayerPrefs.SetInt("DayLimit", day);
	}
}
