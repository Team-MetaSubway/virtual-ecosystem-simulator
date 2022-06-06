using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UpdateValue : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		double IdealValue = double.Parse(GameObject.Find("IdealValue").GetComponent<TextMeshProUGUI>().text);
		double SimValue = RecordInformation.instance.CalDeviation();
		gameObject.GetComponent<TextMeshProUGUI>().text = Math.Round(SimValue, 6).ToString();
		GameObject.Find("ChangeRate").GetComponent<TextMeshProUGUI>().text
			= Math.Round(((IdealValue - SimValue) / IdealValue * 100), 6).ToString();
	}
}
