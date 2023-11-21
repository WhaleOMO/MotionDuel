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
    
    public event Action<GameObject, HandEnum> onHoverEnter;
    public event Action<GameObject, HandEnum> onHoverStay; 

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
    }

    private void Update()
    {
        hands[0] = pipeServer.body.instances[15];
        hands[1] = pipeServer.body.instances[16];

        UpdateHoverState(HandEnum.Left);
        UpdateHoverState(HandEnum.Right);
    }

    private void UpdateHoverState(HandEnum whichHand)
    {
        var hand = hands[(int)whichHand];
        Ray ray = new Ray(hand.transform.position, Vector3.forward);

        if (collider.Raycast(ray, out var hit, 200))
        {
            if (!isHovering[(int)whichHand])
            {
                // mRenderer.material.color = Color.cyan; // for debug
                isHovering[(int)whichHand] = true;
                onHoverEnter?.Invoke(gameObject, whichHand);
                mRenderer.material.color *= 5f;
                return;
            }
            
            onHoverStay?.Invoke(gameObject, whichHand);
        }
        else
        {
            if (isHovering[(int)whichHand])
            {
                // mRenderer.material.color = Color.white;  
                isHovering[(int)whichHand] = false;
                mRenderer.material.color /= 5f;
                // On Hover Exit
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
