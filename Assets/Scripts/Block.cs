using UnityEngine;

public class Block : MonoBehaviour
{
    public float fallSpeed = 5f; // œ¬¬‰ÀŸ∂»
    private int _positionindex;
    private int _playerIndex;//player1 with index 1, and player2 with index2
    private int _additionalScore = 0;//
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
            FindObjectOfType<ScoreManager>().UpdateScore();
        }
    }



}