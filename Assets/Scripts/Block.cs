using UnityEngine;

public class Block : MonoBehaviour
{
    public float fallSpeed = 5f; // �����ٶ�

    void Update()
    {
        CheckFall();
    }

    void CheckFall()
    {
        // ���·�������
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            // ����·�û����ײ�壬˵����Ҫ����
            if (hit.collider == null)
            {
                StartFalling();
            }
        }
    }

    public void StartFalling()
    {
        // �����߼�
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
    }
}