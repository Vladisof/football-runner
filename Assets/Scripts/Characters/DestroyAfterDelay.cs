using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
  public float delay = 15f; // Задержка перед уничтожением в секундах

  void Start()
  {
    // Вызываем метод DestroyObject с задержкой
    Invoke("DestroyObject", delay);
  }

  void DestroyObject()
  {
    // Уничтожаем текущий объект
    Destroy(gameObject);
  }
}