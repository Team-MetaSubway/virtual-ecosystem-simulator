using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimalList
{
    // Start is called before the first frame update
    public static readonly Dictionary<string, string> animalList =
        new Dictionary<string , string>
        {
            { "Rabbit Brown","prefab ���" },
            { "Deer","prefab ���"},
            { "Giraffe","prefab ���"},
            { "Elephant","prefab ���"},
            { "Wolf Grey","prefab ���"},
            { "Boar","prefab ���"},
            { "Lion","prefab ���"},
            { "Bear","prefab ���"}
        };
}
