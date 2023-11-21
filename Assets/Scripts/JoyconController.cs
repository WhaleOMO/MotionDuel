using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyconController : MonoBehaviour
{
    public static JoyconController instance;
    private List<Joycon> _joycons;

    private void Awake()
    {
        if (instance!=null)
        {
            Destroy(instance);
        }
        instance = this;
    }

    void Start()
    {
        _joycons = JoyconManager.Instance.j;
    }

    public bool IsJoyconShoulderDown(HandEnum whichHand)
    {
        bool isDown = _joycons[(int)whichHand].GetButton(Joycon.Button.SHOULDER_2);
        #if UNITY_EDITOR
            if (isDown)
            {
                UnityEditor.EditorApplication.Beep();
            }
        #endif
        return isDown;
    }
    
    public void OnHandHoverEnter(GameObject target, HandEnum whichHand)
    {
        _joycons[(int)whichHand].SetRumble(160, 320, 0.1f, 50);
    }
}
