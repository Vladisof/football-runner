using UnityEngine;
namespace Characters
{
  public class DestroyAfterDelay : MonoBehaviour
  {
    public float delay = 15f;

    void Start()
    {
      Invoke("DestroyObject", delay);
    }

    void DestroyObject()
    {
      Destroy(gameObject);
    }
  }
}