using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class JoyconController : MonoBehaviour
{
    public static JoyconController instance;
    private List<Joycon> _joycons;
    private List<bool> isUp;

    private void Awake()
    {
        if (instance!=null)
        {
            Destroy(instance);
        }
        instance = this;

        isUp = new List<bool>(4);
        for (int i = 0; i < isUp.Capacity; i++)
        {
            isUp.Add(true);
        }
    }

    void Start()
    {
        _joycons = JoyconManager.Instance.j;
        if (_joycons.Count < 2)
        {
            Debug.LogWarning("Only one joycon connected");
        }
    }

    private void Update()
    {
        if (_joycons[(int)HandEnum.P1Left].GetButtonUp(Joycon.Button.SHOULDER_2))
        {
            isUp[(int)HandEnum.P1Left] = true;
        }
        
        if (_joycons[(int)HandEnum.P1Right].GetButtonUp(Joycon.Button.SHOULDER_2))
        {
            isUp[(int)HandEnum.P1Right] = true;
        }
        
        /*
        if (_joycons[(int)HandEnum.P2Left].GetButtonUp(Joycon.Button.SHOULDER_2))
        {
            isUp[(int)HandEnum.P2Left] = true;
        }
        
        if (_joycons[(int)HandEnum.P2Right].GetButtonUp(Joycon.Button.SHOULDER_2))
        {
            isUp[(int)HandEnum.P2Right] = true;
        }
        */
    }

    public bool IsJoyconShoulderDown(HandEnum whichHand)
    {
        bool isDown = _joycons[(int)whichHand].GetButton(Joycon.Button.SHOULDER_2);

        if (isUp[(int)whichHand] && isDown)
        {
            return true;
        }

        return false;
    }
    
    public void OnHandHoverEnter(GameObject target, HandEnum whichHand)
    {
        _joycons[(int)whichHand].SetRumble(160, 320, 0.1f, 50);
    }

    public void RestKeyState(bool state)
    {
        for (int i = 0; i < isUp.Capacity; i++)
        {
            isUp[i] = state;
        }
    }
}
