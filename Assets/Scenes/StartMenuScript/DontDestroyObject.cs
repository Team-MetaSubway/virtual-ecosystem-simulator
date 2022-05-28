using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyObject : MonoBehaviour
{
	int aa;

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
}
