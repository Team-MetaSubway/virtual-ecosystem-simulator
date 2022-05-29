using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimalList
{
    // Start is called before the first frame update
    public static readonly Dictionary<string, string> animalList =
        new Dictionary<string , string>
        {
            { "Rabbit Brown","prefab 경로" },
            { "Deer","prefab 경로"},
            { "Giraffe","prefab 경로"},
            { "Elephant","prefab 경로"},
            { "Wolf Grey","prefab 경로"},
            { "Boar","prefab 경로"},
            { "Lion","prefab 경로"},
            { "Bear","prefab 경로"}
        };
}
