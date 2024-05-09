using UnityEngine;

public class StartLorPlay : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float speed = 2.0f;

    private Vector3 _currentTarget;

    private void Start()
    {
        _currentTarget = endPoint.position;
    }

    private void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _currentTarget, step);

        if (transform.position != _currentTarget)
        {
            return;
        }

        if (_currentTarget == startPoint.position)
        {
            _currentTarget = endPoint.position;
        } else
        {
            transform.position = startPoint.position;
        }

    }
}
