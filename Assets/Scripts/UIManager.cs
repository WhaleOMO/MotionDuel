using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public BlockManager blockManager;
    public Sprite[] imageList;
    public Image sonImage;
    public int playerIndex;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        string blockName = blockManager.GetSkillName(playerIndex);
        if (blockName == "Blue")
        {
            sonImage.sprite = imageList[0];
        }
        else if (blockName == "Red")
        {
            sonImage.sprite = imageList[1];
        }
        else if (blockName == "Yellow")
        {
            sonImage.sprite = imageList[2];
        }
        else if (blockName == "Green")
        {
            sonImage.sprite = imageList[3];
        }
        else if (blockName == "Black")
        {
            sonImage.sprite = imageList[4];
        }
        else if (blockName == "White")
        {
            sonImage.sprite = imageList[5];
        }
  
    }
}
