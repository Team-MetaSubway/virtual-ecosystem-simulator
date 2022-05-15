using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryTrigger : MonoBehaviour
{
	Transform UserTransform;
	Rigidbody UserRigidbody;

	private void Awake()
	{
		UserTransform = GetComponent<Transform>();
		UserRigidbody = GetComponent<Rigidbody>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Boundary")
		{
			UserRigidbody.velocity = Vector3.zero;
			UserTransform.Translate(0, 20, -20);
		}
	}
}
