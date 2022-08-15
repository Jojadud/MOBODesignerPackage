using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToyDesignExample : MonoBehaviour
{
    public float obj1 = 0;
    public float obj2 = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        transform.GetComponent<MOBODesigner.MDWebInterface>().OnDesignParametersUpdated = UpdateDesign;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {            
            List<float> objVals = new List<float> { obj1, obj2 };

            transform.GetComponent<MOBODesigner.MDWebInterface>().EvaluationComplete(objVals);
        }
    }

    public void UpdateDesign(List<float> paramVals)
    {
        Debug.Log("New Design: " + string.Join(",", paramVals));
    }
}
