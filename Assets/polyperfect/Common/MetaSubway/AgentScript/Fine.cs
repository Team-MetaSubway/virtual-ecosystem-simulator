using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fine : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach(var animal in AnimalList.animalList)
        {
            Debug.Log(animal.Key + " " + animal.Value);
        }
    }

    // Update is called once per frame
}
