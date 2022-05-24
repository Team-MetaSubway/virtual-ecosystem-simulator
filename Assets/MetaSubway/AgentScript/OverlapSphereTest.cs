using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OverlapSphereTest : MonoBehaviour
{
    // Start is called before the first frame update

    private Vector3[] observationArr = new Vector3[5];
    int i;

   
    void Start()
    {
        
        
        Vector3 nowPos = transform.position;
        var listOfColliders = Physics.OverlapSphere(nowPos, Mathf.Sqrt(10 * 10 * 3 + 1));
        var orderedByProximity = listOfColliders.OrderBy(c => (c.transform.position-nowPos).sqrMagnitude).ToArray();

        int len = Mathf.Min(6, orderedByProximity.Length);
        for (i = 1; i < len; ++i)
        {
            observationArr[i - 1] = new Vector3(orderedByProximity[i].gameObject.layer,
                                  Mathf.Atan2(orderedByProximity[i].transform.position.x - nowPos.x, orderedByProximity[i].transform.position.z - nowPos.z) * Mathf.Rad2Deg,
                                  (orderedByProximity[i].transform.position - nowPos).magnitude
                                  );
        }

        for(i=orderedByProximity.Length-1; i<5; ++i)
        {
            observationArr[i] = new Vector3(-1, -1, -1);
        }
        for (int i = 0; i < 5; ++i) Debug.Log(observationArr[i]);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
