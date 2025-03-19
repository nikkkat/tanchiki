using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TankBotController : NetworkBehaviour
{
    public float moveSpeed = 3f;
    public float detectionRadius = 5f;
    private Transform player;
    private Rigidbody2D rb;
    private Vector2 currentDirection;
    private Vector2 targetDirection;
    private bool isChasingPlayer = false;
    
    public LayerMask Obstacles;
    
    public GameObject bulletPrefab; // Ссылка на префаб пули
    public float fireRate = 0.5f; // Скорость стрельбы
    public float fireTime = 2f; // Время между выстрелами
    private float nextFireTime = 2f; // Время следующего выстрела

    public int maxHealth = 1;
    private int currentHealth;


    [SyncVar] private Vector2 syncedVelocity;
    [SyncVar] private Vector3 syncedPosition;
    [SyncVar] private Quaternion syncedRotation;

    public Transform bulletSpawnPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        //isStopping = true; // Установить isStopping в true при старте
        //ChooseNewDirection();
        if (isServer) // Только сервер управляет движением
        {
            StartCoroutine(ChangeDirection());
        }
    }
    IEnumerator ChangeDirection()
    {
        while (true)
        {
            currentDirection = Random.insideUnitCircle.normalized;
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }
    void Update()
    {
        if (isServer)
        {
            if (isChasingPlayer && player != null)
            {
                ChasePlayer();
            }
            else
            {
                RandomMove();
            }

            if (Time.time >= nextFireTime && player != null)
            {
                Shoot(); // Стреляем
                nextFireTime = Time.time + fireTime; // Обновляем время следующего выстрела
            }

            syncedVelocity = rb.velocity;
            syncedPosition = transform.position;
            syncedRotation = transform.rotation;
        }

        // Синхронизируем позицию и скорость
        if (isClient)
        {
            rb.velocity = syncedVelocity;
            transform.position = syncedPosition;
            transform.rotation = syncedRotation;
        }
    }


    [Server]
    void Shoot()
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            bullet.transform.up = directionToPlayer;
            Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
            bulletRB.velocity = directionToPlayer * fireRate;
            NetworkServer.Spawn(bullet);
        }
        else
        {
            Debug.LogError("Bullet prefab is not assigned!");
        }
    }


    [Server]
    void ChasePlayer()
    {
        if (player == null)
            return;

        // Вычисляем направление к игроку
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        // Проверяем расстояние до игрока
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            rb.velocity = Vector2.zero; // Останавливаем движение танка
           
            transform.up = directionToPlayer;
            
            Debug.Log("Player detected, shooting!"); // Добавим отладочное сообщение
            
        }
        else
        {
            // Продолжаем движение к игроку
            rb.velocity = directionToPlayer * moveSpeed;
            
        }
    }

    [Server]
    void RandomMove()
    {
        rb.velocity = currentDirection * moveSpeed;
        
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isServer && other.CompareTag("Player"))
        {
            player = other.transform;
            isChasingPlayer = true;
            Debug.Log("Player entered detection radius.");
            
        }
        if (isServer && other.CompareTag("PlayerBullet")) // Проверяем, является ли объект пулей игрока
        {
            TakeDamage(); // Вызываем функцию получения урона
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (isServer && other.CompareTag("Player"))
        {
            player = null;
            isChasingPlayer = false;
            
            Debug.Log("Player exited detection radius.");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isServer)
        {
            if (((1 << collision.gameObject.layer) & Obstacles) != 0)
            {
                ChooseNewDirection();
            }            
        }
    }

    void TakeDamage()
    {
        currentHealth--; // Уменьшаем здоровье на 1

        if (currentHealth <= 0)
        {
            NetworkServer.Destroy(gameObject); // Уничтожаем бота, если его здоровье меньше или равно нулю
        }
    }

    [Server]
    void ChooseNewDirection()
    {
        currentDirection = Random.insideUnitCircle.normalized;
    }
    
    void OnDrawGizmos()
    {         
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
                   
    }
}
