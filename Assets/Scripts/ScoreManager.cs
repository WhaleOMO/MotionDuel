using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    // Start is called before the first frame update

    private int P1score = 0;
    private int P2score = 0;

    public TMP_Text ScoreText1;
    public TMP_Text ScoreText2;

    // Update is called once per frame
    void Update()
    {
        if (BlockManager.scrFlag1)
        {
            P1score += 1;
            ScoreText1.text = P1score.ToString();
            BlockManager.scrFlag1 = false;
        } 
        else if (BlockManager.scrFlag2)
        {
            P2score += 1;
            ScoreText2.text = P2score.ToString();
            BlockManager.scrFlag2 = false;
        }
    }
}
