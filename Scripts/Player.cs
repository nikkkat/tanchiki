using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
//using UnityEditor.Tilemaps;

public class Player : NetworkBehaviour
{
    private Rigidbody2D rb;

    public float speed;
    public GameObject bulletPrefab; // Префаб пули
    public Transform firePoint; // Точка стрельбы
    public float fireRate = 0.5f; // Скорость стрельбы
    //public float bulletLifetime = 3f; // Время жизни пули
    private float nextFireTime; // Время для следующего выстрела

    private Vector2 input;
    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (!isLocalPlayer)
        {
            // Если это не локальный игрок, выходим из метода
            return;
        }

        nextFireTime = 0f;

    }

    // Update is called once per frame
    private void Update()
    {
        if (!isLocalPlayer) return;

        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Flip();

        if (Input.GetKeyDown(KeyCode.Space) && Time.time > nextFireTime)
        {
            CmdFire(firePoint.position, firePoint.rotation);
            //CmdFire();
            nextFireTime = Time.time + fireRate;
            Debug.Log("Space key pressed: " + Input.GetKeyDown(KeyCode.Space));
            Debug.Log("Time: " + Time.time);
            Debug.Log("Next fire time: " + nextFireTime);
        }
    }

    private void Flip()
    {

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (horizontalInput > 0)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 270); // Поворот вправо
        }
        else if (horizontalInput < 0)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 90); // Поворот влево
        }
        else if (verticalInput > 0)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0); // Поворот вверх
        }
        else if (verticalInput < 0)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 180); // Поворот вниз
        }

    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
    }

    [Command]
    private void CmdFire(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, rotation);
        bullet.tag = "PlayerBullet";
        NetworkServer.Spawn(bullet);
    }

}
