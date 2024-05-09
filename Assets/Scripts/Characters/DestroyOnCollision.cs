using UnityEngine;
namespace Characters
{
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
}