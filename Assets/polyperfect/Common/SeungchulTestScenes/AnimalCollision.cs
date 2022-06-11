using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalCollision : MonoBehaviour
{
	Rigidbody rigid;

	private void Awake()
	{
		rigid = GetComponent<Rigidbody>();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.name == "Terrain Chunk")
			Invoke("RemoveConstrants", 20);
	}

	void RemoveConstrants()
	{
		rigid.constraints = RigidbodyConstraints.None;
	}
}
