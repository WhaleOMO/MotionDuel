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
    public int columns = 5; // ????
    public List<GameObject> blocks;
    public float moveSpeed = 1f;
    public float swapSpeed = 1f;
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
    private List<GameObject> _blocksToFall;
    private bool _clicked = false;
    
    private string lastBlockTag; // ?????????????????tag

    private GameObject[] hoveredElements;
    
    private void Start()
    {
        blockTags = new List<string>();
        _blocksToFall = new List<GameObject>();
        hoveredElements = new GameObject[4];
        GenerateBlocks();
    }

    void GenerateBlocks()
    {
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
                    AddBlocksToFall(_blockToDelete);
                    AddBlocksToFall(clickedObject);
                    if(_blocksToFall.Contains(_blockToDelete) || _blocksToFall.Contains(clickedObject))
                    {
                        _blocksToFall.Add(_blocksToFall[_blocksToFall.Count - 1]);
                    }

                    if (_blocksToFall.Count != 0) StartFalling();
                    //Debug.Log(_blocksToFall.Count);

                    _blockToDelete.SetActive(false);
                    clickedObject.SetActive(false);


                    //Destroy(clickedObject);
                    //Destroy(_blockToDelete);
                }
            }

            // Log the name of the clicked object
            Debug.Log("Clicked on object: " + clickedObject.name);

        }
        else
        {
            Debug.Log("No object hit by the raycast");
        }
    }

    public void AddBlocksToFall(GameObject block)
    { 
        RaycastHit hit;
        if (Physics.Raycast(block.transform.position, Vector3.up, out hit))
        {
            GameObject aboveBlock = hit.collider.gameObject;
            _blocksToFall.Add(aboveBlock);
            AddBlocksToFall(aboveBlock);// ????????????壬????????????
        }
        else
        {
            lastBlockTag = block.tag;
            float xPosition = block.transform.position.x;
            float yPosition = block.transform.position.y;
            Vector3 spawnPosition = new Vector3(xPosition, yPosition+gapScale, 2);
            GameObject randomBlockPrefab = GetRandomBlockPrefab();
            GameObject newBlock = Instantiate(randomBlockPrefab, spawnPosition, Quaternion.identity);
            if (newBlock.TryGetComponent(out CubeHover cHover))
            {
                // Listen to hover event on the element
                cHover.OnHoverEnter += HandelMotionHover;
                cHover.OnHoverEnter += JoyconController.instance.OnHandHoverEnter;
                cHover.playerIndex = block.GetComponent<CubeHover>().playerIndex;
                // cHover.OnHoverExit += HandelMotionExit;
            }
            _blocksToFall.Add(newBlock);
        }

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

        foreach (GameObject obj in _blocksToFall)
        {
            initialPosition = obj.transform.position;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * moveSpeed;
                obj.transform.position = Vector3.Lerp(initialPosition, initialPosition + Vector3.down * gapScale, elapsedTime);
                yield return null;
            }

            elapsedTime = 0f;
        }

        // ?????????????б?
        _blocksToFall.Clear();
    }
}
