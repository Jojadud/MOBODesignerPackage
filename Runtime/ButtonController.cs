using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    public Action OnClick;

    private bool disabled_ = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetText(string text)
    {
        transform.GetComponentInChildren<TextMesh>().text = text;
    }

    public void SetDisabled(bool disabled)
    {
        disabled_ = disabled;

        if (disabled_)
        {
            transform.GetComponentInChildren<TextMesh>().color = new Color(0.8f,0.8f,0.8f);
        }
        else
        {
            transform.GetComponentInChildren<TextMesh>().color = Color.black;
        }

    }

    void OnMouseDown()
    {
        if (!disabled_)
        {
            OnClick?.Invoke();
        }
    }
}
