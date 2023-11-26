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
        if (_joycons.Count < 2)
        {
            return;
        }
        
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
        int handIdx = (int)whichHand;
        if (handIdx > _joycons.Count - 1)
        {
            Debug.LogFormat("hand {0} seems do not have a joycon", handIdx);
            return false;
        }
        
        bool isDown = _joycons[handIdx].GetButton(Joycon.Button.SHOULDER_2);

        if (isUp[handIdx] && isDown)
        {
            return true;
        }

        return false;
    }
    
    public void OnHandHoverEnter(GameObject target, HandEnum whichHand)
    {
        if ((int)whichHand > _joycons.Count - 1)
        {
            return;
        }
        
        _joycons[(int)whichHand].SetRumble(160, 320, 0.1f, 50);
    }

    public void RestKeyState(int player, bool state)
    {
        isUp[player] = state;
    }
    
    void OnGUI()
    {
        var infoText = "";
        infoText += _joycons.Count < 2 ? "Right Joycon disconnected" : "";
        GUI.color = Color.red;
        GUI.Label(new Rect(10, 10, 500, 20), infoText);
    }
}
