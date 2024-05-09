using UnityEngine;
using System.Collections;

public class HandlerCoroutineHandler : MonoBehaviour
{
    private static HandlerCoroutineHandler _mInstance;
    private static HandlerCoroutineHandler instance
    {
        get
        {
            if (_mInstance != null)
            {
                return _mInstance;
            }

            GameObject o = new GameObject("CoroutineHandler");
            DontDestroyOnLoad(o);
            _mInstance = o.AddComponent<HandlerCoroutineHandler>();

            return _mInstance;
        }
    }

    public void OnDisable()
    {
        if(_mInstance)
            Destroy(_mInstance.gameObject);
    }

    public static void StartStaticCoroutine (IEnumerator coroutine)
    {
        instance.StartCoroutine(coroutine);
    }
}
