using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    public GameObject[] blockPrefabs; // �����Ԥ��������
    public int rows = 5; // ����
    public int columns = 5; // ����
    public List<GameObject> blocks;
    public float moveSpeed = 1f;
    public float moveDistance = 1f;

    private List<string> blockTags; // ���ڼ�¼ÿ��λ�õķ����tag
    private GameObject _blockToDelete;
    private List<GameObject> _blocksToFall;
    private bool _clicked = false;

    private string lastBlockTag; // ����׷����һ�������tag

    private void Start()
    {
        blockTags = new List<string>();
        _blocksToFall = new List<GameObject>();
        GenerateBlocks();
    }

    void GenerateBlocks()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 spawnPosition = new Vector3(col-2f, row, 2); // ������������λ��
                GameObject randomBlockPrefab = GetRandomBlockPrefab();
                GameObject block = Instantiate(randomBlockPrefab, spawnPosition, Quaternion.identity);
                blocks.Add(block);
                blockTags.Add(block.tag);
                lastBlockTag = block.tag;
            }
        }
    }

    GameObject GetRandomBlockPrefab()
    {
        GameObject randomBlockPrefab = blockPrefabs[Random.Range(0, blockPrefabs.Length)];

        // ȷ����һ�������tag��ͬ����һ������
        while (randomBlockPrefab.tag == lastBlockTag)
        {
            randomBlockPrefab = blockPrefabs[Random.Range(0, blockPrefabs.Length)];
        }

        return randomBlockPrefab;
    }

    private void Update()
    {

        // �����ҵĵ��
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }


    void FallBlock(GameObject block)
    {
        // ��ȡ��һ��λ��
        Vector3 nextPosition = block.transform.position + Vector3.down;

        // �ƶ����鵽��һ��λ��
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
            AddBlocksToFall(aboveBlock);// ����Ϸ�����ײ�壬�����Ϸ��ķ���
        }
        else
        {
            lastBlockTag = block.tag;
            float xPosition = block.transform.position.x;
            float yPosition = block.transform.position.y;
            Vector3 spawnPosition = new Vector3(xPosition-2, yPosition+1, 2);
            GameObject randomBlockPrefab = GetRandomBlockPrefab();
            GameObject newBlock = Instantiate(randomBlockPrefab, spawnPosition, Quaternion.identity);
            _blocksToFall.Add(newBlock);
        }

    }

    public void StartFalling(float moveDistance = 1f)
    {
        StartCoroutine(MoveObjectsSmoothCoroutine());
    }

    // Э�̣���֡�ƶ�����
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
                obj.transform.position = Vector3.Lerp(initialPosition, initialPosition + Vector3.down * moveDistance, elapsedTime);
                yield return null;
            }

            elapsedTime = 0f;
        }

        // �ƶ�����������б�
        _blocksToFall.Clear();
    }
}
