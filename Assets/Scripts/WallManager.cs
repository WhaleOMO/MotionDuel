using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    public Material wallMaterial;
    
    void OnEnable()
    {
        StartCoroutine(DeactivateObject(3f));
        wallMaterial.SetFloat(Shader.PropertyToID("_SplashAmount"),0.0f);
        wallMaterial.DOFloat(1.0f, Shader.PropertyToID("_SplashAmount"), 0.5f);
    }

    IEnumerator DeactivateObject(float delay)
    {
        // wait for some time to deactive
        yield return new WaitForSeconds(delay);
        //wallMaterial.SetFloat(Shader.PropertyToID("_SplashAmount"),0.0f);
        gameObject.SetActive(false);
    }
}
