using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public Sprite[] imageList;
    public Sprite[] godsList;
    public BlockManager blockManager;
    public Image sonImage;
    public GameObject illusImage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        string skillName = blockManager.GetLastElimation(0);
        if (skillName == "Blue")
        {
            StartCoroutine(animationPlayer());
            sonImage.sprite = imageList[3];
        }
        else if (skillName == "Red")
        {
            sonImage.sprite = imageList[5];
        }
        else if (skillName == "White")
        {
            sonImage.sprite = imageList[4];
        }
        else if (skillName == "Yellow")
        {
            sonImage.sprite = imageList[2];
        }
        else if (skillName == "Black")
        {
            sonImage.sprite = imageList[1];
        }
        else if (skillName == "Green")
        {
            sonImage.sprite = imageList[0];
        }

    }
    IEnumerator animationPlayer()
    {
        // illusImage.GetComponent<Image>().sprite = 
        illusImage.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        illusImage.SetActive(false);
    }
}
