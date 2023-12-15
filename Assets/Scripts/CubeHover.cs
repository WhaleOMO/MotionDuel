using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using DG.Tweening;

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
    public Camera overlayCamera;
    
    private Vector3 rotationVector = new Vector3(0f, 360f, 0f);
    private Vector3 orginScale;
    private Quaternion orginRotation;
    private Tween RotatDoTween;
    
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
        orginScale = this.gameObject.transform.localScale;
        orginRotation = this.gameObject.transform.rotation;
        Camera[] allCameras = FindObjectsOfType<Camera>();
        overlayCamera = allCameras[1];
    }

    private void Update()
    {   
        // Ray ray = overlayCamera.ScreenPointToRay(Input.mousePosition);
        // RaycastHit hit;
        // if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
        // {
        //     SelectEffect();
        // }
        // else
        // {
        //     if (this.gameObject.transform.rotation != orginRotation)
        //     {
        //         ReturnRotateEffect();
        //         RotatDoTween.Kill();                
        //     }
        //
        //     ReturnScaleEffect();
        // }

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
                // mRenderer.material.SetColor("_Color", originColor * 5f);
                SelectEffect();
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
                // mRenderer.material.SetColor("_Color", originColor);
                // On Hover Exit
                ReturnScaleEffect();
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
    
    void SelectEffect()
    {
        this.gameObject.transform.DOScale(orginScale * 1.5f, 0.5f);
        RotatDoTween = this.gameObject.transform.DORotate(rotationVector, 8f, RotateMode.WorldAxisAdd).SetLoops(-1).SetEase(Ease.Linear);
        
        
    }

    void ReturnRotateEffect()
    {
        this.gameObject.transform.DORotateQuaternion(orginRotation, 1f);
    }

    void ReturnScaleEffect()
    {
        this.gameObject.transform.DOScale(orginScale, 0.5f);
    }
}
