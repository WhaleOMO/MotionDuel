using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    // Start is called before the first frame update

    public static int P1score = 0;
    public static int P2score = 0;

    public TMP_Text ScoreText1;
    public TMP_Text ScoreText2;

    public static int p1ScoreRate = 1;
    public static int p2ScoreRate = 1;

    // Update is called once per frame
    void Update()
    {
        if (BlockManager.scrFlag1)
        {
            P1score += p1ScoreRate;
            ScoreText1.text = P1score.ToString();
            BlockManager.scrFlag1 = false;
        }
        else if (BlockManager.scrFlag2)
        {
            P2score += p2ScoreRate;
            ScoreText2.text = P2score.ToString();
            BlockManager.scrFlag2 = false;
        }
    }

    public static int WhichPlayerWins()
    {
        return P1score == P2score ? 0 :
               P1score > P2score ? 1 : 2;
    }

    public void UpdateScore(){
        ScoreText1.text = P1score.ToString();
        ScoreText2.text = P2score.ToString();
    }


}
