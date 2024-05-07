using UnityEngine;

namespace AssetTheory
{
    public class CamFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        Vector3 offset;

        void Start()
        {
            offset = transform.position - target.position;
        }

        void Update()
        {
            transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.deltaTime * 2);
        }
    }
}