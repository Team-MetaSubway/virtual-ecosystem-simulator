using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Day : MonoBehaviour
{
	public void UploadDay()
	{
		PlayerPrefs.SetInt("DayLimit", int.Parse(gameObject.GetComponent<Text>().text));
	}
}
