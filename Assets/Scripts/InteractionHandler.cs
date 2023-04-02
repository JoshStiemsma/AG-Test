using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionHandler : MonoBehaviour
{
    public Action OnClick;


    private void OnMouseDown()
    {
        Debug.Log("OnMouseDown");

        if (OnClick != null)  OnClick.Invoke();
    }
}
