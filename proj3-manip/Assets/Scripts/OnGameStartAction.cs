using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class OnGameStartAction : MonoBehaviour
{
    public UnityEvent OnStart;

    void Start()
    {
        OnStart.Invoke();
    }
}
