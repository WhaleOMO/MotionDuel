using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MouseHover : MonoBehaviour
{
    
    private Vector3 rotationVector = new Vector3(0f, 0f, 360f);
    private Vector3 orginScale;
    private Quaternion orginRotation;

    private void Start()
    {
        orginScale = this.gameObject.transform.localScale;
        orginRotation = this.gameObject.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
        {
            this.gameObject.transform.DOScale(orginScale * 1.5f, 0.5f);
            this.gameObject.transform.DORotate(rotationVector, 8f, RotateMode.WorldAxisAdd).SetLoops(-1).SetEase(Ease.Linear);
        }
        else
        {
            if (this.gameObject.transform.rotation != orginRotation)
            {
                this.gameObject.transform.DORotateQuaternion(orginRotation, 1f);
            }
            this.gameObject.transform.DOScale(orginScale, 0.5f);
        }
        
    }
}
