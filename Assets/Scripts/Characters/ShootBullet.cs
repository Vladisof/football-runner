using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShootBullet : MonoBehaviour
{
    public GameObject bulletPrefab;    // Префаб пули
    public Transform bulletSpawnPoint; // Точка, откуда вылетает пуля
    public float bulletSpeed = 10f;    // Скорость пули
    public Button shootButton;         // Кнопка, по нажатию на которую пуля вылетает

    private void Start()
    {
        shootButton.onClick.AddListener(Shoot);
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation); // Создаем пулю из префаба
        Rigidbody rb = bullet.GetComponent<Rigidbody>();                                                 // Получаем Rigidbody2D пули

        if (rb != null) // Проверяем, что Rigidbody2D существует
        {
            rb.velocity = bulletSpawnPoint.forward * bulletSpeed; // Задаем скорость пули в направлении, указанном bulletSpawnPoint.up, с заданной скоростью
        }
    }
}
