using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interaction handler is a component that passed OnMouse down through an action
/// </summary>
public class InteractionHandler : MonoBehaviour
{
    /// <summary>
    /// On click is the action that gets called when clicked
    /// </summary>
    public Action OnClick;


    private void OnMouseDown()
    {

        if (OnClick != null)  OnClick.Invoke();
    }
}
