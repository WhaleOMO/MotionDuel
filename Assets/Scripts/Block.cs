using UnityEngine;
using System;
using System.Collections;

public class Block : MonoBehaviour
{
    public float fallSpeed = 5f; // 下落速度
    public GameObject frozenEffect;
    private int _positionindex;
    private int _playerIndex;//player1 with index 1, and player2 with index2
    private int _additionalScore = 0;//
    private bool _isJoker = false;
    private bool _isFrozen = false;
    public ScoreManager scoremanager;

    public int GetIndex()
    {
        return _positionindex;
    }

    public int GetPlayer()
    {
        return _playerIndex;
    }

    public void SetIndex(int index)
    {
        _positionindex = index;
    }

    public void SetPlayer(int index)
    {
        _playerIndex = index;
    }

    public void AddAdditionalScore(int additionalScore)
    {
        _additionalScore += additionalScore;
        //Add the effect for Atermis here
    }

    public void BecomeJoker()
    {
        _isJoker = true;
    }

    public void Froze()
    {
        _isFrozen = true;
        //Add the effet to frozen here
        StartCoroutine(Melting(5f));
    }

    public bool IsJoker()
    {
        return _isJoker;
    }

    public bool IsFrozen()
    {
        return _isFrozen;
    }

    private void OnDestroy()
    {
        if (_additionalScore != 0)
        {
            Debug.Log("this one is being elimatied");
            switch (_playerIndex)
            {
                case 1:
                    ScoreManager.P1score += _additionalScore;
                    break;
                case 2:
                    ScoreManager.P2score += _additionalScore;
                    break;
            }
            scoremanager.UpdateScore();
        }
    }

    IEnumerator Melting(float delay)
    {

        yield return new WaitForSeconds(delay);
        Transform child = this.GetComponent<Block>().transform.Find("Sphere");
        if (child != null)
        {
            GameObject childGameObject = child.gameObject;
            childGameObject.SetActive(false);
        }
        _isFrozen = false;
        //Remove the effect to defrozen here
    }
}