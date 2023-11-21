using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CubeHover : MonoBehaviour
{
    private GameObject lHand, rHand;
    private Collider collider;
    private MeshRenderer mRenderer;

    private PipeServer pipeServer;
    private void Start()
    {
        pipeServer = GameObject.FindObjectOfType(typeof(PipeServer)).GetComponent<PipeServer>();
        if (pipeServer == null)
        {
            Debug.LogWarning("Pipe server not found");
        }
        // lHand = pipeServer.body.instances[15];
        // rHand = pipeServer.body.instances[16];

        mRenderer = GetComponent<MeshRenderer>();
        collider = GetComponent<Collider>();
    }

    private void Update()
    {
        lHand = pipeServer.body.instances[15];
        rHand = pipeServer.body.instances[16];
        Ray lRay = new Ray(lHand.transform.position, Vector3.forward);
        Ray rRay = new Ray(rHand.transform.position, Vector3.forward);
        RaycastHit lHit, rHit;
        
        if (collider.Raycast(lRay, out lHit, 200) || collider.Raycast(rRay, out rHit, 200))
        {
            mRenderer.material.color = Color.cyan;
        }
        else
        {
            mRenderer.material.color = Color.white;
        }
    }

    private void OnDrawGizmos()
    {
        if (lHand == null)
        {
            return;
        }
        Ray lRay = new Ray(lHand.transform.position, Vector3.forward);
        Ray rRay = new Ray(rHand.transform.position, Vector3.forward);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(lRay);
        Gizmos.DrawRay(rRay);
    }
}
