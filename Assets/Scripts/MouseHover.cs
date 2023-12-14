using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.VFX;
using UnityEngine.Rendering.HighDefinition.Attributes;

public class MouseHover : MonoBehaviour
{
    
    private Vector3 rotationVector = new Vector3(0f, 0f, 360f);
    private Vector3 orginScale;
    private Quaternion orginRotation;
    
    public Material Material_;

    public float DissolveTime = 20f;
    public float RefreshRate = 0.01f;
    public float ReturnTime = 1f;
    
    private float DissovleRate;
    private void Start()
    {
        orginScale = this.gameObject.transform.localScale;
        orginRotation = this.gameObject.transform.rotation;
        DissovleRate = 1 / DissolveTime;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
        {
            SelectEffect();
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(DissolveCo());

                Transform[] children = this.gameObject.GetComponentsInChildren<Transform>();
                
                foreach (Transform child in children)
                {
                    if (child != transform)
                    {
                        child.gameObject.SetActive(false);
                    }
                }
                //Instantiate(EraseVFX, this.gameObject.transform.position, Quaternion.identity);
            }
        }
        else
        {
            if (this.gameObject.transform.rotation != orginRotation)
            {
                ReturnRotateEffect();
            }

            ReturnScaleEffect();
        }
        
    }

    void SelectEffect()
    {
        this.gameObject.transform.DOScale(orginScale * 1.5f, 0.5f);
        this.gameObject.transform.DORotate(rotationVector, 8f, RotateMode.WorldAxisAdd).SetLoops(-1).SetEase(Ease.Linear);
    }

    void ReturnRotateEffect()
    {
        this.gameObject.transform.DORotateQuaternion(orginRotation, 1f);
    }

    void ReturnScaleEffect()
    {
        this.gameObject.transform.DOScale(orginScale, 0.5f);
    }

    IEnumerator DissolveCo()
    {
        float counter = 0;

        while (Material_.GetFloat("_DissolveAmount") < 1)
        {
            counter += DissovleRate;
            yield return new WaitForSeconds(RefreshRate);
            Material_.SetFloat("_DissolveAmount", counter);
        }
        yield return new WaitForSeconds(ReturnTime);
        Material_.SetFloat("_DissolveAmount", 0);
    }
    
}
