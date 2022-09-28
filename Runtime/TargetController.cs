using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    public Action<int,float> OnSelect;

    public int id = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        // Compute touch offset error (from target centre)
        Vector3 mousePos = Input.mousePosition;
        Vector3 targetPosScreen = Camera.main.WorldToScreenPoint(transform.position);
        float error = Mathf.Sqrt(Mathf.Pow(mousePos.x - targetPosScreen.x, 2) + Mathf.Pow(mousePos.y - targetPosScreen.y, 2));        

        OnSelect?.Invoke(id,error);
    }
}
