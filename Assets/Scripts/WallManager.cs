using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DeactivateObject(3f));
    }

    IEnumerator DeactivateObject(float delay)
    {
        // wait for some time to deactive
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
