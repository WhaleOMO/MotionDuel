using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;


public class CubeHover : MonoBehaviour
{
    private GameObject[] hands;
    private Collider collider;
    private MeshRenderer mRenderer;
    private PipeServer pipeServer;

    private bool[] isHovering;
    [HideInInspector]public int playerIndex;

    private Color originColor;
    
    public event Action<GameObject, HandEnum> OnHoverEnter;
    public event Action<GameObject, HandEnum> OnHoverStay;
    public event Action<GameObject, HandEnum> OnHoverExit;
    
    
    private void Start()
    {
        pipeServer = GameObject.FindObjectOfType(typeof(PipeServer)).GetComponent<PipeServer>();
        if (pipeServer == null)
        {
            Debug.LogWarning("Pipe server not found");
        }

        hands = new GameObject[2];
        isHovering = new bool[2];

        mRenderer = GetComponent<MeshRenderer>();
        collider = GetComponent<Collider>();
        originColor = mRenderer.material.GetColor("_Color");
    }

    private void Update()
    {
        var body = playerIndex == 1 ? pipeServer.body2 : pipeServer.body1;
        hands[0] = body.instances[15];
        hands[1] = body.instances[16];

        UpdateHoverState(playerIndex == 1 ? HandEnum.P1Left : HandEnum.P2Left);
        UpdateHoverState(playerIndex == 1 ? HandEnum.P1Right : HandEnum.P2Right);
    }

    private void UpdateHoverState(HandEnum whichHand)
    {
        int handIdx = playerIndex == 1 ? (int)whichHand : (int)whichHand - 2;
        var hand = hands[handIdx];
        Ray ray = new Ray(hand.transform.position, Vector3.forward);
        
        if (collider.Raycast(ray, out var hit, 200))
        {
            if (!isHovering[handIdx])
            {
                // mRenderer.material.color = Color.cyan; // for debug
                isHovering[handIdx] = true;
                OnHoverEnter?.Invoke(this.gameObject, whichHand);
                mRenderer.material.SetColor("_Color", originColor * 5f);
                return;
            }
            
            OnHoverStay?.Invoke(this.gameObject, whichHand);
        }
        else
        {
            if (isHovering[handIdx])
            {
                // mRenderer.material.color = Color.white;  
                isHovering[handIdx] = false;
                mRenderer.material.SetColor("_Color", originColor);
                // On Hover Exit
                OnHoverExit?.Invoke(this.gameObject, whichHand);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        Ray lRay = new Ray(hands[0].transform.position, Vector3.forward);
        Ray rRay = new Ray(hands[1].transform.position, Vector3.forward);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(lRay);
        Gizmos.DrawRay(rRay);
    }
}
