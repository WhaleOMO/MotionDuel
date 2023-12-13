using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

public class BlockManager : MonoBehaviour
{
    public bool noJoyconMode;
    public GameObject[] blockPrefabs; // ??????????????
    public int rows = 5; // ????
    public int columns = 4; // ????
    public List<GameObject> blocks;
    public float moveSpeed = 1f;
    //public float moveDistance = 2f;
    public float gapScale = 1f;
    public float xOffset = 1f;
    public int numPlayers = 2;
    public float playerGap = 13f;
    public float yOffset;

    public static bool scrFlag1;
    public static bool scrFlag2;

    public GameObject eraseVFX;
    
    private List<string> blockTags; // ?????????λ???????tag
    private GameObject _blockToDelete;

    private bool _player1PushedButtonToReleaseSkill = true;
    private bool _player2PushedButtonToReleaseSkill = true;

    private List<GameObject> _blocksToFall;
    private List<GameObject> _blocksToFallBuffer;
    private List<GameObject> _blocksBin; //Finally We end up with having a garbage bin
    private string[] _lastElimination;
    private Queue<string>[] _playerSkills;
    //private Queue<string>[] lastTwoErase; //Stores the last two elements the player has elimated


    private bool _isFalling = false;

    private bool _clicked = false;
    
    private string lastBlockTag; // ?????????????????tag

    private GameObject[] hoveredElements;

    private string skillCache = "Blue";
    
    private void Start()
    {
        blockTags = new List<string>();
        _blocksToFall = new List<GameObject>();
        _blocksToFallBuffer = new List<GameObject>();
        _blocksBin = new List<GameObject>();
        hoveredElements = new GameObject[4];
        _lastElimination = new string[2];
        _lastElimination[0] = "";
        _lastElimination[1] = "";
        _playerSkills = new Queue<string>[2];
        _playerSkills[0] = new Queue<string>();
        _playerSkills[1] = new Queue<string>();

        /*** This part of the code is for skill released by 3 continues elimenation,
        I changed it into 2 because its too hard to get 3 in a sequence.
        lastTwoErase = new Queue<string>[2];
        Queue<string> player1Init = new Queue<string>();
        player1Init.Enqueue("Empty");
        player1Init.Enqueue("Empty");
        Queue<string> player2Init = new Queue<string>();
        player2Init.Enqueue("Empty");
        player2Init.Enqueue("Empty");
        lastTwoErase[0] = player1Init;
        lastTwoErase[1] = player2Init;
        ***/
        GenerateBlocks();
    }

    void GenerateBlocks()
    {
        int positionIndex = 0;
        for (int iter = 1; iter < numPlayers+1; iter++)
        {
            
            for (float row = 0; row < rows*gapScale; row+=gapScale)
            {
                for (float col = (iter-1) * playerGap; col < columns*gapScale + (iter-1) * playerGap; col+=gapScale)
                {
                    Vector3 spawnPosition = new Vector3(col-xOffset, row-yOffset, 2); // ????????????λ??
                    GameObject randomBlockPrefab = GetRandomBlockPrefab();
                    GameObject block = Instantiate(randomBlockPrefab, spawnPosition, Quaternion.identity);
                    if (block.TryGetComponent(out CubeHover cHover))
                    {
                        // Listen to hover event on the element
                        cHover.OnHoverEnter += HandelMotionHover;
                        cHover.OnHoverEnter += JoyconController.instance.OnHandHoverEnter;
                        cHover.playerIndex = iter;
                        // cHover.OnHoverExit += HandelMotionExit;
                    }
                    block.GetComponent<Block>().SetIndex(positionIndex);
                    block.GetComponent<Block>().SetPlayer(iter);
                    positionIndex++;
                    blocks.Add(block);
                    blockTags.Add(block.tag);
                    lastBlockTag = block.tag;
                  
                     
                }
            }           
        }

    }

    GameObject GetRandomBlockPrefab()
    {
        GameObject randomBlockPrefab = blockPrefabs[Random.Range(0, blockPrefabs.Length)];

        // ?????????????tag??????????????
        while (randomBlockPrefab.tag == lastBlockTag)
        {
            randomBlockPrefab = blockPrefabs[Random.Range(0, blockPrefabs.Length)];
        }

        return randomBlockPrefab;
    }

    private void Update()
    {
        // ?????????
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }

        var joyconCon = JoyconController.instance;
        if (joyconCon.IsJoyconShoulderDown(HandEnum.P1Left))
        {
            Debug.Log("P1 Erase!");
            if (Erase(0))
            {
                joyconCon.RestKeyState(0, false);
                scrFlag1 = true;
            }
        }
        
        if (joyconCon.IsJoyconShoulderDown(HandEnum.P1Right))
        {
            Debug.Log("P2 Erase!");
            if (Erase(2))
            {
                joyconCon.RestKeyState(1, false);
                scrFlag2 = true;
            }
        }

        if (!_isFalling)
        {
            if (_blocksBin.Count != 0)
            {
                foreach (GameObject trashBlock in _blocksBin)
                {
                    Destroy(trashBlock);
                }
                _blocksBin.Clear();
/***
                foreach (GameObject obj in blocks)
                {
                    Debug.Log(obj.GetComponent<Block>().GetIndex());
                }
***/
            }


            //Change _playerPushedButtonToReleaseSkill to true to use skills


            if (_player1PushedButtonToReleaseSkill && _playerSkills[0].Count != 0)
            {
                string skill = _playerSkills[0].Dequeue();
                UsingSkill(skill, 0);

                //Debug.Log("player1 has released skill" + _playerSkills[0].Dequeue());
            }

            if (_player2PushedButtonToReleaseSkill && _playerSkills[1].Count != 0)
            {
                string skill = _playerSkills[1].Dequeue();
                UsingSkill(skill, 1);
                //Debug.Log("player2 has released skill" + _playerSkills[1].Dequeue());
            }
        }






    }

    private bool Erase(int offset)
    {
        if (hoveredElements[0 + offset] == null || hoveredElements[1 + offset] == null)
        {
            return false;
        }
        
        if (hoveredElements[0 + offset].CompareTag(hoveredElements[1 + offset].tag))
        {
            AddBlocksToFall(hoveredElements[0 + offset]);
            AddBlocksToFall(hoveredElements[1 + offset]);
            if(_blocksToFall.Contains(hoveredElements[0 + offset]) || _blocksToFall.Contains(hoveredElements[1 + offset]))
            {
                _blocksToFall.Add(_blocksToFall[_blocksToFall.Count - 1]);
            }

            if (_blocksToFall.Count != 0) StartFalling();
            //Debug.Log(_blocksToFall.Count);

            Instantiate(eraseVFX, hoveredElements[0 + offset].transform.position, Quaternion.identity);
            Instantiate(eraseVFX, hoveredElements[1 + offset].transform.position, Quaternion.identity);
            
            hoveredElements[0 + offset].SetActive(false);
            hoveredElements[1 + offset].SetActive(false);
            hoveredElements[0 + offset] = null;
            hoveredElements[1 + offset] = null;
            SoundManager.instance.PlayBrokenSound();
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Will be triggered when a hand hovers on an element
    /// </summary>
    /// <param name="target">the game object that are hovered</param>
    /// <param name="hand">indicates which hand, see the enum</param>
    private void HandelMotionHover(GameObject target, HandEnum hand)
    {
        hoveredElements[(int)hand] = target;
    }

    /// <summary>
    /// Will be triggered when a hand no longer hovers on an element
    /// </summary>
    /// <param name="target">the game object that are hovered</param>
    /// <param name="hand">indicates which hand, see the enum</param>
    private void HandelMotionExit(GameObject target, HandEnum hand)
    {
        hoveredElements[(int)hand] = null;
    }

    void FallBlock(GameObject block)
    {
        // ????????λ??
        Vector3 nextPosition = block.transform.position + Vector3.down;

        // ??????鵽?????λ??
        block.transform.position = nextPosition;
    }

    void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject clickedObject = hit.collider.gameObject;
            if(!_clicked)
            {
                _clicked = true;
                _blockToDelete = clickedObject;
            }
            else if(_blockToDelete != clickedObject)
            {
                _clicked = false;
                if(_blockToDelete.tag == clickedObject.tag)
                {


                    //write on the buffer list if the coroutine is working

                    if (!_isFalling)
                    {
                        //if the two blocks are in the same column
                        AddBlocksToFall(_blockToDelete);
                        AddBlocksToFall(clickedObject);                                                 
                        if(_blocksToFall.Contains(_blockToDelete) || _blocksToFall.Contains(clickedObject))
                        {
                            _blocksToFall.Add(_blocksToFall[_blocksToFall.Count - 1]);
                        }

                        if (_blocksToFall.Count != 0) StartFalling();
                        //Debug.Log(_blocksToFall.Count);
                        int playerIndex = _blockToDelete.GetComponent<Block>().GetPlayer()-1;
                        string lastElement = _lastElimination[playerIndex];
                        if(lastElement == _blockToDelete.tag)
                        {
                            _playerSkills[playerIndex].Enqueue(lastElement);
                            _lastElimination[playerIndex] = "";
                        }
                        else
                        {
                            _lastElimination[playerIndex] = _blockToDelete.tag;
                        }


                        /***
                        string lastElement = lastTwoErase[playerIndex].Peek();
                        string lastlastElement = lastTwoErase[playerIndex].Dequeue();
                        lastTwoErase[playerIndex].Enqueue(_blockToDelete.tag);
                        Debug.Log(lastElement + "and" + _blockToDelete.tag);
                        if(lastElement        == _blockToDelete.tag & 
                           _blockToDelete.tag == lastTwoErase[playerIndex].Dequeue())
                        {
                            Debug.Log("It's about time to have a skill release");
                        }
                        ***/
                        _blockToDelete.SetActive(false);
                        clickedObject.SetActive(false);
                        _blocksBin.Add(_blockToDelete);
                        _blocksBin.Add(clickedObject);


                        //Destroy(clickedObject);
                        //Destroy(_blockToDelete);
                    }

                }
            }

            // Log the name of the clicked object
            Debug.Log("Clicked on object: " + clickedObject.GetComponent<Block>().GetIndex());

        }
        else
        {
            //Debug.Log("No object hit by the raycast");
        }
    }

    public void AddBlocksToFall(GameObject block)
    { 
        RaycastHit hit;
        if (Physics.Raycast(block.transform.position, Vector3.up, out hit))
        {
            GameObject aboveBlock = hit.collider.gameObject;
            //write to the buffer list if the coroutine is working
            if (_isFalling)
            {
                _blocksToFallBuffer.Add(aboveBlock);
            }
            else
            {
                _blocksToFall.Add(aboveBlock);
            }
            
            AddBlocksToFall(aboveBlock);// 
        }
        else
        {
            lastBlockTag = block.tag;
            float xPosition = block.transform.position.x;
            float yPosition = block.transform.position.y;
            Vector3 spawnPosition = new Vector3(xPosition, yPosition+gapScale, 2);
            GameObject randomBlockPrefab = GetRandomBlockPrefab();
            GameObject newBlock = Instantiate(randomBlockPrefab, spawnPosition, Quaternion.identity);
            newBlock.GetComponent<Block>().SetIndex(block.GetComponent<Block>().GetIndex()+columns);
            newBlock.GetComponent<Block>().SetPlayer(block.GetComponent<Block>().GetPlayer());
            if (newBlock.TryGetComponent(out CubeHover cHover))
            {
                // Listen to hover event on the element
                cHover.OnHoverEnter += HandelMotionHover;
                cHover.OnHoverEnter += JoyconController.instance.OnHandHoverEnter;
                cHover.playerIndex = block.GetComponent<CubeHover>().playerIndex;
                // cHover.OnHoverExit += HandelMotionExit;
            }
            if (_isFalling)
            {
                _blocksToFallBuffer.Add(newBlock);
            }
            else
            {
                _blocksToFall.Add(newBlock);
            }
        }

    }

    public void UsingSkill(string skillname, int player)
    {
        
        switch (skillname)
        {
            case "Red"://This is for Zeus
                break;
            case "Blue"://This is for Artemis
                        // Now this will bless on 3 blocks for the player
                for(int i=0; i < 3; i++)
                {
                    int randomIndex = Random.Range(0, 20) + player * rows * columns;
                    Debug.Log("The "+ i+ " Additional one is "+ randomIndex);
                    blocks[randomIndex].GetComponent<Block>().AddAdditionalScore(2);
                }
                break;
            case "White":// This is for Dionysus
                UsingSkill(skillCache, player);
                break;
            case "Green":// This is for Poseidon
                break;
            case "Black":// This is for Aris
                for (int i = 0; i < 3; i++)
                {
                    int randomIndex = Random.Range(0, 20) + player * rows * columns;
                    Debug.Log("The " + i + " WildCard one is " + randomIndex);
                    blocks[randomIndex].GetComponent<Block>().SetWildCard();
                }
                break;
            case "Yellow":// This is for Demeter
                for (int i = 0; i < 3; i++)
                {
                    int randomIndex = Random.Range(0, 20) + player * rows * columns;
                    Debug.Log("The " + i + " Frozen one is " + randomIndex);
                    blocks[randomIndex].GetComponent<Block>().SetBlockFrozen();
                }
                break;
            default:
                Debug.Log("Unknown Skill, try to change the tag of the prefabs to the correct skill name");
                break;
        }
        if (skillname != "White")
            skillCache = skillname;
    }


    public void StartFalling(float moveDistance = 2f)
    {
        StartCoroutine(MoveObjectsSmoothCoroutine());
    }

    // Э?????????????
    IEnumerator MoveObjectsSmoothCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 initialPosition;

        IEnumerable<GameObject> mergedEnumerable = _blocksToFall.Concat(_blocksToFallBuffer);
        GameObject[] allBlocksToFall = mergedEnumerable.ToArray();
        // Reset the blocks to fall
        _blocksToFall.Clear();
        _blocksToFallBuffer.Clear();

        while (_isFalling)
        {
            yield return null;
        }

        _isFalling = true;
        //Debug.Log(allBlocksToFall.Length);
        int totalBlocks = rows * columns;
        foreach (GameObject obj in allBlocksToFall)
        {
            initialPosition = obj.transform.position;
            int newIndex = obj.GetComponent<Block>().GetIndex() - columns;
            int playerIndex = obj.GetComponent<Block>().GetPlayer();
            obj.GetComponent<Block>().SetIndex(newIndex);
            if (newIndex >= (playerIndex-1) * totalBlocks  && newIndex< playerIndex * totalBlocks)
            {
                blocks[newIndex] = obj;
            }

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * moveSpeed;
                obj.transform.position = Vector3.Lerp(initialPosition, initialPosition + Vector3.down * gapScale, elapsedTime);
                yield return null;
            }

            elapsedTime = 0f;
        }

        _isFalling = false;

    }
}
