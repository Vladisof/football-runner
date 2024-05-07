using UnityEngine;

public class DestroyOnCollision : MonoBehaviour
{
  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Bullet"))
    {
      Destroy(gameObject);
    }
  }
}