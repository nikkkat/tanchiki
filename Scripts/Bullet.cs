using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public float speed = 10f; // Скорость пули
    public float lifeTime = 2f; // Время жизни пули

    private void Start()
    {
        if (isServer)
        {
            // Уничтожить пулю через заданное время
            Destroy(gameObject, lifeTime);
        }
    }

    private void Update()
    {
        // Движение пули вперед
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isServer)
        {
            if (collision.CompareTag("Player") || collision.CompareTag("Enemy") || collision.CompareTag("Obstacle"))
            {
                Destroy(gameObject); // Уничтожить пулю при столкновении
            }
        }
    }

}
