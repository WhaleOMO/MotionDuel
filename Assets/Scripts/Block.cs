using UnityEngine;

public class Block : MonoBehaviour
{
    public float fallSpeed = 5f; // œ¬¬‰ÀŸ∂»
    private int _positionindex;
    private int _playerIndex;//player1 with index 1, and player2 with index2
    private int _additionalScore = 0;//
    private bool _isFrozen = false;
    private float _frozenTimer = 0;
    private bool _isWildCard = false;
    
    public ScoreManager scoremanager;

    public int GetIndex()
    {
        return _positionindex;
    }

    public int GetPlayer()
    {
        return _playerIndex;
    }
    public bool GetFrozenStatus()
    {
        return _isFrozen;
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
    public void SetBlockFrozen()
    {
        _isFrozen = true;
        _frozenTimer = 4.0f;
    }
    public void SetWildCard()
    {
        _isWildCard = true;
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
    public void Update()
    {
        if (_isFrozen && _frozenTimer > 0)
        {
            _frozenTimer -= Time.deltaTime;
            if (_frozenTimer <= 0f)
            {
                _isFrozen = false;
                _frozenTimer = 4.0f;
            }
        }
    }



}