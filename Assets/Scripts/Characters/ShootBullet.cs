using UnityEngine;
using UnityEngine.UI;
namespace Characters
{
    public class ShootBullet : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public Transform bulletSpawnPoint;
        public float bulletSpeed = 10f;
        public Button shootButton;

        private void Start()
        {
            shootButton.onClick.AddListener(Shoot);
        }

        void Shoot()
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.velocity = bulletSpawnPoint.forward * bulletSpeed;
            }
        }
    }
}
