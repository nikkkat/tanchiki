using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public float speed = 10f; // �������� ����
    public float lifeTime = 2f; // ����� ����� ����

    private void Start()
    {
        if (isServer)
        {
            // ���������� ���� ����� �������� �����
            Destroy(gameObject, lifeTime);
        }
    }

    private void Update()
    {
        // �������� ���� ������
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isServer)
        {
            if (collision.CompareTag("Player") || collision.CompareTag("Enemy") || collision.CompareTag("Obstacle"))
            {
                Destroy(gameObject); // ���������� ���� ��� ������������
            }
        }
    }

}
